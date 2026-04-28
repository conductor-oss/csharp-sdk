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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using OpenTelemetry;
using OpenTelemetry.Metrics;

namespace Conductor.Client.Telemetry
{
    /// <summary>
    /// Instruments the Conductor worker poll-execute-update loop with
    /// <see cref="System.Diagnostics.Metrics"/> counters, histograms, and gauges.
    ///
    /// The <see cref="Meter"/> is named <c>Conductor.Client</c>. Call
    /// <see cref="StartServer(int)"/> to expose a Prometheus-compatible
    /// <c>/metrics</c> endpoint with the canonical bucket configuration,
    /// or attach your own <see cref="MeterProvider"/> using
    /// <see cref="CanonicalTimeBuckets"/>.
    /// </summary>
    public sealed class MetricsCollector : IDisposable
    {
        public const string MeterName = "Conductor.Client";

        /// <summary>
        /// Canonical time histogram buckets shared across all Conductor SDKs.
        /// </summary>
        public static readonly double[] CanonicalTimeBuckets =
            { 0.001, 0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10 };

        /// <summary>
        /// Proposed size histogram buckets (in bytes); if accepted, will be shared across all Conductor SDKs.
        /// </summary>
        public static readonly double[] CanonicalSizeBuckets =
            { 100, 1_000, 10_000, 100_000, 1_000_000, 10_000_000 };

        private readonly Meter _meter;
        private MeterProvider _meterProvider;

        // --- counters ---
        private readonly Counter<long> _taskPollTotal;
        private readonly Counter<long> _taskExecutionStartedTotal;
        private readonly Counter<long> _taskPollErrorTotal;
        private readonly Counter<long> _taskExecuteErrorTotal;
        private readonly Counter<long> _taskUpdateErrorTotal;
        private readonly Counter<long> _taskPausedTotal;
        private readonly Counter<long> _taskExecutionQueueFullTotal;
        private readonly Counter<long> _threadUncaughtExceptionsTotal;
        private readonly Counter<long> _workflowStartErrorTotal;
        private readonly Counter<long> _externalPayloadUsedTotal;
        private readonly Counter<long> _taskAckErrorTotal;
        private readonly Counter<long> _taskAckFailedTotal;

        // --- histograms ---
        private readonly Histogram<double> _taskPollTimeSeconds;
        private readonly Histogram<double> _taskExecuteTimeSeconds;
        private readonly Histogram<double> _taskUpdateTimeSeconds;
        private readonly Histogram<double> _httpApiClientRequestSeconds;
        private readonly Histogram<double> _taskResultSizeBytesHistogram;
        private readonly Histogram<double> _workflowInputSizeBytesHistogram;

        // --- gauges ---
        private readonly ConcurrentDictionary<string, int> _activeWorkerCounts = new ConcurrentDictionary<string, int>();
        private readonly ConcurrentDictionary<string, double> _taskResultSizes = new ConcurrentDictionary<string, double>();
        private readonly ConcurrentDictionary<string, double> _workflowInputSizes = new ConcurrentDictionary<string, double>();

