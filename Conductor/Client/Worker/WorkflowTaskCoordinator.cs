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
using Conductor.Client.Models;
using Conductor.Client.Telemetry;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using ThreadingTask = System.Threading.Tasks.Task;

namespace Conductor.Client.Worker
{
    internal class WorkflowTaskCoordinator : IWorkflowTaskCoordinator
    {
        private readonly ILogger<WorkflowTaskCoordinator> _logger;
        private readonly ILogger<WorkflowTaskExecutor> _loggerWorkflowTaskExecutor;
        private readonly ILogger<WorkflowTaskMonitor> _loggerWorkflowTaskMonitor;
        private readonly HashSet<IWorkflowTaskExecutor> _workers;
        private readonly IWorkflowTaskClient _client;
        private readonly MetricsCollector _metrics;
        private readonly Dictionary<string, WorkflowTaskMonitor> _workerMonitors;
        private readonly IMetadataClient _metadataClient;

        /// <param name="metadataClient">
        /// Optional. When provided, task definitions annotated with
        /// <c>[WorkerTask(RegisterTaskDef = true)]</c> are automatically registered on startup.
        /// </param>
        public WorkflowTaskCoordinator(IWorkflowTaskClient client, ILogger<WorkflowTaskCoordinator> logger,
            ILogger<WorkflowTaskExecutor> loggerWorkflowTaskExecutor,
            ILogger<WorkflowTaskMonitor> loggerWorkflowTaskMonitor,
            MetricsCollector metrics = null,
            IMetadataClient metadataClient = null)
        {
            _logger = logger;
            _client = client;
            _workers = new HashSet<IWorkflowTaskExecutor>();
            _loggerWorkflowTaskExecutor = loggerWorkflowTaskExecutor;
            _loggerWorkflowTaskMonitor = loggerWorkflowTaskMonitor;
            _metrics = metrics;
            _workerMonitors = new Dictionary<string, WorkflowTaskMonitor>();
            _metadataClient = metadataClient;
        }

        public async ThreadingTask Start(CancellationToken token)
        {
            if (token != CancellationToken.None)
                token.ThrowIfCancellationRequested();

            _logger.LogDebug("Starting workers...");
            DiscoverWorkers();
            var runningWorkers = new List<ThreadingTask>();
            foreach (var worker in _workers)
            {
                var runningWorker = worker.Start(token);
                runningWorkers.Add(runningWorker);
            }
            _logger.LogDebug("Started all workers");
            await ThreadingTask.WhenAll(runningWorkers);
        }

        public void RegisterWorker(IWorkflowTask worker)
        {
            var maxConsecutiveErrors = worker.WorkerSettings?.MaxConsecutiveErrors ?? 10;
            var workflowTaskMonitor = new WorkflowTaskMonitor(_loggerWorkflowTaskMonitor, maxConsecutiveErrors);
            var workflowTaskExecutor = new WorkflowTaskExecutor(
                _loggerWorkflowTaskExecutor,
                _client,
                worker,
                workflowTaskMonitor,
                _metrics
            );
            _workers.Add(workflowTaskExecutor);
            _workerMonitors[worker.TaskType] = workflowTaskMonitor;
        }

        public bool IsHealthy()
        {
            return _workerMonitors.Values.All(m => m.IsHealthy());
        }

        public Dictionary<string, WorkerHealthStatus> GetHealthStatuses()
        {
            return _workerMonitors.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.GetHealthStatus()
            );
        }

        private void DiscoverWorkers()
        {
            var taskDefsToRegister = new List<TaskDef>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.GetCustomAttribute<WorkerTask>() == null)
                    {
                        continue;
                    }
                    foreach (var method in type.GetMethods())
                    {
                        var workerTask = method.GetCustomAttribute<WorkerTask>();
                        if (workerTask == null)
                        {
                            continue;
                        }
                        object workerInstance = null;
                        if (!method.IsStatic)
                        {
                            workerInstance = Activator.CreateInstance(type);
                        }
                        var worker = new GenericWorker(
                            workerTask.TaskType,
                            workerTask.WorkerSettings,
                            method,
                            workerInstance
                        );
                        RegisterWorker(worker);

                        if (workerTask.RegisterTaskDef && _metadataClient != null)
                        {
                            taskDefsToRegister.Add(new TaskDef
                            {
                                Name = workerTask.TaskType,
                                Description = workerTask.Description,
                                TimeoutSeconds = workerTask.TimeoutSeconds,
                            });
                        }
                    }
                }
            }

            if (taskDefsToRegister.Count > 0)
            {
                try
                {
                    _metadataClient.RegisterTaskDefs(taskDefsToRegister);
                    _logger.LogInformation($"Registered {taskDefsToRegister.Count} task definition(s): {string.Join(", ", taskDefsToRegister.Select(t => t.Name))}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to auto-register task definitions: {ex.Message}");
                }
            }
        }
    }
}
