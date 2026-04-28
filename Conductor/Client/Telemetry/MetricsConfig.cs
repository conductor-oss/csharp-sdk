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
namespace Conductor.Client.Telemetry
{
    /// <summary>
    /// Configuration for Conductor SDK metrics collection.
    /// </summary>
    public class MetricsConfig
    {
        /// <summary>
        /// Enable or disable metrics collection globally. Default: true.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Enable task polling metrics (poll count, latency, empty polls, errors).
        /// </summary>
        public bool TaskPollingMetricsEnabled { get; set; } = true;

        /// <summary>
        /// Enable task execution metrics (execution count, latency, success/error).
        /// </summary>
        public bool TaskExecutionMetricsEnabled { get; set; } = true;

        /// <summary>
        /// Enable task update metrics (update count, retry count, latency).
        /// </summary>
        public bool TaskUpdateMetricsEnabled { get; set; } = true;

        /// <summary>
        /// Enable API call metrics (call count, error count, latency).
        /// </summary>
        public bool ApiMetricsEnabled { get; set; } = true;

        /// <summary>
        /// Enable payload size tracking.
        /// </summary>
        public bool PayloadSizeMetricsEnabled { get; set; } = false;

        /// <summary>
        /// Default configuration with all standard metrics enabled.
        /// </summary>
        public static MetricsConfig Default => new MetricsConfig();

        /// <summary>
        /// Configuration with all metrics disabled.
        /// </summary>
        public static MetricsConfig Disabled => new MetricsConfig { Enabled = false };
    }
}
