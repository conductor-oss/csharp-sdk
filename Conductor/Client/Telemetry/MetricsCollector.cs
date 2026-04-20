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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Metrics;

namespace Conductor.Client.Telemetry
{
    /// <summary>
    /// Instruments the Conductor worker poll-execute-update loop with
    /// <see cref="System.Diagnostics.Metrics"/> counters, histograms, and gauges.
    ///
    /// The <see cref="Meter"/> is named <c>Conductor.Client</c>. To expose these
    /// metrics, attach a listener such as the OpenTelemetry Prometheus exporter
    /// (see METRICS.md for examples).
    /// </summary>
    public sealed class MetricsCollector
    {
        public const string MeterName = "Conductor.Client";

        private readonly Meter _meter;

        // --- counters ---
        private readonly Counter<long> _taskPollTotal;
        private readonly Counter<long> _taskPollErrorTotal;
        private readonly Counter<long> _taskExecuteErrorTotal;
        private readonly Counter<long> _taskUpdateErrorTotal;
        private readonly Counter<long> _taskPausedTotal;
        private readonly Counter<long> _taskExecutionQueueFullTotal;
        private readonly Counter<long> _threadUncaughtExceptionsTotal;
        private readonly Counter<long> _workflowStartErrorTotal;
        private readonly Counter<long> _externalPayloadUsedTotal;

        // --- histograms ---
        private readonly Histogram<double> _taskPollTimeSeconds;
        private readonly Histogram<double> _taskExecuteTimeSeconds;
        private readonly Histogram<double> _taskUpdateTimeSeconds;
        private readonly Histogram<double> _taskResultSizeBytes;
        private readonly Histogram<double> _workflowInputSizeBytes;

        // --- gauges ---
        private readonly ConcurrentDictionary<string, int> _activeWorkerCounts = new ConcurrentDictionary<string, int>();

        public MetricsCollector()
        {
            _meter = new Meter(MeterName);

            _taskPollTotal = _meter.CreateCounter<long>(
                "task_poll_total",
                description: "Total number of task poll attempts");

            _taskPollErrorTotal = _meter.CreateCounter<long>(
                "task_poll_error_total",
                description: "Total number of task poll errors");

            _taskExecuteErrorTotal = _meter.CreateCounter<long>(
                "task_execute_error_total",
                description: "Total number of task execution errors");

            _taskUpdateErrorTotal = _meter.CreateCounter<long>(
                "task_update_error_total",
                description: "Total number of task update errors");

            _taskPausedTotal = _meter.CreateCounter<long>(
                "task_paused_total",
                description: "Polls skipped because worker is paused");

            _taskExecutionQueueFullTotal = _meter.CreateCounter<long>(
                "task_execution_queue_full_total",
                description: "Polls returning zero capacity (all workers busy)");

            _threadUncaughtExceptionsTotal = _meter.CreateCounter<long>(
                "thread_uncaught_exceptions_total",
                description: "Uncaught exceptions in worker threads");

            _workflowStartErrorTotal = _meter.CreateCounter<long>(
                "workflow_start_error_total",
                description: "Errors starting workflows");

            _externalPayloadUsedTotal = _meter.CreateCounter<long>(
                "external_payload_used_total",
                description: "External payload storage usage");

            _taskPollTimeSeconds = _meter.CreateHistogram<double>(
                "task_poll_time_seconds",
                unit: "s",
                description: "Task poll round-trip duration in seconds");

            _taskExecuteTimeSeconds = _meter.CreateHistogram<double>(
                "task_execute_time_seconds",
                unit: "s",
                description: "Task execution duration in seconds");

            _taskUpdateTimeSeconds = _meter.CreateHistogram<double>(
                "task_update_time_seconds",
                unit: "s",
                description: "Task result update duration in seconds");

            _taskResultSizeBytes = _meter.CreateHistogram<double>(
                "task_result_size_bytes",
                unit: "By",
                description: "Size of task result payload in bytes");

            _workflowInputSizeBytes = _meter.CreateHistogram<double>(
                "workflow_input_size_bytes",
                unit: "By",
                description: "Size of workflow input payload in bytes");

            _meter.CreateObservableGauge(
                "active_workers",
                observeValues: () =>
                {
                    var measurements = new List<Measurement<int>>();
                    foreach (var kvp in _activeWorkerCounts)
                    {
                        measurements.Add(new Measurement<int>(
                            kvp.Value,
                            new KeyValuePair<string, object>("task_type", kvp.Key)));
                    }
                    return measurements;
                },
                description: "Number of workers currently executing tasks");
        }

