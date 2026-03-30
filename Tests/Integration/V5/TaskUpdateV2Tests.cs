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
using Conductor.Client;
using Conductor.Client.Models;
using Conductor.Definition;
using Conductor.Definition.TaskType;
using System.Collections.Generic;
using Tests.Integration.Helpers;
using Xunit;

namespace Tests.Integration.V5
{
    /// <summary>
    /// Tests for task-update-v2 (PUT /tasks/{taskId}).
    /// Runs only against v5 — excluded from v4 CI job via Version!=V5Only filter.
    /// </summary>
    [Collection("Integration")]
    [Trait("Category", "Integration")]
    [Trait("Version", "V5Only")]
    public class TaskUpdateV2Tests : IClassFixture<ConductorFixture>
    {
        private readonly WorkflowResourceApi _workflowClient;
        private readonly TaskResourceApi _taskClient;
        private readonly MetadataResourceApi _metadataClient;
        private readonly string _workflowName;
        private readonly string _taskName;
        private const string WorkerId = "csharp-sdk-test-worker";

        public TaskUpdateV2Tests(ConductorFixture fixture)
        {
            _workflowClient = fixture.Configuration.GetClient<WorkflowResourceApi>();
            _taskClient = fixture.Configuration.GetClient<TaskResourceApi>();
            _metadataClient = fixture.Configuration.GetClient<MetadataResourceApi>();
            _workflowName = TestPrefix.Name("v2_wf");
            _taskName = TestPrefix.Name("v2_task");

            _metadataClient.RegisterTaskDef(new List<TaskDef> { new TaskDef(name: _taskName) { RetryCount = 0 } });
            _metadataClient.UpdateWorkflowDefinitions(new List<WorkflowDef>
            {
                new ConductorWorkflow()
                    .WithName(_workflowName)
                    .WithVersion(1)
                    .WithOwner("sdk-test@conductor.io")
                    .WithTask(new SimpleTask(_taskName, _taskName))
            }, true);
        }

        [Fact]
        public void UpdateTask_CompletesTask_WorkflowCompletes()
        {
            var workflowId = StartWorkflow();
            var task = PollTask();

            _taskClient.UpdateTask(new TaskResult
            {
                TaskId = task.TaskId,
                WorkflowInstanceId = workflowId,
                Status = TaskResult.StatusEnum.COMPLETED,
                OutputData = new Dictionary<string, object> { { "result", "ok" } }
            });

            Assert.Equal(Conductor.Client.Models.Workflow.StatusEnum.COMPLETED, GetWorkflowStatus(workflowId));
            Cleanup(workflowId);
        }

        [Fact]
        public void UpdateTask_FailTask_WorkflowFails()
        {
            var workflowId = StartWorkflow();
            var task = PollTask();

            _taskClient.UpdateTask(new TaskResult
            {
                TaskId = task.TaskId,
                WorkflowInstanceId = workflowId,
                Status = TaskResult.StatusEnum.FAILED,
                ReasonForIncompletion = "v5 test failure"
            });

            Assert.Equal(Conductor.Client.Models.Workflow.StatusEnum.FAILED, GetWorkflowStatus(workflowId));
            Cleanup(workflowId);
        }

        private string StartWorkflow() =>
            _workflowClient.StartWorkflow(new StartWorkflowRequest(name: _workflowName));

        private Conductor.Client.Models.Task PollTask()
        {
            Conductor.Client.Models.Task task = null;
            for (var i = 0; i < 10 && task == null; i++)
            {
                task = _taskClient.Poll(_taskName, WorkerId);
                if (task == null) System.Threading.Thread.Sleep(500);
            }
            Assert.NotNull(task);
            return task;
        }

        private Conductor.Client.Models.Workflow.StatusEnum? GetWorkflowStatus(string id)
        {
            for (var i = 0; i < 10; i++)
            {
                var status = _workflowClient.GetExecutionStatus(id).Status;
                if (status != Conductor.Client.Models.Workflow.StatusEnum.RUNNING)
                    return status;
                System.Threading.Thread.Sleep(500);
            }
            return _workflowClient.GetExecutionStatus(id).Status;
        }

        private void Cleanup(string workflowId)
        {
            try { _workflowClient.Terminate(workflowId); } catch { }
            try { _workflowClient.Delete(workflowId); } catch { }
        }
    }
}
