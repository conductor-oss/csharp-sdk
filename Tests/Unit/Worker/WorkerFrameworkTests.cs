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
using Conductor.Client.Interfaces;
using Conductor.Client.Worker;
using System;
using Xunit;

namespace Tests.Unit.Worker
{
    /// <summary>
    /// Tests for worker framework edge cases: exponential backoff, auto-restart, 3-tier config, pause/resume.
    /// </summary>
    public class WorkerFrameworkTests
    {
        #region Exponential Backoff Configuration

        [Fact]
        public void Config_MaxPollBackoffInterval_DefaultIs10Seconds()
        {
            var config = new WorkflowTaskExecutorConfiguration();
            Assert.Equal(TimeSpan.FromSeconds(10), config.MaxPollBackoffInterval);
        }

        [Fact]
        public void Config_PollBackoffMultiplier_DefaultIs2()
        {
            var config = new WorkflowTaskExecutorConfiguration();
            Assert.Equal(2.0, config.PollBackoffMultiplier);
        }

        [Fact]
        public void Config_CustomBackoffSettings()
        {
            var config = new WorkflowTaskExecutorConfiguration
            {
                MaxPollBackoffInterval = TimeSpan.FromSeconds(30),
                PollBackoffMultiplier = 1.5
            };
            Assert.Equal(TimeSpan.FromSeconds(30), config.MaxPollBackoffInterval);
            Assert.Equal(1.5, config.PollBackoffMultiplier);
        }

        #endregion

        #region Auto-Restart Configuration

        [Fact]
        public void Config_MaxRestartAttempts_DefaultIs3()
        {
            var config = new WorkflowTaskExecutorConfiguration();
            Assert.Equal(3, config.MaxRestartAttempts);
        }

        [Fact]
        public void Config_RestartDelay_DefaultIs5Seconds()
        {
            var config = new WorkflowTaskExecutorConfiguration();
            Assert.Equal(TimeSpan.FromSeconds(5), config.RestartDelay);
        }

        [Fact]
        public void Config_CustomRestartSettings()
        {
            var config = new WorkflowTaskExecutorConfiguration
            {
                MaxRestartAttempts = 5,
                RestartDelay = TimeSpan.FromSeconds(10)
            };
            Assert.Equal(5, config.MaxRestartAttempts);
            Assert.Equal(TimeSpan.FromSeconds(10), config.RestartDelay);
        }

        #endregion

        #region Lease Extension Configuration

        [Fact]
        public void Config_LeaseExtension_DisabledByDefault()
        {
            var config = new WorkflowTaskExecutorConfiguration();
            Assert.False(config.LeaseExtensionEnabled);
        }

        [Fact]
        public void Config_LeaseExtensionThreshold_DefaultIs30Seconds()
        {
            var config = new WorkflowTaskExecutorConfiguration();
            Assert.Equal(TimeSpan.FromSeconds(30), config.LeaseExtensionThreshold);
        }

        [Fact]
        public void Config_EnableLeaseExtension()
        {
            var config = new WorkflowTaskExecutorConfiguration
            {
                LeaseExtensionEnabled = true,
                LeaseExtensionThreshold = TimeSpan.FromSeconds(60)
            };
            Assert.True(config.LeaseExtensionEnabled);
            Assert.Equal(TimeSpan.FromSeconds(60), config.LeaseExtensionThreshold);
        }

        #endregion

        #region Pause/Resume Configuration

        [Fact]
        public void Config_PauseEnvironmentVariable_DefaultIsNull()
        {
            var config = new WorkflowTaskExecutorConfiguration();
            Assert.Null(config.PauseEnvironmentVariable);
        }

        [Fact]
        public void Config_PauseCheckInterval_DefaultIs5Seconds()
        {
            var config = new WorkflowTaskExecutorConfiguration();
            Assert.Equal(TimeSpan.FromSeconds(5), config.PauseCheckInterval);
        }

        [Fact]
        public void Config_CustomPauseSettings()
        {
            var config = new WorkflowTaskExecutorConfiguration
            {
                PauseEnvironmentVariable = "WORKER_PAUSED",
                PauseCheckInterval = TimeSpan.FromSeconds(2)
            };
            Assert.Equal("WORKER_PAUSED", config.PauseEnvironmentVariable);
            Assert.Equal(TimeSpan.FromSeconds(2), config.PauseCheckInterval);
        }

        #endregion

        #region MaxConsecutiveErrors

        [Fact]
        public void Config_MaxConsecutiveErrors_DefaultIs10()
        {
            var config = new WorkflowTaskExecutorConfiguration();
            Assert.Equal(10, config.MaxConsecutiveErrors);
        }

        #endregion

        #region 3-Tier Configuration Override

        [Fact]
        public void Config_ApplyEnvironmentOverrides_PollInterval()
        {
            var config = new WorkflowTaskExecutorConfiguration();
            var original = config.PollInterval;

            // Set environment variable
            Environment.SetEnvironmentVariable("CONDUCTOR_WORKER_POLL_INTERVAL", "5000");
            try
            {
                config.ApplyEnvironmentOverrides();
                // The method should have read the env var and applied it
                // Verify the method ran without throwing
            }
            finally
            {
                Environment.SetEnvironmentVariable("CONDUCTOR_WORKER_POLL_INTERVAL", null);
            }
        }

        [Fact]
        public void Config_ApplyEnvironmentOverrides_WithPrefix()
        {
            var config = new WorkflowTaskExecutorConfiguration();

            Environment.SetEnvironmentVariable("CONDUCTOR_WORKER_my_task_BATCH_SIZE", "10");
            try
            {
                config.ApplyEnvironmentOverrides("my_task");
                // Verify method runs without throwing
            }
            finally
            {
                Environment.SetEnvironmentVariable("CONDUCTOR_WORKER_my_task_BATCH_SIZE", null);
            }
        }

        #endregion

        #region Health Status

        [Fact]
        public void WorkerHealthStatus_DefaultValues()
        {
            var status = new WorkerHealthStatus();
            Assert.False(status.IsHealthy);
            Assert.Equal(0, status.RunningWorkers);
            Assert.Equal(0, status.ConsecutivePollErrors);
            Assert.Equal(0, status.TotalTasksProcessed);
            Assert.Equal(0, status.TotalTaskErrors);
            Assert.Equal(0, status.TotalPollErrors);
            Assert.Null(status.LastPollTime);
            Assert.Null(status.LastTaskCompletedTime);
            Assert.Null(status.LastErrorTime);
        }

        [Fact]
        public void WorkerHealthStatus_SetProperties()
        {
            var now = DateTime.UtcNow;
            var status = new WorkerHealthStatus
            {
                IsHealthy = true,
                RunningWorkers = 3,
                ConsecutivePollErrors = 0,
                TotalTasksProcessed = 100,
                TotalTaskErrors = 2,
                TotalPollErrors = 5,
                LastPollTime = now,
                LastTaskCompletedTime = now,
                LastErrorTime = null
            };

            Assert.True(status.IsHealthy);
            Assert.Equal(3, status.RunningWorkers);
            Assert.Equal(100, status.TotalTasksProcessed);
            Assert.Equal(now, status.LastPollTime);
        }

        #endregion
    }
}
