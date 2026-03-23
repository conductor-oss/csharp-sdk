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
using conductor.csharp.Client.Extensions;
using Conductor.Api;
using Conductor.Client.Extensions;
using Conductor.Client.Models;
using Conductor.Definition;
using Conductor.Definition.TaskType;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using conductor_csharp.Api;
using Tests.Worker;
using Xunit;
using Xunit.Abstractions;
using Task = Conductor.Client.Models.Task;

namespace conductor_csharp.test.Api
{
    [Trait("Category", "CloudIntegration")]
    public class WorkflowResourceApiTest
    {
        private const string WORKFLOW_NAME = "TestToCreateVariables";
        private const string TASK_NAME = "TesttoCreateVaribles_task";
        private const string WORKFLOW_VARIABLE_1 = "TestVariable1";
        private const string WORKFLOW_VARIABLE_2 = "TestVariable2";
        private const string WORKFLOW_DESC = "Test Workflow With Variables";
        private const int WORKFLOW_VERSION = 1;
        private const string OWNER_EMAIL = "developer@orkes.io";

        private readonly WorkflowResourceApi _workflowClient;
        private readonly ITaskResourceApi _taskClient;
        private readonly MetadataResourceApi _metadataResourceApi;
        private readonly ILogger _logger;
        private readonly ITestOutputHelper _testOutputHelper;