        public MetricsCollector()
        {
            _meter = new Meter(MeterName);

            // --- counters ---

            _taskPollTotal = _meter.CreateCounter<long>(
                "task_poll_total",
                description: "Total number of task poll attempts");

            _taskExecutionStartedTotal = _meter.CreateCounter<long>(
                "task_execution_started_total",
                description: "Tasks dispatched to the worker function");

            _taskPollErrorTotal = _meter.CreateCounter<long>(
                "task_poll_error_total",
                description: "Total number of task poll errors");

            _taskExecuteErrorTotal = _meter.CreateCounter<long>(
                "task_execute_error_total",
                description: "Total number of task execution errors");

            _taskUpdateErrorTotal = _meter.CreateCounter<long>(
                "task_update_error_total",
                description: "Total number of task update errors");

            _taskPausedTotal = _meter.CreateCounter<long>(
                "task_paused_total",
                description: "Polls skipped because worker is paused");

            _taskExecutionQueueFullTotal = _meter.CreateCounter<long>(
                "task_execution_queue_full_total",
                description: "Polls returning zero capacity (all workers busy)");

            _threadUncaughtExceptionsTotal = _meter.CreateCounter<long>(
                "thread_uncaught_exceptions_total",
                description: "Uncaught exceptions in worker threads");

            _workflowStartErrorTotal = _meter.CreateCounter<long>(
                "workflow_start_error_total",
                description: "Errors starting workflows");

            _externalPayloadUsedTotal = _meter.CreateCounter<long>(
                "external_payload_used_total",
                description: "External payload storage usage");

            _taskAckErrorTotal = _meter.CreateCounter<long>(
                "task_ack_error_total",
                description: "Task ack client-side errors");

            _taskAckFailedTotal = _meter.CreateCounter<long>(
                "task_ack_failed_total",
                description: "Task ack declined by server");

            // --- time histograms ---

            _taskPollTimeSeconds = _meter.CreateHistogram<double>(
                "task_poll_time_seconds",
                unit: "s",
                description: "Task poll round-trip duration in seconds");

            _taskExecuteTimeSeconds = _meter.CreateHistogram<double>(
                "task_execute_time_seconds",
                unit: "s",
                description: "Task execution duration in seconds");

            _taskUpdateTimeSeconds = _meter.CreateHistogram<double>(
                "task_update_time_seconds",
                unit: "s",
                description: "Task result update duration in seconds");

            _httpApiClientRequestSeconds = _meter.CreateHistogram<double>(
                "http_api_client_request_seconds",
                unit: "s",
                description: "HTTP API client request duration in seconds");

            // --- size histograms (non-canonical names while spec uses gauges) ---

            _taskResultSizeBytesHistogram = _meter.CreateHistogram<double>(
                "task_result_size_bytes_histogram",
                unit: "By",
                description: "Distribution of task result payload sizes in bytes");

            _workflowInputSizeBytesHistogram = _meter.CreateHistogram<double>(
                "workflow_input_size_bytes_histogram",
                unit: "By",
                description: "Distribution of workflow input payload sizes in bytes");

            // --- canonical size gauges ---

            _meter.CreateObservableGauge(
                "task_result_size_bytes",
                observeValues: () =>
                {
                    var measurements = new List<Measurement<double>>();
                    foreach (var kvp in _taskResultSizes)
                    {
                        measurements.Add(new Measurement<double>(
                            kvp.Value,
                            new KeyValuePair<string, object>("taskType", kvp.Key)));
                    }
                    return measurements;
                },
                unit: "By",
                description: "Size of task result payload in bytes");

            _meter.CreateObservableGauge(
                "workflow_input_size_bytes",
                observeValues: () =>
                {
                    var measurements = new List<Measurement<double>>();
                    foreach (var kvp in _workflowInputSizes)
                    {
                        var parts = kvp.Key.Split('\0');
                        var wfType = parts[0];
                        var version = parts.Length > 1 ? parts[1] : "";
                        measurements.Add(new Measurement<double>(
                            kvp.Value,
                            new KeyValuePair<string, object>("workflowType", wfType),
                            new KeyValuePair<string, object>("version", version)));
                    }
                    return measurements;
                },
                unit: "By",
                description: "Size of workflow input payload in bytes");

            // --- utilization gauge ---

            _meter.CreateObservableGauge(
                "active_workers",
                observeValues: () =>
                {
                    var measurements = new List<Measurement<int>>();
                    foreach (var kvp in _activeWorkerCounts)
                    {
                        measurements.Add(new Measurement<int>(
                            kvp.Value,
                            new KeyValuePair<string, object>("taskType", kvp.Key)));
                    }
                    return measurements;
                },
                description: "Number of workers currently executing tasks");
        }

        // ---------------------------------------------------------------
        // Built-in Prometheus server
        // ---------------------------------------------------------------

        /// <summary>
        /// Starts an OpenTelemetry Prometheus HTTP listener on the given port
        /// with the canonical bucket configuration for all histograms.
        /// </summary>
        public void StartServer(int port)
        {
            if (_meterProvider != null)
                throw new InvalidOperationException("Metrics server already started");

            _meterProvider = Sdk.CreateMeterProviderBuilder()
                .AddMeter(MeterName)
                .AddView("task_poll_time_seconds", new ExplicitBucketHistogramConfiguration { Boundaries = CanonicalTimeBuckets })
                .AddView("task_execute_time_seconds", new ExplicitBucketHistogramConfiguration { Boundaries = CanonicalTimeBuckets })
                .AddView("task_update_time_seconds", new ExplicitBucketHistogramConfiguration { Boundaries = CanonicalTimeBuckets })
                .AddView("http_api_client_request_seconds", new ExplicitBucketHistogramConfiguration { Boundaries = CanonicalTimeBuckets })
                .AddView("task_result_size_bytes_histogram", new ExplicitBucketHistogramConfiguration { Boundaries = CanonicalSizeBuckets })
                .AddView("workflow_input_size_bytes_histogram", new ExplicitBucketHistogramConfiguration { Boundaries = CanonicalSizeBuckets })
                .AddPrometheusHttpListener(options =>
                {
                    options.UriPrefixes = new[] { $"http://*:{port}/" };
                })
                .Build();
        }

        public void Dispose()
        {
            _meterProvider?.Dispose();
            _meter?.Dispose();
        }

        // ---------------------------------------------------------------
        // Poll
        // ---------------------------------------------------------------

        public void RecordTaskPoll(string taskType)
        {
            _taskPollTotal.Add(1,
                new KeyValuePair<string, object>("taskType", taskType));
        }

        public void RecordTaskPollTime(string taskType, double durationSeconds, string status)
        {
            _taskPollTimeSeconds.Record(durationSeconds,
                new KeyValuePair<string, object>("taskType", taskType),
                new KeyValuePair<string, object>("status", status));
        }

        public void RecordTaskPollError(string taskType, string exceptionType)
        {
            _taskPollErrorTotal.Add(1,
                new KeyValuePair<string, object>("taskType", taskType),
                new KeyValuePair<string, object>("exception", exceptionType));
        }

