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
using conductor_csharp.Api;
using Conductor.Client.Interfaces;
using Conductor.Client.Models;
using System.Collections.Generic;
using ThreadTask = System.Threading.Tasks;

namespace Conductor.Client.Orkes
{
    public class OrkesSchedulerClient : ISchedulerClient
    {
        private readonly ISchedulerResourceApi _schedulerApi;

        public OrkesSchedulerClient(Configuration configuration)
        {
            _schedulerApi = configuration.GetClient<SchedulerResourceApi>();
        }

        public OrkesSchedulerClient(ISchedulerResourceApi schedulerApi)
        {
            _schedulerApi = schedulerApi;
        }

        public void SaveSchedule(SaveScheduleRequest saveScheduleRequest) => _schedulerApi.SaveSchedule(saveScheduleRequest);
        public ThreadTask.Task SaveScheduleAsync(SaveScheduleRequest saveScheduleRequest) => _schedulerApi.SaveScheduleAsync(saveScheduleRequest);

        public WorkflowSchedule GetSchedule(string name) => _schedulerApi.GetSchedule(name);
        public ThreadTask.Task<WorkflowSchedule> GetScheduleAsync(string name) => _schedulerApi.GetScheduleAsync(name);

        public List<WorkflowSchedule> GetAllSchedules(string workflowName = null) => _schedulerApi.GetAllSchedules(workflowName);
        public ThreadTask.Task<List<WorkflowSchedule>> GetAllSchedulesAsync(string workflowName = null) => _schedulerApi.GetAllSchedulesAsync(workflowName);

        public List<long?> GetNextFewScheduleExecutionTimes(string cronExpression, long? scheduleStartTime = null, long? scheduleEndTime = null, int? limit = null)
            => _schedulerApi.GetNextFewSchedules(cronExpression, scheduleStartTime, scheduleEndTime, limit);
        public ThreadTask.Task<List<long?>> GetNextFewScheduleExecutionTimesAsync(string cronExpression, long? scheduleStartTime = null, long? scheduleEndTime = null, int? limit = null)
            => _schedulerApi.GetNextFewSchedulesAsync(cronExpression, scheduleStartTime, scheduleEndTime, limit);

        public void DeleteSchedule(string name) => _schedulerApi.DeleteSchedule(name);
        public ThreadTask.Task DeleteScheduleAsync(string name) => _schedulerApi.DeleteScheduleAsync(name);

        public void PauseSchedule(string name) => _schedulerApi.PauseSchedule(name);
        public ThreadTask.Task PauseScheduleAsync(string name) => _schedulerApi.PauseScheduleAsync(name);

        public void PauseAllSchedules() => _schedulerApi.PauseAllSchedules();
        public ThreadTask.Task PauseAllSchedulesAsync() => _schedulerApi.PauseAllSchedulesAsync();

        public void ResumeSchedule(string name) => _schedulerApi.ResumeSchedule(name);
        public ThreadTask.Task ResumeScheduleAsync(string name) => _schedulerApi.ResumeScheduleAsync(name);

        public void ResumeAllSchedules() => _schedulerApi.ResumeAllSchedules();
        public ThreadTask.Task ResumeAllSchedulesAsync() => _schedulerApi.ResumeAllSchedulesAsync();

        public SearchResultWorkflowScheduleExecutionModel SearchScheduleExecutions(int? start = null, int? size = null, string sort = null, string freeText = null, string query = null)
            => _schedulerApi.SearchV22(start, size, sort, freeText, query);
        public ThreadTask.Task<SearchResultWorkflowScheduleExecutionModel> SearchScheduleExecutionsAsync(int? start = null, int? size = null, string sort = null, string freeText = null, string query = null)
            => _schedulerApi.SearchV22Async(start, size, sort, freeText, query);

        public List<TagObject> GetTagsForSchedule(string name) => _schedulerApi.GetTagsForSchedule(name);
        public void PutTagForSchedule(List<TagObject> tags, string name) => _schedulerApi.PutTagForSchedule(tags, name);
        public void DeleteTagForSchedule(List<TagObject> tags, string name) => _schedulerApi.DeleteTagForSchedule(tags, name);
    }
}
