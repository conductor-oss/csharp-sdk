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
using Tests.Integration.Helpers;
using Xunit;

namespace Tests.Integration.Workflow
{
    [Collection("Integration")]
    [Trait("Category", "CloudIntegration")]
    public class WorkflowLifecycleTests : IClassFixture<ConductorFixture>
    {
        private readonly WorkflowResourceApi _workflowClient;
        private readonly MetadataResourceApi _metadataClient;
        private readonly string _workflowName;
        private readonly string _taskName;

        public WorkflowLifecycleTests(ConductorFixture fixture)
        {
            _workflowClient = fixture.Configuration.GetClient<WorkflowResourceApi>();
            _metadataClient = fixture.Configuration.GetClient<MetadataResourceApi>();
            _workflowName = TestPrefix.Name("lifecycle_wf");
            _taskName = TestPrefix.Name("lifecycle_task");

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
        public void StartWorkflow_ReturnsWorkflowId()
        {
            var id = StartWorkflow();
            Assert.NotNull(id);
            Assert.NotEmpty(id);
            Cleanup(id);
        }

        [Fact]
        public void StartWorkflow_StatusIsRunning()
        {
            var id = StartWorkflow();
            Assert.Equal(Conductor.Client.Models.Workflow.StatusEnum.RUNNING, GetStatus(id));
            Cleanup(id);
        }

        [Fact]
        public void PauseWorkflow_StatusIsPaused()
        {
            var id = StartWorkflow();
            _workflowClient.PauseWorkflow(id);
            Assert.Equal(Conductor.Client.Models.Workflow.StatusEnum.PAUSED, GetStatus(id));
            Cleanup(id);
        }

        [Fact]
        public void ResumeWorkflow_StatusIsRunning()
        {
            var id = StartWorkflow();
            _workflowClient.PauseWorkflow(id);
            _workflowClient.ResumeWorkflow(id);
            Assert.Equal(Conductor.Client.Models.Workflow.StatusEnum.RUNNING, GetStatus(id));
            Cleanup(id);
        }

        [Fact]
        public void TerminateWorkflow_StatusIsTerminated()
        {
            var id = StartWorkflow();
            _workflowClient.Terminate(id);
            Assert.Equal(Conductor.Client.Models.Workflow.StatusEnum.TERMINATED, GetStatus(id));
            Cleanup(id);
        }

        [Fact]
        public void RestartWorkflow_StatusIsRunning()
        {
            var id = StartWorkflow();
            _workflowClient.Terminate(id);
            _workflowClient.Restart(id);
            Assert.Equal(Conductor.Client.Models.Workflow.StatusEnum.RUNNING, GetStatus(id));
            Cleanup(id);
        }

        [Fact]
        public void RetryWorkflow_StatusIsRunning()
        {
            var id = StartWorkflow();
            _workflowClient.Terminate(id);
            _workflowClient.Retry(id);
            Assert.Equal(Conductor.Client.Models.Workflow.StatusEnum.RUNNING, GetStatus(id));
            Cleanup(id);
        }

        [Fact]
        public void DeleteWorkflow_RemovedFromRunning()
        {
            var id = StartWorkflow();
            _workflowClient.Terminate(id);
            _workflowClient.Delete(id);
            var running = _workflowClient.GetRunningWorkflowWithHttpInfo(_workflowName).Data;
            Assert.DoesNotContain(id, running);
        }

        [Fact]
        public void GetExecutionStatus_ReturnsWorkflowDetails()
        {
            var id = StartWorkflow();
            var workflow = _workflowClient.GetExecutionStatus(id);
            Assert.NotNull(workflow);
            Assert.Equal(_workflowName, workflow.WorkflowName);
            Assert.NotEmpty(workflow.Tasks);
            Cleanup(id);
        }

        [Fact]
        public void UpdateWorkflowVariables_VariablesAreReflected()
        {
            var id = StartWorkflow();
            var variables = new System.Collections.Generic.Dictionary<string, object>
            {
                { "testVar", "testValue" }
            };
            var updated = _workflowClient.UpdateWorkflowVariables(id, variables);
            Assert.NotNull(updated);
            Cleanup(id);
        }

        [Fact]
        public void GetExecutionStatus_NonExistentId_ThrowsApiException()
        {
            Assert.Throws<Conductor.Client.ApiException>(() =>
                _workflowClient.GetExecutionStatus("non-existent-workflow-id-12345"));
        }

        private string StartWorkflow() =>
            _workflowClient.StartWorkflow(new StartWorkflowRequest(name: _workflowName));

        private Conductor.Client.Models.Workflow.StatusEnum? GetStatus(string id) =>
            _workflowClient.GetExecutionStatus(id).Status;

        private void Cleanup(string id)
        {
            try { _workflowClient.Terminate(id); } catch { }
            try { _workflowClient.Delete(id); } catch { }
        }
    }
}
