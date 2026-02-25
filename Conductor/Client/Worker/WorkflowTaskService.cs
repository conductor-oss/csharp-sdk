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
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Logging;

namespace Conductor.Client.Worker
{
    public class WorkflowTaskService : BackgroundService
    {
        private readonly IWorkflowTaskCoordinator _workflowTaskCoordinator;
        private readonly IEnumerable<IWorkflowTask> _workers;
        private readonly ILogger<WorkflowTaskService> _logger;

        public WorkflowTaskService(IWorkflowTaskCoordinator workflowTaskCoordinator, IEnumerable<IWorkflowTask> workers, ILogger<WorkflowTaskService> logger)
        {
            _workflowTaskCoordinator = workflowTaskCoordinator;
            _workers = workers;
            _logger = logger;
        }

        protected override System.Threading.Tasks.Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogTrace($"Task Service execution started.....");
                foreach (var worker in _workers)
                {
                    _workflowTaskCoordinator.RegisterWorker(worker);
                }
                return _workflowTaskCoordinator.Start(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Task Service execution error out.....Message: {ex.Message}, Exception Stack trace: {ex.StackTrace}");
                StopAsync(stoppingToken);
                throw;
            }
        }
    }
}
