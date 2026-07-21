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

        // Restricted to Orkes-only until the server HTTP verb is corrected upstream.
        // The Orkes Conductor server currently exposes pause/resume of a schedule by name as GET
        // (/scheduler/schedules/{name}/pause and .../resume), which is wrong for a state-mutating
        // operation and forced every SDK (including this one) to call it with GET. OSS Conductor
        // already models these correctly as PUT, so running these tests against OSS fails with
        // "Request method 'GET' is not supported". The enterprise server is in the process of being
        // updated to PUT. Once the deployed SDK-test enterprise server gets that update, these tests
        // will start failing there (GET no longer accepted) — that is the signal to switch all SDKs
        // to PUT for PauseSchedule/ResumeSchedule and remove this Orkes-only restriction so they run
        // against OSS again (which is already on PUT).
        [Fact]
        [Trait("ServerType", "Orkes")]
        public void PauseSchedule_ScheduleIsPaused()
        {
            Save();
            _schedulerClient.PauseSchedule(_scheduleName);
            var schedule = _schedulerClient.GetSchedule(_scheduleName);
            Assert.True(schedule.Paused);
            Cleanup();
        }

        // Restricted to Orkes-only for the same reason as PauseSchedule_ScheduleIsPaused above:
        // ResumeSchedule (and the PauseSchedule it depends on) is called as GET to match the current
        // Orkes server, but OSS correctly requires PUT. Re-enable against OSS (drop this trait) when
        // the SDKs are switched to PUT after the enterprise server is corrected.
        [Fact]
        [Trait("ServerType", "Orkes")]
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

        [Fact]
        public void GetNextFewSchedules_ReturnsTimestamps()
        {
            var timestamps = _schedulerClient.GetNextFewSchedules("0 0 * * * *", limit: 3);
            Assert.NotNull(timestamps);
            Assert.Equal(3, timestamps.Count);
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
