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
using Conductor.Client.Telemetry;
using Xunit;

namespace Tests.Telemetry
{
    public class MetricsCollectorTests : IDisposable
    {
        private readonly MetricsCollector _sut = new();
        private readonly MeterListener _listener = new();
        private readonly List<RecordedMeasurement> _recorded = new();

        public MetricsCollectorTests()
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

            _listener.SetMeasurementEventCallback<int>((instrument, value, tags, _) =>
                _recorded.Add(new RecordedMeasurement(instrument.Name, value, tags.ToArray())));

            _listener.Start();
        }

        public void Dispose() => _listener.Dispose();

        // ---------------------------------------------------------------
        // Counters
        // ---------------------------------------------------------------

        [Fact]
        public void RecordTaskPoll_EmitsCounterWithTaskType()
        {
            _sut.RecordTaskPoll("my_task");

            var m = Assert.Single(_recorded);
            Assert.Equal("task_poll_total", m.Name);
            Assert.Equal(1L, m.Value);
            AssertTag(m, "task_type", "my_task");
        }

        [Fact]
        public void RecordTaskPollError_EmitsCounterWithBothTags()
        {
            _sut.RecordTaskPollError("my_task", "TimeoutException");

            var m = Assert.Single(_recorded);
            Assert.Equal("task_poll_error_total", m.Name);
            AssertTag(m, "task_type", "my_task");
            AssertTag(m, "error_type", "TimeoutException");
        }

        [Fact]
        public void RecordTaskExecuteError_EmitsCounterWithBothTags()
        {
            _sut.RecordTaskExecuteError("task_a", "NullReferenceException");

            var m = Assert.Single(_recorded);
            Assert.Equal("task_execute_error_total", m.Name);
            AssertTag(m, "task_type", "task_a");
            AssertTag(m, "error_type", "NullReferenceException");
        }

        [Fact]
        public void RecordTaskUpdateError_EmitsCounter()
        {
            _sut.RecordTaskUpdateError("task_b");

            var m = Assert.Single(_recorded);
            Assert.Equal("task_update_error_total", m.Name);
            AssertTag(m, "task_type", "task_b");
        }

        [Fact]
        public void RecordTaskPaused_EmitsCounter()
        {
            _sut.RecordTaskPaused("paused_task");

            var m = Assert.Single(_recorded);
            Assert.Equal("task_paused_total", m.Name);
            AssertTag(m, "task_type", "paused_task");
        }

        [Fact]
        public void RecordTaskExecutionQueueFull_EmitsCounter()
        {
            _sut.RecordTaskExecutionQueueFull("busy_task");

            var m = Assert.Single(_recorded);
            Assert.Equal("task_execution_queue_full_total", m.Name);
            AssertTag(m, "task_type", "busy_task");
        }

        [Fact]
        public void RecordUncaughtException_EmitsCounterWithNoTags()
        {
            _sut.RecordUncaughtException();

            var m = Assert.Single(_recorded);
            Assert.Equal("thread_uncaught_exceptions_total", m.Name);
            Assert.Equal(1L, m.Value);
        }

        [Fact]
        public void RecordWorkflowStartError_EmitsCounter()
        {
            _sut.RecordWorkflowStartError("my_workflow");

            var m = Assert.Single(_recorded);
            Assert.Equal("workflow_start_error_total", m.Name);
            AssertTag(m, "workflow_type", "my_workflow");
        }

        [Fact]
        public void RecordExternalPayloadUsed_EmitsCounterWithThreeTags()
        {
            _sut.RecordExternalPayloadUsed("entity", "read", "input");

            var m = Assert.Single(_recorded);
            Assert.Equal("external_payload_used_total", m.Name);
            AssertTag(m, "entity_name", "entity");
            AssertTag(m, "operation", "read");
            AssertTag(m, "payload_type", "input");
        }

        // ---------------------------------------------------------------
        // Histograms
        // ---------------------------------------------------------------

        [Fact]
        public void RecordTaskPollTime_RecordsDuration()
        {
            _sut.RecordTaskPollTime("fast_task", 0.123);

            var m = Assert.Single(_recorded);
            Assert.Equal("task_poll_time_seconds", m.Name);
            Assert.Equal(0.123, m.Value);
            AssertTag(m, "task_type", "fast_task");
        }

