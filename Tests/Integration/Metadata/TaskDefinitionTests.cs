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
using System.Collections.Generic;
using Tests.Integration.Helpers;
using Xunit;

namespace Tests.Integration.Metadata
{
    [Collection("Integration")]
    [Trait("Category", "CloudIntegration")]
    public class TaskDefinitionTests : IClassFixture<ConductorFixture>
    {
        private readonly MetadataResourceApi _metadataClient;
        private readonly string _taskName;

        public TaskDefinitionTests(ConductorFixture fixture)
        {
            _metadataClient = fixture.Configuration.GetClient<MetadataResourceApi>();
            _taskName = TestPrefix.Name("taskdef");
        }

        [Fact]
        public void RegisterTaskDef_CanBeRetrieved()
        {
            Register();
            var def = _metadataClient.GetTaskDef(_taskName);
            Assert.NotNull(def);
            Assert.Equal(_taskName, def.Name);
            Cleanup();
        }

        [Fact]
        public void UpdateTaskDef_ChangesAreReflected()
        {
            Register();
            var def = _metadataClient.GetTaskDef(_taskName);
            def.Description = "updated";
            def.RetryCount = 5;
            _metadataClient.UpdateTaskDef(def);

            var fetched = _metadataClient.GetTaskDef(_taskName);
            Assert.Equal("updated", fetched.Description);
            Assert.Equal(5, fetched.RetryCount);
            Cleanup();
        }

        [Fact]
        public void GetAllTaskDefs_ContainsRegistered()
        {
            Register();
            var all = _metadataClient.GetTaskDefs();
            Assert.Contains(all, t => t.Name == _taskName);
            Cleanup();
        }

        [Fact]
        public void DeleteTaskDef_RemovedFromList()
        {
            Register();
            _metadataClient.UnregisterTaskDef(_taskName);
            var all = _metadataClient.GetTaskDefs();
            Assert.DoesNotContain(all, t => t.Name == _taskName);
        }

        [Fact]
        public void GetTaskDef_NonExistent_ThrowsApiException()
        {
            Assert.Throws<Conductor.Client.ApiException>(() =>
                _metadataClient.GetTaskDef(TestPrefix.Name("nonexistent_task")));
        }

        private void Register() =>
            _metadataClient.RegisterTaskDef(new List<TaskDef>
            {
                new TaskDef(name: _taskName)
                {
                    Description = "SDK integration test task",
                    RetryCount = 0,
                    TimeoutSeconds = 300,
                    ResponseTimeoutSeconds = 300
                }
            });

        private void Cleanup()
        {
            try { _metadataClient.UnregisterTaskDef(_taskName); } catch { }
        }
    }
}
