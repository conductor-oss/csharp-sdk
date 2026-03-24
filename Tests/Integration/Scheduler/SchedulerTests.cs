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

namespace Tests.Integration.Scheduler
{
    [Collection("Integration")]
    [Trait("Category", "Integration")]
    public class SchedulerTests : IClassFixture<ConductorFixture>
    {
        private readonly SchedulerResourceApi _schedulerClient;
        private readonly MetadataResourceApi _metadataClient;
        private readonly string _scheduleName;
        private readonly string _workflowName;
        private readonly string _taskName;

        public SchedulerTests(ConductorFixture fixture)
        {
            _schedulerClient = fixture.Configuration.GetClient<SchedulerResourceApi>();
            _metadataClient = fixture.Configuration.GetClient<MetadataResourceApi>();
            _scheduleName = TestPrefix.Name("schedule");
            _workflowName = TestPrefix.Name("schedule_wf");
            _taskName = TestPrefix.Name("schedule_task");

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
        public void SaveSchedule_CanBeRetrieved()
        {
            Save();
            var schedule = _schedulerClient.GetSchedule(_scheduleName);
            Assert.NotNull(schedule);
            Assert.Equal(_scheduleName, schedule.Name);
            Cleanup();
        }

        [Fact]
        public void GetAllSchedules_ContainsSaved()
        {
            Save();
            var all = _schedulerClient.GetAllSchedules(_workflowName);
            Assert.Contains(all, s => s.Name == _scheduleName);
            Cleanup();
        }

        [Fact]
        public void PauseSchedule_ScheduleIsPaused()
        {
            Save();
            _schedulerClient.PauseSchedule(_scheduleName);
            var schedule = _schedulerClient.GetSchedule(_scheduleName);
            Assert.True(schedule.Paused);
            Cleanup();
        }

        [Fact]
        public void ResumeSchedule_ScheduleIsActive()
        {
            Save();
            _schedulerClient.PauseSchedule(_scheduleName);
            _schedulerClient.ResumeSchedule(_scheduleName);
            var schedule = _schedulerClient.GetSchedule(_scheduleName);
            Assert.False(schedule.Paused);
            Cleanup();
        }

        [Fact]
        public void DeleteSchedule_RemovedFromList()
        {
            Save();
            _schedulerClient.DeleteSchedule(_scheduleName);
            var all = _schedulerClient.GetAllSchedules(_workflowName);
            Assert.DoesNotContain(all, s => s.Name == _scheduleName);
        }

        private void Save() =>
            _schedulerClient.SaveSchedule(new SaveScheduleRequest(
                name: _scheduleName,
                cronExpression: "0 0 * * * *",
                startWorkflowRequest: new StartWorkflowRequest(name: _workflowName)
            ));

        private void Cleanup()
        {
            try { _schedulerClient.DeleteSchedule(_scheduleName); } catch { }
        }
    }
}