        [Fact]
        public void RecordTaskExecuteTime_RecordsDuration()
        {
            _sut.RecordTaskExecuteTime("slow_task", 5.5);

            var m = Assert.Single(_recorded);
            Assert.Equal("task_execute_time_seconds", m.Name);
            Assert.Equal(5.5, m.Value);
            AssertTag(m, "task_type", "slow_task");
        }

        [Fact]
        public void RecordTaskUpdateTime_RecordsDuration()
        {
            _sut.RecordTaskUpdateTime("task_c", 0.05);

            var m = Assert.Single(_recorded);
            Assert.Equal("task_update_time_seconds", m.Name);
            Assert.Equal(0.05, m.Value);
            AssertTag(m, "task_type", "task_c");
        }

        [Fact]
        public void RecordTaskResultSize_RecordsBytes()
        {
            _sut.RecordTaskResultSize("task_d", 1024.0);

            var m = Assert.Single(_recorded);
            Assert.Equal("task_result_size_bytes", m.Name);
            Assert.Equal(1024.0, m.Value);
            AssertTag(m, "task_type", "task_d");
        }

        [Fact]
        public void RecordWorkflowInputSize_RecordsBytesWithVersionTag()
        {
            _sut.RecordWorkflowInputSize("wf_type", "v2", 2048.0);

            var m = Assert.Single(_recorded);
            Assert.Equal("workflow_input_size_bytes", m.Name);
            Assert.Equal(2048.0, m.Value);
            AssertTag(m, "workflow_type", "wf_type");
            AssertTag(m, "version", "v2");
        }

        // ---------------------------------------------------------------
        // Observable gauge
        // ---------------------------------------------------------------

        [Fact]
        public void RecordActiveWorkers_ExposedViaObservableGauge()
        {
            _sut.RecordActiveWorkers("task_x", 3);
            _sut.RecordActiveWorkers("task_y", 7);

            var gaugeValues = new List<RecordedMeasurement>();
            using var gaugeListener = new MeterListener();
            gaugeListener.InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Meter.Name == MetricsCollector.MeterName
                    && instrument.Name == "active_workers")
                    l.EnableMeasurementEvents(instrument);
            };
            gaugeListener.SetMeasurementEventCallback<int>((instrument, value, tags, _) =>
                gaugeValues.Add(new RecordedMeasurement(instrument.Name, value, tags.ToArray())));
            gaugeListener.Start();
            gaugeListener.RecordObservableInstruments();

            Assert.Equal(2, gaugeValues.Count);
            Assert.Contains(gaugeValues, g =>
                (int)g.Value == 3 && g.Tags.Any(t => t.Key == "task_type" && (string)t.Value == "task_x"));
            Assert.Contains(gaugeValues, g =>
                (int)g.Value == 7 && g.Tags.Any(t => t.Key == "task_type" && (string)t.Value == "task_y"));
        }

        [Fact]
        public void RecordActiveWorkers_OverwritesPreviousValue()
        {
            _sut.RecordActiveWorkers("overwrite_test_task", 3);
            _sut.RecordActiveWorkers("overwrite_test_task", 10);

            var gaugeValues = new List<RecordedMeasurement>();
            using var gaugeListener = new MeterListener();
            gaugeListener.InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Meter.Name == MetricsCollector.MeterName
                    && instrument.Name == "active_workers")
                    l.EnableMeasurementEvents(instrument);
            };
            gaugeListener.SetMeasurementEventCallback<int>((instrument, value, tags, _) =>
                gaugeValues.Add(new RecordedMeasurement(instrument.Name, value, tags.ToArray())));
            gaugeListener.Start();
            gaugeListener.RecordObservableInstruments();

            var match = gaugeValues.Where(g =>
                g.Tags.Any(t => t.Key == "task_type" && (string)t.Value == "overwrite_test_task")).ToList();
            var single = Assert.Single(match);
            Assert.Equal(10, (int)single.Value);
        }

        // ---------------------------------------------------------------
        // Multiple increments
        // ---------------------------------------------------------------

        [Fact]
        public void Counters_AccumulateAcrossMultipleCalls()
        {
            _sut.RecordTaskPoll("task_a");
            _sut.RecordTaskPoll("task_a");
            _sut.RecordTaskPoll("task_b");

            Assert.Equal(3, _recorded.Count);
            Assert.Equal(2, _recorded.Count(r => r.Tags.Any(t => (string)t.Value == "task_a")));
            Assert.Equal(1, _recorded.Count(r => r.Tags.Any(t => (string)t.Value == "task_b")));
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

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
