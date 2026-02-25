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

using Conductor.Client.Models;
using System.Collections.Generic;
using ThreadTask = System.Threading.Tasks;

namespace Conductor.Client.Interfaces
{
    public interface ISchedulerClient
    {
        void SaveSchedule(SaveScheduleRequest saveScheduleRequest);
        ThreadTask.Task SaveScheduleAsync(SaveScheduleRequest saveScheduleRequest);

        WorkflowSchedule GetSchedule(string name);
        ThreadTask.Task<WorkflowSchedule> GetScheduleAsync(string name);

        List<WorkflowSchedule> GetAllSchedules(string workflowName = null);
        ThreadTask.Task<List<WorkflowSchedule>> GetAllSchedulesAsync(string workflowName = null);

        List<long?> GetNextFewScheduleExecutionTimes(string cronExpression, long? scheduleStartTime = null, long? scheduleEndTime = null, int? limit = null);
        ThreadTask.Task<List<long?>> GetNextFewScheduleExecutionTimesAsync(string cronExpression, long? scheduleStartTime = null, long? scheduleEndTime = null, int? limit = null);

        void DeleteSchedule(string name);
        ThreadTask.Task DeleteScheduleAsync(string name);

        void PauseSchedule(string name);
        ThreadTask.Task PauseScheduleAsync(string name);

        void PauseAllSchedules();
        ThreadTask.Task PauseAllSchedulesAsync();

        void ResumeSchedule(string name);
        ThreadTask.Task ResumeScheduleAsync(string name);

        void ResumeAllSchedules();
        ThreadTask.Task ResumeAllSchedulesAsync();

        SearchResultWorkflowScheduleExecutionModel SearchScheduleExecutions(int? start = null, int? size = null, string sort = null, string freeText = null, string query = null);
        ThreadTask.Task<SearchResultWorkflowScheduleExecutionModel> SearchScheduleExecutionsAsync(int? start = null, int? size = null, string sort = null, string freeText = null, string query = null);

        List<TagObject> GetTagsForSchedule(string name);
        void PutTagForSchedule(List<TagObject> tags, string name);
        void DeleteTagForSchedule(List<TagObject> tags, string name);
    }
}