        public WorkflowResourceApiTest(ITestOutputHelper testOutputHelper)
        {
            _workflowClient = ApiExtensions.GetClient<WorkflowResourceApi>();
            _taskClient = ApiExtensions.GetClient<TaskResourceApi>();
            _metadataResourceApi = ApiExtensions.GetClient<MetadataResourceApi>();
            _logger = ApplicationLogging.CreateLogger<WorkerTests>();
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async void ResumeWorkflow()
        {
            var workflowId = RegisterAndStartWorkflow();

            _workflowClient.PauseWorkflow(workflowId);
            _workflowClient.ResumeWorkflow(workflowId);
            var res = _workflowClient.GetExecutionStatus(workflowId);
            Assert.NotNull(res);
            Assert.Equal(Workflow.StatusEnum.RUNNING, res.Status);
            DeleteWorkflowExecution(workflowId);
        }

        [Fact]
        public async void DeleteWorkflow()
        {
            var workflowId = RegisterAndStartWorkflow();

            _workflowClient.Delete(workflowId);
            var res = _workflowClient.GetRunningWorkflowWithHttpInfo(WORKFLOW_NAME);
            Assert.NotNull(res);
            Assert.DoesNotContain(workflowId, res.Data);
        }

        [Fact]
        public async void TerminateWorkflow()
        {
            var workflowId = RegisterAndStartWorkflow();

            _workflowClient.Terminate(workflowId);
            var res = _workflowClient.GetExecutionStatus(workflowId);
            Assert.NotNull(res);
            Assert.Equal(Workflow.StatusEnum.TERMINATED, res.Status);
            DeleteWorkflowExecution(workflowId);
        }

        [Fact]
        public async void RetryLastFailedWorkflow()
        {
            var workflowId = RegisterAndStartWorkflow();

            _workflowClient.Terminate(workflowId);
            _workflowClient.Retry(workflowId);
            var res = _workflowClient.GetExecutionStatus(workflowId);
            Assert.NotNull(res);
            Assert.Equal(Workflow.StatusEnum.RUNNING, res.Status);
            DeleteWorkflowExecution(workflowId);
        }


        [Fact]
        public async void PauseWorkflowExecution()
        {
            var workflowId = RegisterAndStartWorkflow();

            _workflowClient.PauseWorkflow(workflowId);
            var res = _workflowClient.GetExecutionStatus(workflowId);
            Assert.NotNull(res);
            Assert.Equal(Workflow.StatusEnum.PAUSED, res.Status);
            DeleteWorkflowExecution(workflowId);
        }

        [Fact]
        public async void UpdateWorkflowVariables()
        {
            // Prepare workflow
            var workflowId = RegisterAndStartWorkflow();

            // Create variables collection with values to be updated
            var updateDict = new Dictionary<string, object> {
                      {WORKFLOW_VARIABLE_1,"Value1" },
                      {WORKFLOW_VARIABLE_2,"Value2" },
                  };

            // Update the work flow variables 
            var _updatedWorkFlow = _workflowClient.UpdateWorkflowVariables(workflowId, updateDict);

            // Verify workflow variables data is equal with input passed 
            Assert.Equal(_updatedWorkFlow.Variables, updateDict);

            DeleteWorkflowExecution(workflowId);
        }

        private string RegisterAndStartWorkflow()
        {
            var _workflow = GetConductorWorkflow();
            _metadataResourceApi.UpdateWorkflowDefinitions(new List<WorkflowDef>(1) { _workflow }, true);
            var workflowId = _workflowClient.StartWorkflow(new StartWorkflowRequest(name: WORKFLOW_NAME));

            return workflowId;
        }

        private ConductorWorkflow GetConductorWorkflow()
        {
            return new ConductorWorkflow()
                .WithName(WORKFLOW_NAME)
                .WithVersion(WORKFLOW_VERSION)
                .WithDescription(WORKFLOW_DESC)
                .WithOwner(OWNER_EMAIL)
                .WithTask(new SimpleTask(TASK_NAME, TASK_NAME))
                .WithVariable(WORKFLOW_VARIABLE_1, $"{WORKFLOW_VARIABLE_1}_Value")
                .WithVariable(WORKFLOW_VARIABLE_2, $"{WORKFLOW_VARIABLE_2}_Value");
        }


        private void DeleteWorkflowExecution(string workflowId)
        {
            _workflowClient.Delete(workflowId);

            var res = _workflowClient.GetRunningWorkflowWithHttpInfo(WORKFLOW_NAME);
            Assert.NotNull(res);
            Assert.DoesNotContain(workflowId, res.Data);
        }

        [Fact]
        public void TestWorkflowOperations()
        {
            // Start Workflow
            var correlationId = Guid.NewGuid().ToString();
            var startWorkflowRequest = new StartWorkflowRequest
            {
                Name = "csharp_sync_task_variable_updates",
                Version = 1,
                Input = new Dictionary<string, object>(),
                CorrelationId = correlationId
            };
            var workflowId = _workflowClient.StartWorkflow(startWorkflowRequest);
            _testOutputHelper.WriteLine($"Started workflow with id {workflowId}");

            // Update a variable inside the workflow 
            _workflowClient.UpdateWorkflowVariables(workflowId, new Dictionary<string, object> { { "case", "case1" } });

            // Get workflow execution status
            var workflow = _workflowClient.GetWorkflow(workflowId, true);
            var lastTask = workflow.Tasks.Last();
            _testOutputHelper.WriteLine(
                $"Workflow status is {workflow.Status} and currently running task is {lastTask.ReferenceTaskName}");

            workflow = _taskClient.UpdateTaskSync(new Dictionary<string, object> { { "a", "b" } }, workflowId,
                lastTask.ReferenceTaskName, TaskResult.StatusEnum.COMPLETED, "test_worker");

            // Get updated workflow status
            lastTask = workflow.Tasks.Last();
            Assert.Equal(lastTask.Status, Task.StatusEnum.INPROGRESS);
            _testOutputHelper.WriteLine(
                $"Workflow status is {workflow.Status} and currently running task is {lastTask.ReferenceTaskName}");

            // Terminate the workflow
            _workflowClient.Terminate(workflowId, "testing termination");
            workflow = _workflowClient.GetWorkflow(workflowId, true);
            Assert.Equal(Workflow.StatusEnum.TERMINATED, workflow.Status);
            lastTask = workflow.Tasks.Last();
            _testOutputHelper.WriteLine(
                $"Workflow status is {workflow.Status} and status of last task {lastTask.Status}");

            // Retry the workflow
            _workflowClient.Retry(workflowId);
            workflow = _workflowClient.GetWorkflow(workflowId, true);
            Assert.Equal(Workflow.StatusEnum.RUNNING, workflow.Status);
            lastTask = workflow.Tasks.Last();
            _testOutputHelper.WriteLine(
                $"Workflow status is {workflow.Status} and status of last task {lastTask.ReferenceTaskName} is {lastTask.Status}");

            // Mark the WAIT task as completed by calling Task completion API
            var taskResult = new TaskResult
            {
                WorkflowInstanceId = workflowId,
                TaskId = lastTask.TaskId,
                Status = TaskResult.StatusEnum.COMPLETED,
                OutputData = new Dictionary<string, object> { { "greetings", "hello from Orkes" } }
            };
            workflow = _taskClient.UpdateTaskSync(
                new Dictionary<string, object> { { "greetings", "hello from Orkes" } },
                workflowId, lastTask.ReferenceTaskName, TaskResult.StatusEnum.COMPLETED, "");

            lastTask = workflow.Tasks.Last();
            Assert.Equal(Task.StatusEnum.SCHEDULED, lastTask.Status);
            _testOutputHelper.WriteLine(
                $"Workflow status is {workflow.Status} and status of last task {lastTask.ReferenceTaskName} is {lastTask.Status}");

            // Terminate the workflow again
            _workflowClient.Terminate(workflowId, "terminating for testing");
            workflow = _workflowClient.GetWorkflow(workflowId, true);
            Assert.Equal(Workflow.StatusEnum.TERMINATED, workflow.Status);


            // Rerun workflow from a specific task
            var rerunRequest = new RerunWorkflowRequest
            {
                ReRunFromTaskId = workflow.Tasks[3].TaskId
            };
            _workflowClient.Rerun(rerunRequest, workflowId);
            workflow = _workflowClient.GetWorkflow(workflowId, true);
            Assert.Equal(Workflow.StatusEnum.RUNNING, workflow.Status);


            // Restart the workflow
            _workflowClient.Terminate(workflowId, "terminating so we can do a restart");
            workflow = _workflowClient.GetWorkflow(workflowId, true);
            Assert.Equal(Workflow.StatusEnum.TERMINATED, workflow.Status);

            _workflowClient.Restart(workflowId);
            workflow = _workflowClient.GetWorkflow(workflowId, true);
            Assert.Equal(Workflow.StatusEnum.RUNNING, workflow.Status);

            // Pause the workflow
            _workflowClient.PauseWorkflow(workflowId);
            workflow = _workflowClient.GetWorkflow(workflowId, true);
            Assert.Equal(Workflow.StatusEnum.PAUSED, workflow.Status);
            _testOutputHelper.WriteLine($"Workflow status is {workflow.Status}");

            workflow = _workflowClient.GetWorkflow(workflowId, true);
            // Wait task should have completed
            var waitTask = workflow.Tasks[0];
            Assert.Equal(Task.StatusEnum.INPROGRESS, waitTask.Status);
            _testOutputHelper.WriteLine($"Workflow status is {workflow.Status} and wait task is {waitTask.Status}");


            // Because workflow is paused, no further task should have been scheduled, making WAIT the last task
            // Expecting only 1 task
            _testOutputHelper.WriteLine($"Number of tasks in workflow is {workflow.Tasks.Count}");
            Assert.Single(workflow.Tasks);

            // Resume the workflow
            _workflowClient.ResumeWorkflow(workflowId);
            lastTask = workflow.Tasks.Last();
            workflow = _taskClient.UpdateTaskSync(new Dictionary<string, object> { { "a", "b" } }, workflowId,
                lastTask.ReferenceTaskName, TaskResult.StatusEnum.COMPLETED, "test_worker");

            workflow = _workflowClient.GetWorkflow(workflowId, true);

            System.Threading.Thread.Sleep(5000);

            // There should be 3 tasks
            _testOutputHelper.WriteLine(
                $"Number of tasks in workflow is {workflow.Tasks.Count} and last task is {workflow.Tasks.Last().ReferenceTaskName}");
            Assert.Equal(3, workflow.Tasks.Count);

            _testOutputHelper.WriteLine("Searching for correlationId " + correlationId);
            // Search for workflows
            var startTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() * 1000;
            var searchResults = _workflowClient.Search(start: 0, size: 10, freeText: "*",
                query: $"correlationId = '{correlationId}'");
            _testOutputHelper.WriteLine(
                $"Found {searchResults.Results.Count} execution with correlation_id '{correlationId}'");
            Assert.Single(searchResults.Results);

            correlationId = Guid.NewGuid().ToString();
            searchResults = _workflowClient.Search(start: 0, size: 100, freeText: "*",
                query: $"status IN (RUNNING) AND correlationId = \"{correlationId}\"");
            // Shouldn't find anything!
            _testOutputHelper.WriteLine(
                $"Found {searchResults.Results.Count} workflows with correlation id {correlationId}");
        }
    }
}
