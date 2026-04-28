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

namespace Tests.Integration.Orkes
{
    [Collection("Integration")]
    [Trait("Category", "CloudIntegration")]
    public class TagTests : IClassFixture<ConductorFixture>
    {
        private readonly TagsApi _tagsClient;
        private readonly MetadataResourceApi _metadataClient;
        private readonly string _workflowName;
        private readonly string _taskName;

        public TagTests(ConductorFixture fixture)
        {
            _tagsClient = fixture.Configuration.GetClient<TagsApi>();
            _metadataClient = fixture.Configuration.GetClient<MetadataResourceApi>();
            _workflowName = TestPrefix.Name("tag_wf");
            _taskName = TestPrefix.Name("tag_task");

            _metadataClient.UpdateWorkflowDefinitions(new List<WorkflowDef>
            {
                new ConductorWorkflow()
                    .WithName(_workflowName)
                    .WithVersion(1)
                    .WithOwner("sdk-test@conductor.io")
                    .WithTask(new SimpleTask(_taskName, _taskName))
            }, true);
            _metadataClient.RegisterTaskDef(new List<TaskDef> { new TaskDef(name: _taskName) });
        }

        [Fact]
        public void AddWorkflowTag_CanBeRetrieved()
        {
            var tag = BuildTagObject("env", "test");
            _tagsClient.AddWorkflowTag(tag, _workflowName);
            var tags = _tagsClient.GetWorkflowTags(_workflowName);
            Assert.Contains(tags, t => t.Key == "env" && t.Value.ToString() == "test");
            CleanupWorkflow();
        }

        [Fact]
        public void DeleteWorkflowTag_RemovedFromList()
        {
            var tag = BuildTagObject("env", "test");
            _tagsClient.AddWorkflowTag(tag, _workflowName);
            _tagsClient.DeleteWorkflowTag(tag, _workflowName);
            var tags = _tagsClient.GetWorkflowTags(_workflowName);
            Assert.DoesNotContain(tags, t => t.Key == "env");
        }

        [Fact]
        public void AddTaskTag_CanBeRetrieved()
        {
            var tag = BuildTagObject("env", "test");
            _tagsClient.AddTaskTag(tag, _taskName);
            var tags = _tagsClient.GetTaskTags(_taskName);
            Assert.Contains(tags, t => t.Key == "env" && t.Value.ToString() == "test");
            CleanupTask();
        }

        [Fact]
        public void DeleteTaskTag_RemovedFromList()
        {
            var tag = BuildTagObject("env", "test");
            _tagsClient.AddTaskTag(tag, _taskName);
            _tagsClient.DeleteTaskTag(BuildTagString("env", "test"), _taskName);
            var tags = _tagsClient.GetTaskTags(_taskName);
            Assert.DoesNotContain(tags, t => t.Key == "env");
        }

        [Fact]
        public void SetWorkflowTags_ReplacesAllTags()
        {
            _tagsClient.AddWorkflowTag(BuildTagObject("old", "value"), _workflowName);
            _tagsClient.SetWorkflowTags(new List<TagObject>
            {
                BuildTagObject("new1", "v1"),
                BuildTagObject("new2", "v2")
            }, _workflowName);
            var tags = _tagsClient.GetWorkflowTags(_workflowName);
            Assert.DoesNotContain(tags, t => t.Key == "old");
            Assert.Contains(tags, t => t.Key == "new1");
            Assert.Contains(tags, t => t.Key == "new2");
        }

        [Fact]
        public void SetTaskTags_ReplacesAllTags()
        {
            _tagsClient.AddTaskTag(BuildTagObject("old", "value"), _taskName);
            _tagsClient.SetTaskTags(new List<TagObject>
            {
                BuildTagObject("new1", "v1"),
                BuildTagObject("new2", "v2")
            }, _taskName);
            var tags = _tagsClient.GetTaskTags(_taskName);
            Assert.DoesNotContain(tags, t => t.Key == "old");
            Assert.Contains(tags, t => t.Key == "new1");
            Assert.Contains(tags, t => t.Key == "new2");
        }

        private TagObject BuildTagObject(string key, string value) =>
            new TagObject { Key = key, Value = value, Type = TagObject.TypeEnum.METADATA };

        private TagString BuildTagString(string key, string value = null) =>
            new TagString { Key = key, Type = TagString.TypeEnum.METADATA, Value = value };

        private void CleanupWorkflow()
        {
            try { _tagsClient.DeleteWorkflowTag(BuildTagObject("env", "test"), _workflowName); } catch { }
        }

        private void CleanupTask()
        {
            try { _tagsClient.DeleteTaskTag(BuildTagString("env"), _taskName); } catch { }
        }
    }
}
