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
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using Conductor.Client.Models;
using Conductor.Client.Telemetry;
using Conductor.Executor;
using Xunit;

namespace Tests.Executor
{
    /// <summary>
    /// Tests that WorkflowExecutor correctly records workflow_input_size_bytes
    /// and workflow_start_error_total via the MetricsCollector.
    ///
    /// Because WorkflowResourceApi.StartWorkflow is non-virtual and makes a
    /// real HTTP call, these tests exercise the two recording helper paths
    /// (RecordInputSize and the catch block) by constructing a WorkflowExecutor
    /// whose underlying API client will fail.  The input-size metric is
    /// recorded *before* the API call, so it can be validated even when the
    /// call fails.  The error metric is validated by the same failing call.
    /// </summary>
    public class WorkflowExecutorMetricsTests : IDisposable
    {
        private readonly MetricsCollector _metrics = new();
        private readonly MeterListener _listener = new();
        private readonly List<RecordedMeasurement> _recorded = new();

        public WorkflowExecutorMetricsTests()
        {
            _listener.InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == MetricsCollector.MeterName)
                    listener.EnableMeasurementEvents(instrument);
            };

            _listener.SetMeasurementEventCallback<long>((instrument, value, tags, _) =>
                _recorded.Add(new RecordedMeasurement(instrument.Name, value, tags.ToArray())));
            _listener.SetMeasurementEventCallback<double>((instrument, value, tags, _) =>
                _recorded.Add(new RecordedMeasurement(instrument.Name, value, tags.ToArray())));

            _listener.Start();
        }

        public void Dispose()
        {
            _metrics.Dispose();
            _listener.Dispose();
        }

        [Fact]
        public void StartWorkflow_RecordsInputSize_BeforeApiCall()
        {
            var executor = new WorkflowExecutor(
                new Conductor.Client.Configuration { BasePath = "http://localhost:1" },
                _metrics);

            var request = new StartWorkflowRequest(name: "test_workflow", version: 1)
            {
                Input = new Dictionary<string, object>
                {
                    { "key1", "value1" },
                    { "key2", 42 }
                }
            };

            try { executor.StartWorkflow(request); }
            catch { /* API call to localhost:1 will fail */ }

            var sizeMetric = _recorded.FirstOrDefault(r => r.Name == "workflow_input_size_bytes");
            Assert.NotNull(sizeMetric);
            Assert.True((double)sizeMetric.Value > 0, "Input size should be > 0");
            AssertTag(sizeMetric, "workflowType", "test_workflow");
            AssertTag(sizeMetric, "version", "1");
        }

        [Fact]
        public void StartWorkflow_RecordsZeroSize_WhenNoInput()
        {
            var executor = new WorkflowExecutor(
                new Conductor.Client.Configuration { BasePath = "http://localhost:1" },
                _metrics);

            var request = new StartWorkflowRequest(name: "empty_workflow", version: 2);

            try { executor.StartWorkflow(request); }
            catch { }

            var sizeMetric = _recorded.FirstOrDefault(r => r.Name == "workflow_input_size_bytes");
            Assert.NotNull(sizeMetric);
            Assert.Equal(0.0, (double)sizeMetric.Value);
            AssertTag(sizeMetric, "workflowType", "empty_workflow");
            AssertTag(sizeMetric, "version", "2");
        }

        [Fact]
        public void StartWorkflow_RecordsWorkflowStartError_OnException()
        {
            var executor = new WorkflowExecutor(
                new Conductor.Client.Configuration { BasePath = "http://localhost:1" },
                _metrics);

            var request = new StartWorkflowRequest(name: "failing_workflow", version: 1);

            Assert.ThrowsAny<Exception>(() => executor.StartWorkflow(request));

            var errorMetric = _recorded.FirstOrDefault(r => r.Name == "workflow_start_error_total");
            Assert.NotNull(errorMetric);
            AssertTag(errorMetric, "workflowType", "failing_workflow");
            var exTag = errorMetric.Tags.FirstOrDefault(t => t.Key == "exception");
            Assert.NotNull(exTag.Value);
            Assert.NotEmpty((string)exTag.Value);
        }

        [Fact]
        public void StartWorkflow_NullName_RecordsEmptyString()
        {
            var executor = new WorkflowExecutor(
                new Conductor.Client.Configuration { BasePath = "http://localhost:1" },
                _metrics);

            var request = new StartWorkflowRequest(name: null, version: null);

            try { executor.StartWorkflow(request); }
            catch { }

            var sizeMetric = _recorded.FirstOrDefault(r => r.Name == "workflow_input_size_bytes");
            Assert.NotNull(sizeMetric);
            AssertTag(sizeMetric, "workflowType", "");
            AssertTag(sizeMetric, "version", "");
        }

        private static void AssertTag(RecordedMeasurement measurement, string key, string expectedValue)
        {
            var tag = measurement.Tags.FirstOrDefault(t => t.Key == key);
            Assert.Equal(expectedValue, (string)tag.Value);
        }

        private record RecordedMeasurement(
            string Name,
            object Value,
            KeyValuePair<string, object>[] Tags);
    }
}
