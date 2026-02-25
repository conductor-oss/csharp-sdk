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
using System.Collections.Generic;
using System.Diagnostics;

namespace Conductor.Client.Telemetry
{
    /// <summary>
    /// Per-worker metrics helper that wraps ConductorMetrics with task-type tags.
    /// Provides a convenient way to record metrics with consistent tagging per worker instance.
    /// </summary>
    public class WorkerMetrics
    {
        private readonly string _taskType;
        private readonly string _workerId;
        private readonly MetricsConfig _config;

        public WorkerMetrics(string taskType, string workerId, MetricsConfig config = null)
        {
            _taskType = taskType;
            _workerId = workerId;
            _config = config ?? MetricsConfig.Default;
        }

        public void RecordPoll(bool success, int taskCount)
        {
            if (!_config.Enabled || !_config.TaskPollingMetricsEnabled) return;

            ConductorMetrics.TaskPollCount.Add(1, Tag("taskType", _taskType));
            if (success && taskCount > 0)
            {
                ConductorMetrics.TaskPollSuccessCount.Add(1, Tag("taskType", _taskType));
            }
            else if (success)
            {
                ConductorMetrics.TaskPollEmptyCount.Add(1, Tag("taskType", _taskType));
            }
            else
            {
                ConductorMetrics.TaskPollErrorCount.Add(1, Tag("taskType", _taskType));
            }
        }

        public void RecordPollLatency(double milliseconds)
        {
            if (!_config.Enabled || !_config.TaskPollingMetricsEnabled) return;
            ConductorMetrics.TaskPollLatency.Record(milliseconds, Tag("taskType", _taskType));
        }

        public void RecordExecution(bool success)
        {
            if (!_config.Enabled || !_config.TaskExecutionMetricsEnabled) return;

            ConductorMetrics.TaskExecutionCount.Add(1, Tag("taskType", _taskType));
            if (success)
            {
                ConductorMetrics.TaskExecutionSuccessCount.Add(1, Tag("taskType", _taskType));
            }
            else
            {
                ConductorMetrics.TaskExecutionErrorCount.Add(1, Tag("taskType", _taskType));
            }
        }

        public void RecordExecutionLatency(double milliseconds)
        {
            if (!_config.Enabled || !_config.TaskExecutionMetricsEnabled) return;
            ConductorMetrics.TaskExecutionLatency.Record(milliseconds, Tag("taskType", _taskType));
        }

        public void RecordUpdate(bool success)
        {
            if (!_config.Enabled || !_config.TaskUpdateMetricsEnabled) return;

            ConductorMetrics.TaskUpdateCount.Add(1, Tag("taskType", _taskType));
            if (!success)
            {
                ConductorMetrics.TaskUpdateErrorCount.Add(1, Tag("taskType", _taskType));
            }
        }

        public void RecordUpdateRetry()
        {
            if (!_config.Enabled || !_config.TaskUpdateMetricsEnabled) return;
            ConductorMetrics.TaskUpdateRetryCount.Add(1, Tag("taskType", _taskType));
        }

        public void RecordUpdateLatency(double milliseconds)
        {
            if (!_config.Enabled || !_config.TaskUpdateMetricsEnabled) return;
            ConductorMetrics.TaskUpdateLatency.Record(milliseconds, Tag("taskType", _taskType));
        }

        public void RecordPayloadSize(double bytes)
        {
            if (!_config.Enabled || !_config.PayloadSizeMetricsEnabled) return;
            ConductorMetrics.PayloadSize.Record(bytes, Tag("taskType", _taskType));
        }

        public TimingScope TimePoll()
        {
            return ConductorMetrics.Time(ConductorMetrics.TaskPollLatency, Tag("taskType", _taskType));
        }

        public TimingScope TimeExecution()
        {
            return ConductorMetrics.Time(ConductorMetrics.TaskExecutionLatency, Tag("taskType", _taskType));
        }

        public TimingScope TimeUpdate()
        {
            return ConductorMetrics.Time(ConductorMetrics.TaskUpdateLatency, Tag("taskType", _taskType));
        }

        private static KeyValuePair<string, object> Tag(string key, string value)
        {
            return new KeyValuePair<string, object>(key, value);
        }
    }
}
