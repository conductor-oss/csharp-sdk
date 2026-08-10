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
using Conductor.Client.Telemetry;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

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

        public WorkflowTaskCoordinator(IWorkflowTaskClient client, ILogger<WorkflowTaskCoordinator> logger, ILogger<WorkflowTaskExecutor> loggerWorkflowTaskExecutor, ILogger<WorkflowTaskMonitor> loggerWorkflowTaskMonitor, MetricsCollector metrics = null)
        {
            _logger = logger;
            _client = client;
            _workers = new HashSet<IWorkflowTaskExecutor>();
            _loggerWorkflowTaskExecutor = loggerWorkflowTaskExecutor;
            _loggerWorkflowTaskMonitor = loggerWorkflowTaskMonitor;
            _metrics = metrics;
        }

        public async Task Start(CancellationToken token)
        {
            if (token != CancellationToken.None)
                token.ThrowIfCancellationRequested();

            _logger.LogDebug("Starting workers...");
            DiscoverWorkers();
            var runningWorkers = new List<Task>();
            foreach (var worker in _workers)
            {
                var runningWorker = worker.Start(token);
                runningWorkers.Add(runningWorker);
            }
            _logger.LogDebug("Started all workers");
            await Task.WhenAll(runningWorkers);
        }

        public void RegisterWorker(IWorkflowTask worker)
        {
            var workflowTaskMonitor = new WorkflowTaskMonitor(_loggerWorkflowTaskMonitor);
            var workflowTaskExecutor = new WorkflowTaskExecutor(
                _loggerWorkflowTaskExecutor,
                _client,
                worker,
                workflowTaskMonitor,
                _metrics
            );
            _workers.Add(workflowTaskExecutor);
        }

        private void DiscoverWorkers()
        {
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
                    }
                }
            }
        }
    }
}
