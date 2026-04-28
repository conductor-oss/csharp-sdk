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

        /// <summary>
        /// Maximum number of restart attempts after a worker crashes.
        /// Set to 0 to disable auto-restart (default: 3).
        /// </summary>
        public int MaxRestartAttempts { get; set; } = 3;

        /// <summary>
        /// Delay before restarting a crashed worker.
        /// </summary>
        public TimeSpan RestartDelay { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Applies configuration from environment variables.
        /// Worker-specific env vars (prefixed with taskType) override global env vars,
        /// which in turn override code-level defaults.
        ///
        /// Resolution order: code defaults &lt; global env vars &lt; worker-specific env vars
        ///
        /// Global env vars:
        ///   CONDUCTOR_WORKER_POLL_INTERVAL_MS
        ///   CONDUCTOR_WORKER_BATCH_SIZE
        ///   CONDUCTOR_WORKER_DOMAIN
        ///   CONDUCTOR_WORKER_PAUSE
        ///   CONDUCTOR_WORKER_MAX_BACKOFF_MS
        ///
        /// Worker-specific env vars (replace {TASK_TYPE} with uppercase task type):
        ///   CONDUCTOR_WORKER_{TASK_TYPE}_POLL_INTERVAL_MS
        ///   CONDUCTOR_WORKER_{TASK_TYPE}_BATCH_SIZE
        ///   CONDUCTOR_WORKER_{TASK_TYPE}_DOMAIN
        ///   CONDUCTOR_WORKER_{TASK_TYPE}_PAUSE
        /// </summary>
        public void ApplyEnvironmentOverrides(string taskType = null)
        {
            // Global env vars
            ApplyEnvVar("CONDUCTOR_WORKER_POLL_INTERVAL_MS", v => PollInterval = TimeSpan.FromMilliseconds(int.Parse(v)));
            ApplyEnvVar("CONDUCTOR_WORKER_BATCH_SIZE", v => BatchSize = int.Parse(v));
            ApplyEnvVar("CONDUCTOR_WORKER_DOMAIN", v => Domain = v);
            ApplyEnvVar("CONDUCTOR_WORKER_PAUSE", v => PauseEnvironmentVariable = v);
            ApplyEnvVar("CONDUCTOR_WORKER_MAX_BACKOFF_MS", v => MaxPollBackoffInterval = TimeSpan.FromMilliseconds(int.Parse(v)));

            // Worker-specific env vars (override globals)
            if (!string.IsNullOrEmpty(taskType))
            {
                var prefix = $"CONDUCTOR_WORKER_{taskType.ToUpperInvariant().Replace('-', '_')}_";
                ApplyEnvVar($"{prefix}POLL_INTERVAL_MS", v => PollInterval = TimeSpan.FromMilliseconds(int.Parse(v)));
                ApplyEnvVar($"{prefix}BATCH_SIZE", v => BatchSize = int.Parse(v));
                ApplyEnvVar($"{prefix}DOMAIN", v => Domain = v);
                ApplyEnvVar($"{prefix}PAUSE", v => PauseEnvironmentVariable = v);
            }
        }

        private static void ApplyEnvVar(string name, Action<string> setter)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (!string.IsNullOrEmpty(value))
            {
                setter(value);
            }
        }
    }
}