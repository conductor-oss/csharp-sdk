using Conductor.Client.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests.Unit.Worker
{
    public class WorkflowTaskMonitorTests
    {
        private readonly WorkflowTaskMonitor _monitor;
        private readonly Mock<ILogger<WorkflowTaskMonitor>> _loggerMock;

        public WorkflowTaskMonitorTests()
        {
            _loggerMock = new Mock<ILogger<WorkflowTaskMonitor>>();
            _monitor = new WorkflowTaskMonitor(_loggerMock.Object, maxConsecutiveErrors: 3);
        }

        [Fact]
        public void IncrementAndGetRunningWorkers()
        {
            Assert.Equal(0, _monitor.GetRunningWorkers());

            _monitor.IncrementRunningWorker();
            Assert.Equal(1, _monitor.GetRunningWorkers());

            _monitor.IncrementRunningWorker();
            Assert.Equal(2, _monitor.GetRunningWorkers());
        }

        [Fact]
        public void RunningWorkerDone_DecrementsCounter()
        {
            _monitor.IncrementRunningWorker();
            _monitor.IncrementRunningWorker();
            _monitor.RunningWorkerDone();

            Assert.Equal(1, _monitor.GetRunningWorkers());
        }

        [Fact]
        public void IsHealthy_TrueByDefault()
        {
            Assert.True(_monitor.IsHealthy());
        }

        [Fact]
        public void IsHealthy_FalseAfterMaxConsecutiveErrors()
        {
            _monitor.RecordPollError();
            Assert.True(_monitor.IsHealthy()); // 1 error < 3 max

            _monitor.RecordPollError();
            Assert.True(_monitor.IsHealthy()); // 2 errors < 3 max

            _monitor.RecordPollError();
            Assert.False(_monitor.IsHealthy()); // 3 errors = 3 max
        }

        [Fact]
        public void RecordPollSuccess_ResetsConsecutiveErrors()
        {
            _monitor.RecordPollError();
            _monitor.RecordPollError();
            _monitor.RecordPollSuccess(1);

            Assert.True(_monitor.IsHealthy());
        }

        [Fact]
        public void RecordTaskSuccess_IncrementsTotalProcessed()
        {
            _monitor.RecordTaskSuccess();
            _monitor.RecordTaskSuccess();

            var status = _monitor.GetHealthStatus();
            Assert.Equal(2, status.TotalTasksProcessed);
            Assert.Equal(0, status.TotalTaskErrors);
        }

        [Fact]
        public void RecordTaskError_IncrementsBothCounters()
        {
            _monitor.RecordTaskError();

            var status = _monitor.GetHealthStatus();
            Assert.Equal(1, status.TotalTasksProcessed);
            Assert.Equal(1, status.TotalTaskErrors);
        }

        [Fact]
        public void GetHealthStatus_ReturnsCompleteStatus()
        {
            _monitor.IncrementRunningWorker();
            _monitor.RecordPollSuccess(5);
            _monitor.RecordTaskSuccess();
            _monitor.RecordTaskError();
            _monitor.RecordPollError();

            var status = _monitor.GetHealthStatus();

            Assert.Equal(1, status.RunningWorkers);
            Assert.Equal(1, status.ConsecutivePollErrors);
            Assert.Equal(2, status.TotalTasksProcessed);
            Assert.Equal(1, status.TotalTaskErrors);
            Assert.Equal(1, status.TotalPollErrors);
            Assert.NotNull(status.LastPollTime);
            Assert.NotNull(status.LastTaskCompletedTime);
            Assert.NotNull(status.LastErrorTime);
            Assert.True(status.IsHealthy);
        }

        [Fact]
        public void GetHealthStatus_TimestampsSetCorrectly()
        {
            var status = _monitor.GetHealthStatus();

            Assert.Null(status.LastPollTime);
            Assert.Null(status.LastTaskCompletedTime);
            Assert.Null(status.LastErrorTime);

            _monitor.RecordPollSuccess(0);
            status = _monitor.GetHealthStatus();
            Assert.NotNull(status.LastPollTime);
        }
    }
}
