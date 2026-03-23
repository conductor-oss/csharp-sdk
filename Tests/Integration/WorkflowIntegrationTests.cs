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
using Xunit;

namespace Tests.Integration
{
    [Collection("Conductor")]
    public class WorkflowIntegrationTests
    {
        private const string WorkflowName = "sdk_integration_test_workflow";
        private const string TaskName = "sdk_integration_test_task";
        private const string OwnerEmail = "test@conductor.sdk";

        private readonly WorkflowResourceApi _workflowClient;
        private readonly MetadataResourceApi _metadataClient;

        public WorkflowIntegrationTests(ConductorFixture fixture)
        {
            _workflowClient = fixture.Configuration.GetClient<WorkflowResourceApi>();
            _metadataClient = fixture.Configuration.GetClient<MetadataResourceApi>();

            RegisterWorkflow();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void StartWorkflow_ReturnsWorkflowId()
        {
            var id = _workflowClient.StartWorkflow(new StartWorkflowRequest(name: WorkflowName));
            Assert.NotNull(id);
            Assert.NotEmpty(id);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void StartWorkflow_StatusIsRunning()
        {
            var id = _workflowClient.StartWorkflow(new StartWorkflowRequest(name: WorkflowName));
            var workflow = _workflowClient.GetExecutionStatus(id);
            Assert.Equal(Workflow.StatusEnum.RUNNING, workflow.Status);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void PauseAndResumeWorkflow_StatusTransitions()
        {
            var id = _workflowClient.StartWorkflow(new StartWorkflowRequest(name: WorkflowName));

            _workflowClient.PauseWorkflow(id);
            Assert.Equal(Workflow.StatusEnum.PAUSED, _workflowClient.GetExecutionStatus(id).Status);

            _workflowClient.ResumeWorkflow(id);
            Assert.Equal(Workflow.StatusEnum.RUNNING, _workflowClient.GetExecutionStatus(id).Status);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void TerminateWorkflow_StatusIsTerminated()
        {
            var id = _workflowClient.StartWorkflow(new StartWorkflowRequest(name: WorkflowName));
            _workflowClient.Terminate(id);
            Assert.Equal(Workflow.StatusEnum.TERMINATED, _workflowClient.GetExecutionStatus(id).Status);
        }

        private void RegisterWorkflow()
        {
            var workflow = new ConductorWorkflow()
                .WithName(WorkflowName)
                .WithVersion(1)
                .WithOwner(OwnerEmail)
                .WithVariable("key1", "default1")
                .WithVariable("key2", 0)
                .WithTask(new SimpleTask(TaskName, TaskName));

            _metadataClient.UpdateWorkflowDefinitions(new List<WorkflowDef> { workflow }, true);
        }
    }
}