        // ---------------------------------------------------------------
        // Execution
        // ---------------------------------------------------------------

        public void RecordTaskExecutionStarted(string taskType)
        {
            _taskExecutionStartedTotal.Add(1,
                new KeyValuePair<string, object>("taskType", taskType));
        }

        public void RecordTaskExecuteTime(string taskType, double durationSeconds, string status)
        {
            _taskExecuteTimeSeconds.Record(durationSeconds,
                new KeyValuePair<string, object>("taskType", taskType),
                new KeyValuePair<string, object>("status", status));
        }

        public void RecordTaskExecuteError(string taskType, string exceptionType)
        {
            _taskExecuteErrorTotal.Add(1,
                new KeyValuePair<string, object>("taskType", taskType),
                new KeyValuePair<string, object>("exception", exceptionType));
        }

        // ---------------------------------------------------------------
        // Update
        // ---------------------------------------------------------------

        public void RecordTaskUpdateTime(string taskType, double durationSeconds, string status)
        {
            _taskUpdateTimeSeconds.Record(durationSeconds,
                new KeyValuePair<string, object>("taskType", taskType),
                new KeyValuePair<string, object>("status", status));
        }

        public void RecordTaskUpdateError(string taskType, string exceptionType)
        {
            _taskUpdateErrorTotal.Add(1,
                new KeyValuePair<string, object>("taskType", taskType),
                new KeyValuePair<string, object>("exception", exceptionType));
        }

        // ---------------------------------------------------------------
        // Sizes
        // ---------------------------------------------------------------

        public void RecordTaskResultSize(string taskType, double sizeBytes)
        {
            _taskResultSizes[taskType] = sizeBytes;
            _taskResultSizeBytesHistogram.Record(sizeBytes,
                new KeyValuePair<string, object>("taskType", taskType));
        }

        public void RecordWorkflowInputSize(string workflowType, string version, double sizeBytes)
        {
            _workflowInputSizes[workflowType + "\0" + (version ?? "")] = sizeBytes;
            _workflowInputSizeBytesHistogram.Record(sizeBytes,
                new KeyValuePair<string, object>("workflowType", workflowType),
                new KeyValuePair<string, object>("version", version ?? ""));
        }

        // ---------------------------------------------------------------
        // HTTP API client
        // ---------------------------------------------------------------

        public void RecordHttpApiClientRequest(string method, string uri, string status, double durationSeconds)
        {
            _httpApiClientRequestSeconds.Record(durationSeconds,
                new KeyValuePair<string, object>("method", method),
                new KeyValuePair<string, object>("uri", uri),
                new KeyValuePair<string, object>("status", status));
        }

        // ---------------------------------------------------------------
        // Queue / capacity
        // ---------------------------------------------------------------

        public void RecordTaskExecutionQueueFull(string taskType)
        {
            _taskExecutionQueueFullTotal.Add(1,
                new KeyValuePair<string, object>("taskType", taskType));
        }

        public void RecordTaskPaused(string taskType)
        {
            _taskPausedTotal.Add(1,
                new KeyValuePair<string, object>("taskType", taskType));
        }

        // ---------------------------------------------------------------
        // Active workers (observable gauge snapshot)
        // ---------------------------------------------------------------

        public void RecordActiveWorkers(string taskType, int count)
        {
            _activeWorkerCounts[taskType] = count;
        }

        // ---------------------------------------------------------------
        // Uncaught exceptions
        // ---------------------------------------------------------------

        public void RecordUncaughtException(string exceptionType)
        {
            _threadUncaughtExceptionsTotal.Add(1,
                new KeyValuePair<string, object>("exception", exceptionType));
        }

        // ---------------------------------------------------------------
        // Workflow
        // ---------------------------------------------------------------

        public void RecordWorkflowStartError(string workflowType, string exceptionType)
        {
            _workflowStartErrorTotal.Add(1,
                new KeyValuePair<string, object>("workflowType", workflowType),
                new KeyValuePair<string, object>("exception", exceptionType));
        }

        // ---------------------------------------------------------------
        // External payload
        // ---------------------------------------------------------------

        public void RecordExternalPayloadUsed(string entityName, string operation, string payloadType)
        {
            _externalPayloadUsedTotal.Add(1,
                new KeyValuePair<string, object>("entityName", entityName),
                new KeyValuePair<string, object>("operation", operation),
                new KeyValuePair<string, object>("payloadType", payloadType));
        }

        // ---------------------------------------------------------------
        // Surface-only ack counters (internal runner never increments)
        // ---------------------------------------------------------------

        public void RecordTaskAckError(string taskType, string exceptionType)
        {
            _taskAckErrorTotal.Add(1,
                new KeyValuePair<string, object>("taskType", taskType),
                new KeyValuePair<string, object>("exception", exceptionType));
        }

        public void RecordTaskAckFailed(string taskType)
        {
            _taskAckFailedTotal.Add(1,
                new KeyValuePair<string, object>("taskType", taskType));
        }
    }
}
