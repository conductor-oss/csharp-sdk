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
    /// <summary>
    /// Validates that the URI template (not the resolved path) is used for the
    /// http_api_client_request_seconds metric, and verifies various recording
    /// behaviors of MetricsCollector that go beyond the basic per-instrument
    /// tests in MetricsCollectorTests.
    /// </summary>
    public class MetricsRecordingTests : IDisposable
    {
        private readonly MetricsCollector _sut = new();
        private readonly MeterListener _listener = new();
        private readonly List<RecordedMeasurement> _recorded = new();

        public MetricsRecordingTests()
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

        public void Dispose()
        {
            _sut.Dispose();
            _listener.Dispose();
        }

        [Fact]
        public void HttpRequest_TemplateUri_UsedAsLabel()
        {
            _sut.RecordHttpApiClientRequest("GET", "/api/workflow/{workflowId}", "200", 0.05);

            var m = Assert.Single(_recorded);
            Assert.Equal("http_api_client_request_seconds", m.Name);
            AssertTag(m, "uri", "/api/workflow/{workflowId}");
        }

        [Fact]
        public void HttpRequest_TemplateUri_BoundedCardinality()
        {
            for (int i = 0; i < 100; i++)
                _sut.RecordHttpApiClientRequest("GET", "/api/workflow/{workflowId}", "200", 0.01);

            var distinctUris = _recorded
                .SelectMany(r => r.Tags.Where(t => t.Key == "uri"))
                .Select(t => (string)t.Value)
                .Distinct()
                .ToList();

            Assert.Single(distinctUris);
            Assert.Equal("/api/workflow/{workflowId}", distinctUris[0]);
        }

        [Fact]
        public void HttpRequest_MultipleEndpoints_DistinctSeries()
        {
            _sut.RecordHttpApiClientRequest("GET", "/api/workflow/{workflowId}", "200", 0.01);
            _sut.RecordHttpApiClientRequest("POST", "/api/tasks/{taskId}/ack", "200", 0.02);
            _sut.RecordHttpApiClientRequest("PUT", "/api/tasks", "500", 0.5);

            Assert.Equal(3, _recorded.Count);
            var uris = _recorded
                .SelectMany(r => r.Tags.Where(t => t.Key == "uri"))
                .Select(t => (string)t.Value)
                .ToList();

            Assert.Contains("/api/workflow/{workflowId}", uris);
            Assert.Contains("/api/tasks/{taskId}/ack", uris);
            Assert.Contains("/api/tasks", uris);
        }

        [Fact]
        public void HttpRequest_ErrorStatus_RecordedAsZero()
        {
            _sut.RecordHttpApiClientRequest("GET", "/api/workflow/{workflowId}", "0", 0.1);

            var m = Assert.Single(_recorded);
            AssertTag(m, "status", "0");
        }

        [Fact]
        public void TaskUpdateError_WithRealExceptionType()
        {
            _sut.RecordTaskUpdateError("my_task", "HttpRequestException");

            var m = Assert.Single(_recorded);
            Assert.Equal("task_update_error_total", m.Name);
            AssertTag(m, "exception", "HttpRequestException");
        }

        [Fact]
        public void ActiveWorkers_MultipleTaskTypes_Independent()
        {
            _sut.RecordActiveWorkers("task_a", 3);
            _sut.RecordActiveWorkers("task_b", 7);
            _sut.RecordActiveWorkers("task_a", 5);

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

            var taskA = gaugeValues.First(g =>
                g.Tags.Any(t => t.Key == "taskType" && (string)t.Value == "task_a"));
            Assert.Equal(5, (int)taskA.Value);

            var taskB = gaugeValues.First(g =>
                g.Tags.Any(t => t.Key == "taskType" && (string)t.Value == "task_b"));
            Assert.Equal(7, (int)taskB.Value);
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
