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
using Conductor.Api;
using Conductor.Client.Models;
using Conductor.Definition;
using Conductor.Definition.TaskType;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Tests.Integration
{
    [Collection("Conductor")]
    public class TaskIntegrationTests
    {
        private const string WorkflowName = "sdk_task_integration_test_workflow";
        private const string TaskName = "sdk_task_integration_test_task";
        private const string WorkerId = "sdk-test-worker";
        private const string OwnerEmail = "test@conductor.sdk";

        private readonly WorkflowResourceApi _workflowClient;
        private readonly MetadataResourceApi _metadataClient;
        private readonly TaskResourceApi _taskClient;

        public TaskIntegrationTests(ConductorFixture fixture)
        {
            _workflowClient = fixture.Configuration.GetClient<WorkflowResourceApi>();
            _metadataClient = fixture.Configuration.GetClient<MetadataResourceApi>();
            _taskClient = fixture.Configuration.GetClient<TaskResourceApi>();

            RegisterWorkflow();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void PollTask_AfterWorkflowStart_ReturnsTask()
        {
            _workflowClient.StartWorkflow(new StartWorkflowRequest(name: WorkflowName));

            var tasks = _taskClient.BatchPoll(TaskName, WorkerId, domain: null, count: 1);

            Assert.NotNull(tasks);
            Assert.NotEmpty(tasks);
            Assert.Equal(TaskName, tasks.First().TaskDefName);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CompleteTask_WorkflowReachesCompletedStatus()
        {
            var workflowId = _workflowClient.StartWorkflow(new StartWorkflowRequest(name: WorkflowName));
            var tasks = _taskClient.BatchPoll(TaskName, WorkerId, domain: null, count: 1);
            Assert.NotEmpty(tasks);

            var task = tasks.First();
            _taskClient.UpdateTask(new TaskResult
            {
                TaskId = task.TaskId,
                WorkflowInstanceId = workflowId,
                Status = TaskResult.StatusEnum.COMPLETED,
                OutputData = new Dictionary<string, object> { { "result", "ok" } }
            });

            var workflow = _workflowClient.GetExecutionStatus(workflowId);
            Assert.Equal(Workflow.StatusEnum.COMPLETED, workflow.Status);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void FailTask_WorkflowReachesFailedStatus()
        {
            var workflowId = _workflowClient.StartWorkflow(new StartWorkflowRequest(name: WorkflowName));
            var tasks = _taskClient.BatchPoll(TaskName, WorkerId, domain: null, count: 1);
            Assert.NotEmpty(tasks);

            var task = tasks.First();
            _taskClient.UpdateTask(new TaskResult
            {
                TaskId = task.TaskId,
                WorkflowInstanceId = workflowId,
                Status = TaskResult.StatusEnum.FAILED,
                ReasonForIncompletion = "deliberate failure in test"
            });

            var workflow = _workflowClient.GetExecutionStatus(workflowId);
            Assert.Equal(Workflow.StatusEnum.FAILED, workflow.Status);
        }

        private void RegisterWorkflow()
        {
            var taskDef = new TaskDef(name: TaskName) { RetryCount = 0 };
            _metadataClient.RegisterTaskDef(new List<TaskDef> { taskDef });

            var workflow = new ConductorWorkflow()
                .WithName(WorkflowName)
                .WithVersion(1)
                .WithOwner(OwnerEmail)
                .WithTask(new SimpleTask(TaskName, TaskName));

            _metadataClient.UpdateWorkflowDefinitions(new List<WorkflowDef> { workflow }, true);
        }
    }
}
