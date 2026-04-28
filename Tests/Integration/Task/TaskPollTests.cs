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

namespace Tests.Integration.Task
{
    [Collection("Integration")]
    [Trait("Category", "CloudIntegration")]
    public class TaskPollTests : IClassFixture<ConductorFixture>
    {
        private readonly WorkflowResourceApi _workflowClient;
        private readonly TaskResourceApi _taskClient;
        private readonly MetadataResourceApi _metadataClient;
        private readonly string _workflowName;
        private readonly string _taskName;
        private const string WorkerId = "csharp-sdk-test-worker";

        public TaskPollTests(ConductorFixture fixture)
        {
            _workflowClient = fixture.Configuration.GetClient<WorkflowResourceApi>();
            _taskClient = fixture.Configuration.GetClient<TaskResourceApi>();
            _metadataClient = fixture.Configuration.GetClient<MetadataResourceApi>();
            _workflowName = TestPrefix.Name("poll_wf");
            _taskName = TestPrefix.Name("poll_task");

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
        public void Poll_AfterWorkflowStart_ReturnsTask()
        {
            var id = StartWorkflow();
            var task = _taskClient.Poll(_taskName, WorkerId);
            Assert.NotNull(task);
            Assert.Equal(_taskName, task.TaskDefName);
            Cleanup(id);
        }

        [Fact]
        public void BatchPoll_ReturnsUpToRequestedCount()
        {
            var ids = new List<string> { StartWorkflow(), StartWorkflow(), StartWorkflow() };
            var tasks = _taskClient.BatchPoll(_taskName, WorkerId, domain: null, count: 2);
            Assert.NotNull(tasks);
            Assert.True(tasks.Count <= 2);
            ids.ForEach(Cleanup);
        }

        [Fact]
        public void Poll_EmptyQueue_ReturnsNull()
        {
            var task = _taskClient.Poll(TestPrefix.Name("nonexistent_task"), WorkerId);
            Assert.Null(task);
        }

        [Fact]
        public void GetPollData_ReturnsPollingInfo()
        {
            var id = StartWorkflow();
            _taskClient.Poll(_taskName, WorkerId); // ensure at least one poll
            var pollData = _taskClient.GetPollData(_taskName);
            Assert.NotNull(pollData);
            Cleanup(id);
        }

        private string StartWorkflow() =>
            _workflowClient.StartWorkflow(new StartWorkflowRequest(name: _workflowName));

        private void Cleanup(string id)
        {
            try
            {
                var tasks = _taskClient.BatchPoll(_taskName, WorkerId, domain: null, count: 10);
                foreach (var t in tasks ?? new List<Conductor.Client.Models.Task>())
                    _taskClient.UpdateTask(new TaskResult { TaskId = t.TaskId, WorkflowInstanceId = id, Status = TaskResult.StatusEnum.COMPLETED });
            }
            catch { }
            try { _workflowClient.Terminate(id); } catch { }
            try { _workflowClient.Delete(id); } catch { }
        }
    }
}
