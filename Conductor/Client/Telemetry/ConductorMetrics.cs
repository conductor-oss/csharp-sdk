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
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Conductor.Client.Telemetry
{
    /// <summary>
    /// Central metrics registry for the Conductor SDK.
    /// Uses System.Diagnostics.Metrics which integrates with OpenTelemetry, Prometheus, and other exporters.
    /// </summary>
    public static class ConductorMetrics
    {
        public static readonly string MeterName = "Conductor.Client";

        private static readonly Meter _meter = new Meter(MeterName, "1.0.0");

        // Task polling metrics
        public static readonly Counter<long> TaskPollCount = _meter.CreateCounter<long>(
            "conductor.worker.task_poll_count",
            description: "Total number of task poll attempts");

        public static readonly Counter<long> TaskPollSuccessCount = _meter.CreateCounter<long>(
            "conductor.worker.task_poll_success_count",
            description: "Number of successful task polls that returned tasks");

        public static readonly Counter<long> TaskPollEmptyCount = _meter.CreateCounter<long>(
            "conductor.worker.task_poll_empty_count",
            description: "Number of task polls that returned no tasks");

        public static readonly Counter<long> TaskPollErrorCount = _meter.CreateCounter<long>(
            "conductor.worker.task_poll_error_count",
            description: "Number of task poll errors");

        public static readonly Histogram<double> TaskPollLatency = _meter.CreateHistogram<double>(
            "conductor.worker.task_poll_latency_ms",
            unit: "ms",
            description: "Task poll latency in milliseconds");

        // Task execution metrics
        public static readonly Counter<long> TaskExecutionCount = _meter.CreateCounter<long>(
            "conductor.worker.task_execution_count",
            description: "Total number of task executions");

        public static readonly Counter<long> TaskExecutionSuccessCount = _meter.CreateCounter<long>(
            "conductor.worker.task_execution_success_count",
            description: "Number of successful task executions");

        public static readonly Counter<long> TaskExecutionErrorCount = _meter.CreateCounter<long>(
            "conductor.worker.task_execution_error_count",
            description: "Number of task execution errors");

        public static readonly Histogram<double> TaskExecutionLatency = _meter.CreateHistogram<double>(
            "conductor.worker.task_execution_latency_ms",
            unit: "ms",
            description: "Task execution latency in milliseconds");

        // Task update metrics
        public static readonly Counter<long> TaskUpdateCount = _meter.CreateCounter<long>(
            "conductor.worker.task_update_count",
            description: "Total number of task update attempts");

        public static readonly Counter<long> TaskUpdateErrorCount = _meter.CreateCounter<long>(
            "conductor.worker.task_update_error_count",
            description: "Number of task update errors");

        public static readonly Counter<long> TaskUpdateRetryCount = _meter.CreateCounter<long>(
            "conductor.worker.task_update_retry_count",
            description: "Number of task update retries");

        public static readonly Histogram<double> TaskUpdateLatency = _meter.CreateHistogram<double>(
            "conductor.worker.task_update_latency_ms",
            unit: "ms",
            description: "Task update latency in milliseconds");

        // Worker metrics
        public static readonly Counter<long> WorkerRestartCount = _meter.CreateCounter<long>(
            "conductor.worker.restart_count",
            description: "Number of worker restarts after errors");

        public static readonly Histogram<double> PayloadSize = _meter.CreateHistogram<double>(
            "conductor.worker.payload_size_bytes",
            unit: "bytes",
            description: "Size of task input/output payloads in bytes");

        // API metrics
        public static readonly Counter<long> ApiCallCount = _meter.CreateCounter<long>(
            "conductor.api.call_count",
            description: "Total number of API calls");

        public static readonly Counter<long> ApiErrorCount = _meter.CreateCounter<long>(
            "conductor.api.error_count",
            description: "Number of API call errors");

        public static readonly Histogram<double> ApiLatency = _meter.CreateHistogram<double>(
            "conductor.api.latency_ms",
            unit: "ms",
            description: "API call latency in milliseconds");

        /// <summary>
        /// Creates a Stopwatch-based timing scope that records latency on dispose.
        /// Usage: using (ConductorMetrics.Time(ConductorMetrics.TaskExecutionLatency, tags)) { ... }
        /// </summary>
        public static TimingScope Time(Histogram<double> histogram, params KeyValuePair<string, object>[] tags)
        {
            return new TimingScope(histogram, tags);
        }
    }

    public struct TimingScope : IDisposable
    {
        private readonly Histogram<double> _histogram;
        private readonly KeyValuePair<string, object>[] _tags;
        private readonly Stopwatch _stopwatch;

        public TimingScope(Histogram<double> histogram, KeyValuePair<string, object>[] tags)
        {
            _histogram = histogram;
            _tags = tags;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            if (_tags != null && _tags.Length > 0)
            {
                _histogram.Record(_stopwatch.Elapsed.TotalMilliseconds, _tags);
            }
            else
            {
                _histogram.Record(_stopwatch.Elapsed.TotalMilliseconds);
            }
        }
    }
}
