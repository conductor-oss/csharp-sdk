/*
 * Copyright 2024 Conductor Authors.
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on
 * an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the
 * specific language governing permissions and limitations under the License.
 */
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Conductor.Client.Telemetry;
using Xunit;

namespace Tests.Telemetry
{
    /// <summary>
    /// Integration test that starts a Prometheus HTTP listener, records metrics,
    /// scrapes the /metrics endpoint, and validates the exposition format.
    /// </summary>
    public class PrometheusOutputTests : IDisposable
    {
        private const int TestPort = 19991;
        private readonly string _meterName = $"Conductor.Client.Test.{Guid.NewGuid()}";
        private readonly MetricsCollector _metrics;
        private readonly HttpClient _http = new();

        public PrometheusOutputTests()
        {
            _metrics = new MetricsCollector(_meterName);
            _metrics.StartServer(TestPort);
        }

        public void Dispose()
        {
            _http.Dispose();
            _metrics.Dispose();
        }

        [Fact]
        public async Task CounterMetrics_HaveTotalSuffix()
        {
            _metrics.RecordTaskPoll("integration_task");
            _metrics.RecordTaskPollError("integration_task", "TimeoutException");
            _metrics.RecordTaskExecutionStarted("integration_task");

            var body = await ScrapeMetrics();

            Assert.Contains("task_poll_total", body);
            Assert.Contains("task_poll_error_total", body);
            Assert.Contains("task_execution_started_total", body);
        }

        [Fact]
        public async Task Labels_AreCamelCase()
        {
            _metrics.RecordTaskPoll("label_test_task");
            _metrics.RecordTaskPollError("label_test_task", "IOException");
            _metrics.RecordTaskPollTime("label_test_task", 0.05, "SUCCESS");

            var body = await ScrapeMetrics();

            Assert.Contains("taskType=\"label_test_task\"", body);
            Assert.Contains("exception=\"IOException\"", body);
            Assert.Contains("status=\"SUCCESS\"", body);

            Assert.DoesNotContain("task_type=", body);
            Assert.DoesNotContain("error_type=", body);
        }

        [Fact]
        public async Task TimeHistograms_HaveCanonicalBuckets()
        {
            _metrics.RecordTaskPollTime("bucket_task", 0.05, "SUCCESS");

            var body = await ScrapeMetrics();

            foreach (var bucket in MetricsCollector.CanonicalTimeBuckets)
            {
                var leStr = bucket.ToString("G");
                Assert.Contains($"le=\"{leStr}\"", body);
            }
            Assert.Contains("le=\"+Inf\"", body);
        }

        [Fact]
        public async Task SizeHistograms_HaveCanonicalBuckets()
        {
            _metrics.RecordTaskResultSize("size_task", 5000.0);

            var body = await ScrapeMetrics();

            foreach (var bucket in MetricsCollector.CanonicalSizeBuckets)
            {
                var leStr = bucket.ToString("G");
                Assert.Contains($"le=\"{leStr}\"", body);
            }
        }

        [Fact]
        public async Task HttpMetric_AppearsInOutput()
        {
            _metrics.RecordHttpApiClientRequest("GET", "/api/workflow/{workflowId}", "200", 0.042);

            var body = await ScrapeMetrics();

            Assert.Contains("http_api_client_request_seconds", body);
            Assert.Contains("method=\"GET\"", body);
            Assert.Contains("uri=\"/api/workflow/{workflowId}\"", body);
            Assert.Contains("status=\"200\"", body);
        }

        [Fact]
        public async Task OtelScopeName_IsPresent()
        {
            _metrics.RecordTaskPoll("scope_task");

            var body = await ScrapeMetrics();

            Assert.Contains($"otel_scope_name=\"{_meterName}\"", body);
        }

        [Fact]
        public async Task AllCanonicalCounterNames_AreRegistered()
        {
            _metrics.RecordTaskPoll("full");
            _metrics.RecordTaskExecutionStarted("full");
            _metrics.RecordTaskPollError("full", "E");
            _metrics.RecordTaskExecuteError("full", "E");
            _metrics.RecordTaskUpdateError("full", "E");
            _metrics.RecordTaskAckError("full", "E");
            _metrics.RecordTaskAckFailed("full");
            _metrics.RecordTaskPaused("full");
            _metrics.RecordTaskExecutionQueueFull("full");
            _metrics.RecordUncaughtException("E");
            _metrics.RecordWorkflowStartError("wf", "E");
            _metrics.RecordExternalPayloadUsed("e", "READ", "TASK_INPUT");

            var body = await ScrapeMetrics();

            var expectedCounters = new[]
            {
                "task_poll_total",
                "task_execution_started_total",
                "task_poll_error_total",
                "task_execute_error_total",
                "task_update_error_total",
                "task_ack_error_total",
                "task_ack_failed_total",
                "task_paused_total",
                "task_execution_queue_full_total",
                "thread_uncaught_exceptions_total",
                "workflow_start_error_total",
                "external_payload_used_total",
            };

            foreach (var name in expectedCounters)
            {
                Assert.True(body.Contains(name),
                    $"Expected counter '{name}' not found in Prometheus output");
            }
        }

        private async Task<string> ScrapeMetrics()
        {
            // Allow a brief collection interval for OTel to flush
            await Task.Delay(200);
            return await _http.GetStringAsync($"http://localhost:{TestPort}/metrics");
        }
    }
}
