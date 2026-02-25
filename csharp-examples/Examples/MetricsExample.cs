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
using Conductor.Client.Telemetry;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Threading;

namespace Conductor.Examples
{
    /// <summary>
    /// Demonstrates how to use and consume Conductor SDK metrics.
    /// Shows how to listen for metrics and export them.
    /// </summary>
    public class MetricsExample
    {
        public static void Run()
        {
            Console.WriteLine("=== Metrics Example ===\n");

            // 1. Using WorkerMetrics for per-task-type recording
            Console.WriteLine("1. Using WorkerMetrics helper...");
            var workerMetrics = new WorkerMetrics("my_task_type", "worker-1");

            // Record a poll
            workerMetrics.RecordPoll(success: true, taskCount: 3);
            workerMetrics.RecordPollLatency(15.5);

            // Record an execution with timing
            using (workerMetrics.TimeExecution())
            {
                Thread.Sleep(10); // Simulate work
            }
            workerMetrics.RecordExecution(success: true);

            // Record an update
            workerMetrics.RecordUpdate(success: true);
            workerMetrics.RecordUpdateLatency(5.2);

            Console.WriteLine("   Metrics recorded.");

            // 2. Using MetricsConfig to control what's collected
            Console.WriteLine("\n2. Using MetricsConfig...");
            var config = new MetricsConfig
            {
                Enabled = true,
                TaskPollingMetricsEnabled = true,
                TaskExecutionMetricsEnabled = true,
                TaskUpdateMetricsEnabled = false,  // Disable update metrics
                PayloadSizeMetricsEnabled = true
            };
            var configuredMetrics = new WorkerMetrics("configured_task", "worker-2", config);
            configuredMetrics.RecordPoll(true, 1);
            configuredMetrics.RecordUpdate(true); // This won't be recorded (disabled)
            configuredMetrics.RecordPayloadSize(2048);
            Console.WriteLine("   Configured metrics recorded.");

            // 3. Using MeterListener to consume metrics
            Console.WriteLine("\n3. Listening for metrics...");
            using (var listener = new MeterListener())
            {
                listener.InstrumentPublished = (instrument, meterListener) =>
                {
                    if (instrument.Meter.Name == ConductorMetrics.MeterName)
                    {
                        meterListener.EnableMeasurementEvents(instrument);
                    }
                };

                listener.SetMeasurementEventCallback<long>((instrument, measurement, tags, state) =>
                {
                    Console.WriteLine($"   Counter: {instrument.Name} = {measurement}");
                });

                listener.SetMeasurementEventCallback<double>((instrument, measurement, tags, state) =>
                {
                    Console.WriteLine($"   Histogram: {instrument.Name} = {measurement:F2}");
                });

                listener.Start();

                // These will be captured by the listener
                ConductorMetrics.TaskPollCount.Add(1, new KeyValuePair<string, object>("taskType", "demo"));
                ConductorMetrics.TaskExecutionLatency.Record(42.5, new KeyValuePair<string, object>("taskType", "demo"));

                listener.RecordObservableInstruments();
            }

            // 4. Using TimingScope directly
            Console.WriteLine("\n4. Using TimingScope...");
            using (ConductorMetrics.Time(ConductorMetrics.ApiLatency,
                new KeyValuePair<string, object>("endpoint", "/api/tasks/poll")))
            {
                Thread.Sleep(5); // Simulate API call
            }
            Console.WriteLine("   API latency recorded.");

            Console.WriteLine("\nMetrics Example completed!");
        }
    }
}
