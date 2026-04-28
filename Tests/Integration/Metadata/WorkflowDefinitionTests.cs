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

namespace Tests.Integration.Metadata
{
    [Collection("Integration")]
    [Trait("Category", "CloudIntegration")]
    public class WorkflowDefinitionTests : IClassFixture<ConductorFixture>
    {
        private readonly MetadataResourceApi _metadataClient;
        private readonly string _workflowName;
        private readonly string _taskName;

        public WorkflowDefinitionTests(ConductorFixture fixture)
        {
            _metadataClient = fixture.Configuration.GetClient<MetadataResourceApi>();
            _workflowName = TestPrefix.Name("wfdef_wf");
            _taskName = TestPrefix.Name("wfdef_task");
        }

        [Fact]
        public void RegisterWorkflow_CanBeRetrieved()
        {
            Register();
            var def = _metadataClient.Get(_workflowName, 1);
            Assert.NotNull(def);
            Assert.Equal(_workflowName, def.Name);
            Cleanup();
        }

        [Fact]
        public void UpdateWorkflow_ChangesAreReflected()
        {
            Register();
            var updated = BuildWorkflow();
            updated.Description = "updated description";
            _metadataClient.UpdateWorkflowDefinitions(new List<WorkflowDef> { updated }, true);

            var fetched = _metadataClient.Get(_workflowName, 1);
            Assert.Equal("updated description", fetched.Description);
            Cleanup();
        }

        [Fact]
        public void GetAllWorkflows_ContainsRegistered()
        {
            Register();
            var all = _metadataClient.GetAllWorkflows();
            Assert.Contains(all, w => w.Name == _workflowName);
            Cleanup();
        }

        [Fact]
        public void DeleteWorkflow_RemovedFromList()
        {
            Register();
            _metadataClient.UnregisterWorkflowDef(_workflowName, 1);
            var all = _metadataClient.GetAllWorkflows();
            Assert.DoesNotContain(all, w => w.Name == _workflowName);
        }

        private ConductorWorkflow BuildWorkflow() =>
            new ConductorWorkflow()
                .WithName(_workflowName)
                .WithVersion(1)
                .WithOwner("sdk-test@conductor.io")
                .WithTask(new SimpleTask(_taskName, _taskName));

        private void Register() =>
            _metadataClient.UpdateWorkflowDefinitions(new List<WorkflowDef> { BuildWorkflow() }, true);

        private void Cleanup()
        {
            try { _metadataClient.UnregisterWorkflowDef(_workflowName, 1); } catch { }
        }
    }
}
