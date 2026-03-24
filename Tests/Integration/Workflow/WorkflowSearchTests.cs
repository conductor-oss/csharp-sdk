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
using System;
using System.Collections.Generic;
using Tests.Integration.Helpers;
using Xunit;

namespace Tests.Integration.Workflow
{
    [Collection("Integration")]
    [Trait("Category", "Integration")]
    public class WorkflowSearchTests : IClassFixture<ConductorFixture>
    {
        private readonly WorkflowResourceApi _workflowClient;
        private readonly MetadataResourceApi _metadataClient;
        private readonly string _workflowName;
        private readonly string _taskName;

        public WorkflowSearchTests(ConductorFixture fixture)
        {
            _workflowClient = fixture.Configuration.GetClient<WorkflowResourceApi>();
            _metadataClient = fixture.Configuration.GetClient<MetadataResourceApi>();
            _workflowName = TestPrefix.Name("search_wf");
            _taskName = TestPrefix.Name("search_task");

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
        public void Search_ByCorrelationId_ReturnsMatchingWorkflow()
        {
            var correlationId = Guid.NewGuid().ToString();
            var id = _workflowClient.StartWorkflow(new StartWorkflowRequest(
                name: _workflowName,
                correlationId: correlationId));

            System.Threading.Thread.Sleep(2000);

            var results = _workflowClient.Search(
                start: 0, size: 10, freeText: "*",
                query: $"correlationId = \"{correlationId}\"");

            Assert.NotNull(results);
            Assert.Contains(results.Results, r => r.WorkflowId == id);
            Cleanup(id);
        }

        [Fact]
        public void Search_ByStatus_ReturnsRunningWorkflows()
        {
            var id = _workflowClient.StartWorkflow(new StartWorkflowRequest(name: _workflowName));

            System.Threading.Thread.Sleep(2000);

            var results = _workflowClient.Search(
                start: 0, size: 100, freeText: "*",
                query: $"status = RUNNING AND workflowType = \"{_workflowName}\"");

            Assert.NotNull(results);
            Assert.Contains(results.Results, r => r.WorkflowId == id);
            Cleanup(id);
        }

        [Fact]
        public void GetRunningWorkflows_ReturnsActiveIds()
        {
            var id = _workflowClient.StartWorkflow(new StartWorkflowRequest(name: _workflowName));

            var running = _workflowClient.GetRunningWorkflow(_workflowName);

            Assert.NotNull(running);
            Assert.Contains(id, running);
            Cleanup(id);
        }

        private void Cleanup(string id)
        {
            try { _workflowClient.Terminate(id); } catch { }
            try { _workflowClient.Delete(id); } catch { }
        }
    }
}
