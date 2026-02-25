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
    public class OrkesTaskClient : ITaskClient
    {
        private readonly ITaskResourceApi _taskApi;

        public OrkesTaskClient(Configuration configuration)
        {
            _taskApi = configuration.GetClient<TaskResourceApi>();
        }

        public OrkesTaskClient(ITaskResourceApi taskApi)
        {
            _taskApi = taskApi;
        }

        public Models.Task PollTask(string taskType, string workerId = null, string domain = null)
            => _taskApi.Poll(taskType, workerId, domain);
        public ThreadTask.Task<Models.Task> PollTaskAsync(string taskType, string workerId = null, string domain = null)
            => _taskApi.PollAsync(taskType, workerId, domain);

        public List<Models.Task> BatchPollTasks(string taskType, string workerId = null, string domain = null, int? count = null, int? timeout = null)
            => _taskApi.BatchPoll(taskType, workerId, domain, count, timeout);
        public ThreadTask.Task<List<Models.Task>> BatchPollTasksAsync(string taskType, string workerId = null, string domain = null, int? count = null, int? timeout = null)
            => _taskApi.BatchPollAsync(taskType, workerId, domain, count, timeout);

        public Models.Task GetTask(string taskId) => _taskApi.GetTask(taskId);
        public ThreadTask.Task<Models.Task> GetTaskAsync(string taskId) => _taskApi.GetTaskAsync(taskId);

        public string UpdateTask(TaskResult taskResult) => _taskApi.UpdateTask(taskResult);
        public ThreadTask.Task<string> UpdateTaskAsync(TaskResult taskResult) => _taskApi.UpdateTaskAsync(taskResult);

        public Workflow UpdateTaskSync(Dictionary<string, object> output, string workflowId, string taskRefName, TaskResult.StatusEnum status, string workerId = null)
            => _taskApi.UpdateTaskSync(output, workflowId, taskRefName, status, workerId);
        public ThreadTask.Task<Workflow> UpdateTaskSyncAsync(Dictionary<string, object> output, string workflowId, string taskRefName, TaskResult.StatusEnum status, string workerId = null)
            => _taskApi.UpdateTaskSyncAsync(output, workflowId, taskRefName, status, workerId);

        public Dictionary<string, int?> GetQueueSizeForTasks(List<string> taskTypes = null) => _taskApi.Size(taskTypes);
        public ThreadTask.Task<Dictionary<string, int?>> GetQueueSizeForTasksAsync(List<string> taskTypes = null) => _taskApi.SizeAsync(taskTypes);

        public void AddTaskLog(string taskId, string logMessage) => _taskApi.Log(logMessage, taskId);
        public ThreadTask.Task AddTaskLogAsync(string taskId, string logMessage) => ThreadTask.Task.Run(() => _taskApi.Log(logMessage, taskId));

        public List<TaskExecLog> GetTaskLogs(string taskId) => _taskApi.GetTaskLogs(taskId);
        public ThreadTask.Task<List<TaskExecLog>> GetTaskLogsAsync(string taskId) => _taskApi.GetTaskLogsAsync(taskId);

        public SearchResultTaskSummary Search(string query = null, string freeText = null, int? start = null, int? size = null, string sort = null)
            => _taskApi.Search(start, size, sort, freeText ?? "*", query);
        public ThreadTask.Task<SearchResultTaskSummary> SearchAsync(string query = null, string freeText = null, int? start = null, int? size = null, string sort = null)
            => _taskApi.SearchAsync(start, size, sort, freeText ?? "*", query);
    }
}
