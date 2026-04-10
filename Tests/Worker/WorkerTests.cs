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
using conductor.csharp.Client.Extensions;
using Conductor.Api;
using Conductor.Client.Extensions;
using Conductor.Client.Models;
using Conductor.Definition;
using Conductor.Definition.TaskType;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace Tests.Worker
{
    [Trait("Category", "CloudIntegration")]
    public class WorkerTests
    {
        private const string WORKFLOW_NAME = "test-sdk-csharp-worker";
        private const int WORKFLOW_VERSION = 1;
        private const string TASK_NAME = "test-sdk-csharp-task";
        private const string TASK_DOMAIN = "taskDomain";
        private const string OWNER_EMAIL = "test-sdk-csharp@conductor.com";

        private readonly WorkflowResourceApi _workflowClient;
        private readonly ILogger _logger;

        public WorkerTests()
        {
            _workflowClient = ApiExtensions.GetClient<WorkflowResourceApi>();
            _logger = ApplicationLogging.CreateLogger<WorkerTests>();
        }

        [Fact]
        public async System.Threading.Tasks.Task TestWorkflowAsyncExecution()
        {
            var workflow = GetConductorWorkflow();
            ApiExtensions.GetWorkflowExecutor().RegisterWorkflow(workflow, true);
            var workflowIdList = await StartWorkflows(workflow, quantity: 15);
            await ExecuteWorkflowTasks(workflowCompletionTimeout: TimeSpan.FromSeconds(30));
            await ValidateWorkflowCompletion(workflowIdList.ToArray());
        }

        private ConductorWorkflow GetConductorWorkflow()
        {
            return new ConductorWorkflow()
                .WithName(WORKFLOW_NAME)
                .WithVersion(WORKFLOW_VERSION)
                .WithTask(new SimpleTask(TASK_NAME, TASK_NAME))
                .WithOwner(OWNER_EMAIL);
        }

        private async System.Threading.Tasks.Task<ConcurrentBag<string>> StartWorkflows(ConductorWorkflow workflow, int quantity)
        {
            var startWorkflowRequest = workflow.GetStartWorkflowRequest();
            startWorkflowRequest.TaskToDomain = new Dictionary<string, string> { { TASK_NAME, TASK_DOMAIN } };
            var startedWorkflows = await WorkflowExtensions.StartWorkflows(
                _workflowClient,
                startWorkflowRequest,
                maxAllowedInParallel: 3,
                total: quantity
            );
            return startedWorkflows;
        }

        private async System.Threading.Tasks.Task ExecuteWorkflowTasks(TimeSpan workflowCompletionTimeout)
        {
            var host = WorkflowTaskHost.CreateWorkerHost(Microsoft.Extensions.Logging.LogLevel.Information, new ClassWorker());
            await host.StartAsync();
            Thread.Sleep(workflowCompletionTimeout);
            await host.StopAsync();
        }

        private async System.Threading.Tasks.Task ValidateWorkflowCompletion(params string[] workflowIdList)
        {
            var workflowStatusList = await WorkflowExtensions.GetWorkflowStatusList(
                _workflowClient,
                maxAllowedInParallel: 10,
                workflowIdList
            );
            var incompleteWorkflowCounter = 0;
            foreach (var workflowStatus in workflowStatusList)
            {
                if (workflowStatus.Status.Value != WorkflowStatus.StatusEnum.COMPLETED)
                {
                    incompleteWorkflowCounter += 1;
                    _logger.LogInformation($"Workflow not completed, workflowId: {workflowStatus.WorkflowId}");
                }
            }
            Assert.Equal(0, incompleteWorkflowCounter);
        }
    }
}
