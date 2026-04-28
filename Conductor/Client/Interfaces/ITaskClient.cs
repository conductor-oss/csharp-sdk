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
    public interface ITaskClient
    {
        Models.Task PollTask(string taskType, string workerId = null, string domain = null);
        ThreadTask.Task<Models.Task> PollTaskAsync(string taskType, string workerId = null, string domain = null);

        List<Models.Task> BatchPollTasks(string taskType, string workerId = null, string domain = null, int? count = null, int? timeout = null);
        ThreadTask.Task<List<Models.Task>> BatchPollTasksAsync(string taskType, string workerId = null, string domain = null, int? count = null, int? timeout = null);

        Models.Task GetTask(string taskId);
        ThreadTask.Task<Models.Task> GetTaskAsync(string taskId);

        string UpdateTask(TaskResult taskResult);
        ThreadTask.Task<string> UpdateTaskAsync(TaskResult taskResult);

        Workflow UpdateTaskSync(Dictionary<string, object> output, string workflowId, string taskRefName, TaskResult.StatusEnum status, string workerId = null);
        ThreadTask.Task<Workflow> UpdateTaskSyncAsync(Dictionary<string, object> output, string workflowId, string taskRefName, TaskResult.StatusEnum status, string workerId = null);

        Dictionary<string, int?> GetQueueSizeForTasks(List<string> taskTypes = null);
        ThreadTask.Task<Dictionary<string, int?>> GetQueueSizeForTasksAsync(List<string> taskTypes = null);

        void AddTaskLog(string taskId, string logMessage);
        ThreadTask.Task AddTaskLogAsync(string taskId, string logMessage);

        List<TaskExecLog> GetTaskLogs(string taskId);
        ThreadTask.Task<List<TaskExecLog>> GetTaskLogsAsync(string taskId);

        SearchResultTaskSummary Search(string query = null, string freeText = null, int? start = null, int? size = null, string sort = null);
        ThreadTask.Task<SearchResultTaskSummary> SearchAsync(string query = null, string freeText = null, int? start = null, int? size = null, string sort = null);
    }
}
