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
using Conductor.Client;
using Conductor.Client.Extensions;
using Conductor.Client.Interfaces;
using Conductor.Client.Models;
using Conductor.Client.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace Conductor.Examples
{
    /// <summary>
    /// Demonstrates various worker configuration options including:
    /// - Custom poll intervals, batch sizes, and domains
    /// - Exponential backoff configuration
    /// - Lease extension for long-running tasks
    /// - Pause/resume via environment variables
    /// - 3-tier hierarchical configuration (code < global env < worker-specific env)
    /// </summary>
    public class WorkerConfigurationExample
    {
        /// <summary>
        /// Example 1: Basic worker with custom configuration via annotation.
        /// </summary>
        [WorkerTask(taskType: "configured_worker", batchSize: 5, pollIntervalMs: 500, workerId: "my-worker-1")]
        public TaskResult ConfiguredWorker(Conductor.Client.Models.Task task)
        {
            Console.WriteLine($"Processing task: {task.TaskId}");
            return task.Completed();
        }

        /// <summary>
        /// Example 2: Worker with domain-based routing.
        /// Tasks are routed to workers in specific domains.
        /// </summary>
        [WorkerTask(taskType: "domain_worker", domain: "us-east")]
        public TaskResult DomainWorker(Conductor.Client.Models.Task task)
        {
            Console.WriteLine($"Processing task in us-east domain: {task.TaskId}");
            return task.Completed();
        }

        /// <summary>
        /// Example 3: Programmatic worker configuration with all options.
        /// </summary>
        public static void RunWithAdvancedConfig()
        {
            var configuration = ApiExtensions.GetConfiguration();

            // Create a worker host with advanced configuration
            var host = new HostBuilder()
                .ConfigureServices((ctx, services) =>
                {
                    services.AddConductorWorker(configuration);

                    // Register a custom worker with full configuration
                    var worker = new SimpleConfiguredWorker();
                    services.AddConductorWorkflowTask(worker);
                    services.WithHostedService();
                })
                .ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Information);
                    logging.AddConsole();
                })
                .Build();

            host.RunAsync(CancellationToken.None).Wait();
        }

        /// <summary>
        /// Example 4: Using 3-tier hierarchical configuration.
        /// Set environment variables to override code defaults:
        ///
        /// Global (all workers):
        ///   export CONDUCTOR_WORKER_POLL_INTERVAL_MS=500
        ///   export CONDUCTOR_WORKER_BATCH_SIZE=10
        ///   export CONDUCTOR_WORKER_DOMAIN=production
        ///
        /// Worker-specific (overrides global):
        ///   export CONDUCTOR_WORKER_MY_TASK_POLL_INTERVAL_MS=1000
        ///   export CONDUCTOR_WORKER_MY_TASK_BATCH_SIZE=5
        /// </summary>
        public static void RunWithEnvironmentConfig()
        {
            var config = new WorkflowTaskExecutorConfiguration();
            config.ApplyEnvironmentOverrides("my_task");
            Console.WriteLine($"Final config - PollInterval: {config.PollInterval}, BatchSize: {config.BatchSize}");
        }
    }

    /// <summary>
    /// Example worker with programmatic configuration for exponential backoff,
    /// lease extension, and pause/resume.
    /// </summary>
    public class SimpleConfiguredWorker : IWorkflowTask
    {
        public string TaskType => "simple_configured_task";

        public WorkflowTaskExecutorConfiguration WorkerSettings => new WorkflowTaskExecutorConfiguration
        {
            BatchSize = 3,
            PollInterval = TimeSpan.FromMilliseconds(200),
            WorkerId = "advanced-worker-1",

            // Exponential backoff: start at 200ms, double each empty poll, max 30s
            MaxPollBackoffInterval = TimeSpan.FromSeconds(30),
            PollBackoffMultiplier = 2.0,

            // Lease extension: extend task lease every 60s for long-running tasks
            LeaseExtensionEnabled = true,
            LeaseExtensionThreshold = TimeSpan.FromSeconds(60),

            // Pause: check PAUSE_MY_WORKER env var every 10s
            PauseEnvironmentVariable = "PAUSE_MY_WORKER",
            PauseCheckInterval = TimeSpan.FromSeconds(10),

            // Auto-restart: retry up to 5 times with 10s delay
            MaxRestartAttempts = 5,
            RestartDelay = TimeSpan.FromSeconds(10),

            // Health: consider unhealthy after 20 consecutive errors
            MaxConsecutiveErrors = 20
        };

        public TaskResult Execute(Conductor.Client.Models.Task task)
        {
            Console.WriteLine($"Executing task: {task.TaskId}");
            return task.Completed();
        }

        public System.Threading.Tasks.Task<TaskResult> Execute(Conductor.Client.Models.Task task, CancellationToken token)
        {
            return System.Threading.Tasks.Task.FromResult(Execute(task));
        }
    }
}
