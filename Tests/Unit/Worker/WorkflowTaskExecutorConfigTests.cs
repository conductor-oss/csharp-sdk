using Conductor.Client.Worker;
using System;
using Xunit;

namespace Tests.Unit.Worker
{
    public class WorkflowTaskExecutorConfigTests
    {
        [Fact]
        public void DefaultValues_AreCorrect()
        {
            var config = new WorkflowTaskExecutorConfiguration();

            Assert.True(config.BatchSize >= 2);
            Assert.Null(config.Domain);
            Assert.Equal(TimeSpan.FromMilliseconds(100), config.PollInterval);
            Assert.Equal(Environment.MachineName, config.WorkerId);
            Assert.Equal(TimeSpan.FromSeconds(10), config.MaxPollBackoffInterval);
            Assert.Equal(2.0, config.PollBackoffMultiplier);
            Assert.Null(config.PauseEnvironmentVariable);
            Assert.Equal(TimeSpan.FromSeconds(5), config.PauseCheckInterval);
            Assert.False(config.LeaseExtensionEnabled);
            Assert.Equal(TimeSpan.FromSeconds(30), config.LeaseExtensionThreshold);
            Assert.Equal(10, config.MaxConsecutiveErrors);
        }

        [Fact]
        public void BatchSize_DefaultIsAtLeast2()
        {
            var config = new WorkflowTaskExecutorConfiguration();
            Assert.True(config.BatchSize >= 2);
        }

        [Fact]
        public void Properties_CanBeSet()
        {
            var config = new WorkflowTaskExecutorConfiguration
            {
                BatchSize = 10,
                Domain = "test-domain",
                PollInterval = TimeSpan.FromMilliseconds(500),
                WorkerId = "custom-worker",
                MaxPollBackoffInterval = TimeSpan.FromSeconds(30),
                PollBackoffMultiplier = 3.0,
                PauseEnvironmentVariable = "PAUSE_WORKER",
                PauseCheckInterval = TimeSpan.FromSeconds(10),
                LeaseExtensionEnabled = true,
                LeaseExtensionThreshold = TimeSpan.FromSeconds(60),
                MaxConsecutiveErrors = 5
            };

            Assert.Equal(10, config.BatchSize);
            Assert.Equal("test-domain", config.Domain);
            Assert.Equal(TimeSpan.FromMilliseconds(500), config.PollInterval);
            Assert.Equal("custom-worker", config.WorkerId);
            Assert.Equal(TimeSpan.FromSeconds(30), config.MaxPollBackoffInterval);
            Assert.Equal(3.0, config.PollBackoffMultiplier);
            Assert.Equal("PAUSE_WORKER", config.PauseEnvironmentVariable);
            Assert.Equal(TimeSpan.FromSeconds(10), config.PauseCheckInterval);
            Assert.True(config.LeaseExtensionEnabled);
            Assert.Equal(TimeSpan.FromSeconds(60), config.LeaseExtensionThreshold);
            Assert.Equal(5, config.MaxConsecutiveErrors);
        }
    }
}
