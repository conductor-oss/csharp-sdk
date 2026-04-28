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
using Tests.Integration.Helpers;
using Xunit;

namespace Tests.Integration.Workflow
{
    [Collection("Integration")]
    [Trait("Category", "CloudIntegration")]
    public class WorkflowBulkTests : IClassFixture<ConductorFixture>
    {
        private readonly WorkflowResourceApi _workflowClient;
        private readonly WorkflowBulkResourceApi _bulkClient;
        private readonly MetadataResourceApi _metadataClient;
        private readonly string _workflowName;
        private readonly string _taskName;

        public WorkflowBulkTests(ConductorFixture fixture)
        {
            _workflowClient = fixture.Configuration.GetClient<WorkflowResourceApi>();
            _bulkClient = fixture.Configuration.GetClient<WorkflowBulkResourceApi>();
            _metadataClient = fixture.Configuration.GetClient<MetadataResourceApi>();
            _workflowName = TestPrefix.Name("bulk_wf");
            _taskName = TestPrefix.Name("bulk_task");

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
        public void BulkPause_PausesAllWorkflows()
        {
            var ids = StartWorkflows(3);
            _bulkClient.PauseWorkflow(ids);
            foreach (var id in ids)
                Assert.Equal(Conductor.Client.Models.Workflow.StatusEnum.PAUSED, GetStatus(id));
            CleanupAll(ids);
        }

        [Fact]
        public void BulkResume_ResumesAllWorkflows()
        {
            var ids = StartWorkflows(3);
            _bulkClient.PauseWorkflow(ids);
            _bulkClient.ResumeWorkflow(ids);
            foreach (var id in ids)
                Assert.Equal(Conductor.Client.Models.Workflow.StatusEnum.RUNNING, GetStatus(id));
            CleanupAll(ids);
        }

        [Fact]
        public void BulkTerminate_TerminatesAllWorkflows()
        {
            var ids = StartWorkflows(3);
            _bulkClient.Terminate(ids);
            foreach (var id in ids)
                Assert.Equal(Conductor.Client.Models.Workflow.StatusEnum.TERMINATED, GetStatus(id));
            CleanupAll(ids);
        }

        [Fact]
        public void BulkRestart_RestartsAllWorkflows()
        {
            var ids = StartWorkflows(3);
            _bulkClient.Terminate(ids);
            _bulkClient.Restart(ids);
            foreach (var id in ids)
                Assert.Equal(Conductor.Client.Models.Workflow.StatusEnum.RUNNING, GetStatus(id));
            CleanupAll(ids);
        }

        [Fact]
        public void BulkRetry_RetriesAllWorkflows()
        {
            var ids = StartWorkflows(3);
            _bulkClient.Terminate(ids);
            _bulkClient.Retry(ids);
            foreach (var id in ids)
                Assert.Equal(Conductor.Client.Models.Workflow.StatusEnum.RUNNING, GetStatus(id));
            CleanupAll(ids);
        }

        private List<string> StartWorkflows(int count) =>
            Enumerable.Range(0, count)
                .Select(_ => _workflowClient.StartWorkflow(new StartWorkflowRequest(name: _workflowName)))
                .ToList();

        private Conductor.Client.Models.Workflow.StatusEnum? GetStatus(string id) =>
            _workflowClient.GetExecutionStatus(id).Status;

        private void CleanupAll(List<string> ids)
        {
            foreach (var id in ids)
            {
                try { _workflowClient.Terminate(id); } catch { }
                try { _workflowClient.Delete(id); } catch { }
            }
        }
    }
}
