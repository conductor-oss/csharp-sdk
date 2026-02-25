using Conductor.Client.Telemetry;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using Xunit;

namespace Tests.Unit.Telemetry
{
    public class ConductorMetricsTests
    {
        [Fact]
        public void MeterName_IsCorrect()
        {
            Assert.Equal("Conductor.Client", ConductorMetrics.MeterName);
        }

        [Fact]
        public void TaskPollCount_IsNotNull()
        {
            Assert.NotNull(ConductorMetrics.TaskPollCount);
        }

        [Fact]
        public void TaskPollSuccessCount_IsNotNull()
        {
            Assert.NotNull(ConductorMetrics.TaskPollSuccessCount);
        }

        [Fact]
        public void TaskPollEmptyCount_IsNotNull()
        {
            Assert.NotNull(ConductorMetrics.TaskPollEmptyCount);
        }

        [Fact]
        public void TaskPollErrorCount_IsNotNull()
        {
            Assert.NotNull(ConductorMetrics.TaskPollErrorCount);
        }

        [Fact]
        public void TaskPollLatency_IsNotNull()
        {
            Assert.NotNull(ConductorMetrics.TaskPollLatency);
        }

        [Fact]
        public void TaskExecutionCount_IsNotNull()
        {
            Assert.NotNull(ConductorMetrics.TaskExecutionCount);
        }

        [Fact]
        public void TaskExecutionSuccessCount_IsNotNull()
        {
            Assert.NotNull(ConductorMetrics.TaskExecutionSuccessCount);
        }

        [Fact]
        public void TaskExecutionErrorCount_IsNotNull()
        {
            Assert.NotNull(ConductorMetrics.TaskExecutionErrorCount);
        }

        [Fact]
        public void TaskExecutionLatency_IsNotNull()
        {
            Assert.NotNull(ConductorMetrics.TaskExecutionLatency);
        }

        [Fact]
        public void TaskUpdateCount_IsNotNull()
        {
            Assert.NotNull(ConductorMetrics.TaskUpdateCount);
        }

        [Fact]
        public void TaskUpdateErrorCount_IsNotNull()
        {
            Assert.NotNull(ConductorMetrics.TaskUpdateErrorCount);
        }

        [Fact]
        public void TaskUpdateRetryCount_IsNotNull()
        {
            Assert.NotNull(ConductorMetrics.TaskUpdateRetryCount);
        }

        [Fact]
        public void TaskUpdateLatency_IsNotNull()
        {
            Assert.NotNull(ConductorMetrics.TaskUpdateLatency);
        }

        [Fact]
        public void WorkerRestartCount_IsNotNull()
        {
            Assert.NotNull(ConductorMetrics.WorkerRestartCount);
        }

        [Fact]
        public void PayloadSize_IsNotNull()
        {
            Assert.NotNull(ConductorMetrics.PayloadSize);
        }

        [Fact]
        public void ApiCallCount_IsNotNull()
        {
            Assert.NotNull(ConductorMetrics.ApiCallCount);
        }

        [Fact]
        public void ApiErrorCount_IsNotNull()
        {
            Assert.NotNull(ConductorMetrics.ApiErrorCount);
        }

        [Fact]
        public void ApiLatency_IsNotNull()
        {
            Assert.NotNull(ConductorMetrics.ApiLatency);
        }

        [Fact]
        public void TimingScope_RecordsLatency()
        {
            // Verify TimingScope can be created and disposed without error
            using (var scope = ConductorMetrics.Time(ConductorMetrics.TaskExecutionLatency))
            {
                // Simulate some work
                System.Threading.Thread.Sleep(1);
            }
        }

        [Fact]
        public void TimingScope_WithTags_RecordsLatency()
        {
            var tags = new[] { new KeyValuePair<string, object>("taskType", "test_task") };
            using (var scope = ConductorMetrics.Time(ConductorMetrics.TaskPollLatency, tags))
            {
                System.Threading.Thread.Sleep(1);
            }
        }

        [Fact]
        public void Counters_CanBeIncremented()
        {
            // These should not throw
            ConductorMetrics.TaskPollCount.Add(1);
            ConductorMetrics.TaskExecutionCount.Add(1);
            ConductorMetrics.ApiCallCount.Add(1);
        }

        [Fact]
        public void Counters_CanBeIncrementedWithTags()
        {
            ConductorMetrics.TaskPollCount.Add(1, new KeyValuePair<string, object>("taskType", "test_task"));
            ConductorMetrics.ApiCallCount.Add(1, new KeyValuePair<string, object>("endpoint", "/api/tasks/poll"));
        }

        [Fact]
        public void Histograms_CanRecord()
        {
            ConductorMetrics.TaskPollLatency.Record(15.5);
            ConductorMetrics.TaskExecutionLatency.Record(150.0);
            ConductorMetrics.PayloadSize.Record(1024.0);
        }

        [Fact]
        public void MetricsCanBeCollectedViaListener()
        {
            // Use a MeterListener to verify metrics are being emitted
            var measurements = new List<double>();
            using (var listener = new MeterListener())
            {
                listener.InstrumentPublished = (instrument, meterListener) =>
                {
                    if (instrument.Meter.Name == ConductorMetrics.MeterName && instrument.Name == "conductor.worker.task_poll_latency_ms")
                    {
                        meterListener.EnableMeasurementEvents(instrument);
                    }
                };
                listener.SetMeasurementEventCallback<double>((instrument, measurement, tags, state) =>
                {
                    measurements.Add(measurement);
                });
                listener.Start();

                ConductorMetrics.TaskPollLatency.Record(42.5);

                listener.RecordObservableInstruments();
            }

            Assert.Contains(42.5, measurements);
        }
    }
}