        // ---------------------------------------------------------------
        // Poll
        // ---------------------------------------------------------------

        public void RecordTaskPoll(string taskType)
        {
            _taskPollTotal.Add(1, new KeyValuePair<string, object>("task_type", taskType));
        }

        public void RecordTaskPollTime(string taskType, double durationSeconds)
        {
            _taskPollTimeSeconds.Record(durationSeconds,
                new KeyValuePair<string, object>("task_type", taskType));
        }

        public void RecordTaskPollError(string taskType, string errorType)
        {
            _taskPollErrorTotal.Add(1,
                new KeyValuePair<string, object>("task_type", taskType),
                new KeyValuePair<string, object>("error_type", errorType));
        }

        // ---------------------------------------------------------------
        // Execution
        // ---------------------------------------------------------------

        public void RecordTaskExecuteTime(string taskType, double durationSeconds)
        {
            _taskExecuteTimeSeconds.Record(durationSeconds,
                new KeyValuePair<string, object>("task_type", taskType));
        }

        public void RecordTaskExecuteError(string taskType, string errorType)
        {
            _taskExecuteErrorTotal.Add(1,
                new KeyValuePair<string, object>("task_type", taskType),
                new KeyValuePair<string, object>("error_type", errorType));
        }

        // ---------------------------------------------------------------
        // Update
        // ---------------------------------------------------------------

        public void RecordTaskUpdateTime(string taskType, double durationSeconds)
        {
            _taskUpdateTimeSeconds.Record(durationSeconds,
                new KeyValuePair<string, object>("task_type", taskType));
        }

        public void RecordTaskUpdateError(string taskType)
        {
            _taskUpdateErrorTotal.Add(1,
                new KeyValuePair<string, object>("task_type", taskType));
        }

        // ---------------------------------------------------------------
        // Sizes
        // ---------------------------------------------------------------

        public void RecordTaskResultSize(string taskType, double sizeBytes)
        {
            _taskResultSizeBytes.Record(sizeBytes,
                new KeyValuePair<string, object>("task_type", taskType));
        }

        public void RecordWorkflowInputSize(string workflowType, string version, double sizeBytes)
        {
            _workflowInputSizeBytes.Record(sizeBytes,
                new KeyValuePair<string, object>("workflow_type", workflowType),
                new KeyValuePair<string, object>("version", version));
        }

        // ---------------------------------------------------------------
        // Queue / capacity
        // ---------------------------------------------------------------

        public void RecordTaskExecutionQueueFull(string taskType)
        {
            _taskExecutionQueueFullTotal.Add(1,
                new KeyValuePair<string, object>("task_type", taskType));
        }

        public void RecordTaskPaused(string taskType)
        {
            _taskPausedTotal.Add(1,
                new KeyValuePair<string, object>("task_type", taskType));
        }

        // ---------------------------------------------------------------
        // Active workers (observable gauge snapshot)
        // ---------------------------------------------------------------

        public void RecordActiveWorkers(string taskType, int count)
        {
            _activeWorkerCounts[taskType] = count;
        }

        // ---------------------------------------------------------------
        // Uncategorised
        // ---------------------------------------------------------------

        public void RecordUncaughtException()
        {
            _threadUncaughtExceptionsTotal.Add(1);
        }

        public void RecordWorkflowStartError(string workflowType)
        {
            _workflowStartErrorTotal.Add(1,
                new KeyValuePair<string, object>("workflow_type", workflowType));
        }

        public void RecordExternalPayloadUsed(string entityName, string operation, string payloadType)
        {
            _externalPayloadUsedTotal.Add(1,
                new KeyValuePair<string, object>("entity_name", entityName),
                new KeyValuePair<string, object>("operation", operation),
                new KeyValuePair<string, object>("payload_type", payloadType));
        }
    }
}
