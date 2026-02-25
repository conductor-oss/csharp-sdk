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

namespace Conductor.Client.Worker
{
    public class WorkflowTaskExecutorConfiguration
    {
        public int BatchSize { get; set; } = Math.Max(2, Environment.ProcessorCount * 2);
        public string Domain { get; set; } = null;
        public TimeSpan PollInterval { get; set; } = TimeSpan.FromMilliseconds(100);
        public string WorkerId { get; set; } = Environment.MachineName;

        /// <summary>
        /// Maximum backoff interval when no tasks are found during polling.
        /// The backoff increases exponentially from PollInterval up to this maximum.
        /// </summary>
        public TimeSpan MaxPollBackoffInterval { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Multiplier for exponential backoff on empty polls. Default is 2.0 (doubling).
        /// </summary>
        public double PollBackoffMultiplier { get; set; } = 2.0;

        /// <summary>
        /// If set, the worker checks this environment variable before each poll.
        /// When the variable is set to "true" (case-insensitive), the worker pauses polling.
        /// </summary>
        public string PauseEnvironmentVariable { get; set; } = null;

        /// <summary>
        /// Interval to check the pause environment variable when paused.
        /// </summary>
        public TimeSpan PauseCheckInterval { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Enable automatic lease extension for long-running tasks (> LeaseExtensionThreshold).
        /// </summary>
        public bool LeaseExtensionEnabled { get; set; } = false;

        /// <summary>
        /// Duration threshold after which task lease will be extended.
        /// </summary>
        public TimeSpan LeaseExtensionThreshold { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Maximum number of consecutive errors before a worker is considered unhealthy.
        /// </summary>
        public int MaxConsecutiveErrors { get; set; } = 10;
    }
}