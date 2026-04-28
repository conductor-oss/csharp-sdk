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
using Conductor.Client;
using System;
using System.Collections.Generic;
using System.Text;
using ThreadTask = System.Threading.Tasks;

namespace conductor_csharp.Api
{
    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface ITaskResourceApi : IApiAccessor
    {
        #region Synchronous Operations
        /// <summary>
        /// Get the details about each queue
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Dictionary&lt;string, long?&gt;</returns>
        Dictionary<string, long?> All();

        /// <summary>
        /// Get the details about each queue
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Dictionary&lt;string, Dictionary&lt;string, Dictionary&lt;string, long?&gt;&gt;&gt;</returns>
        Dictionary<string, Dictionary<string, Dictionary<string, long?>>> AllVerbose();

        /// <summary>
        /// Batch poll for a task of a certain type
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="tasktype"></param>
        /// <param name="workerid"> (optional)</param>
        /// <param name="domain"> (optional)</param>
        /// <param name="count"> (optional, default to 1)</param>
        /// <param name="timeout"> (optional, default to 100)</param>
        /// <returns>List&lt;Task&gt;</returns>
        List<Task> BatchPoll(string tasktype, string workerid = null, string domain = null, int? count = null, int? timeout = null);

        /// <summary>
        /// Get the last poll data for all task types
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="workerSize"> (optional)</param>
        /// <param name="workerOpt"> (optional)</param>
        /// <param name="queueSize"> (optional)</param>
        /// <param name="queueOpt"> (optional)</param>
        /// <param name="lastPollTimeSize"> (optional)</param>
        /// <param name="lastPollTimeOpt"> (optional)</param>
        /// <returns>Dictionary&lt;string, Object&gt;</returns>
        Dictionary<string, Object> GetAllPollData(long? workerSize = null, string workerOpt = null, long? queueSize = null, string queueOpt = null, long? lastPollTimeSize = null, string lastPollTimeOpt = null);

        /// <summary>
        /// Get the external uri where the task payload is to be stored
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="path"></param>
        /// <param name="operation"></param>
        /// <param name="payloadType"></param>
        /// <returns>ExternalStorageLocation</returns>
        ExternalStorageLocation GetExternalStorageLocation(string path, string operation, string payloadType);

        /// <summary>
        /// Get the last poll data for a given task type
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="taskType"></param>
        /// <returns>List&lt;PollData&gt;</returns>
        List<PollData> GetPollData(string taskType);

        /// <summary>
        /// Get task by Id
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="taskId"></param>
        /// <returns>Task</returns>
        Task GetTask(string taskId);

        /// <summary>
        /// Get Task Execution Logs
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="taskId"></param>
        /// <returns>List&lt;TaskExecLog&gt;</returns>
        List<TaskExecLog> GetTaskLogs(string taskId);

        /// <summary>
        /// Log Task Execution Details
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        void Log(string body, string taskId);

        /// <summary>
        /// Poll for a task of a certain type
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="tasktype"></param>
        /// <param name="workerid"> (optional)</param>
        /// <param name="domain"> (optional)</param>
        /// <returns>Task</returns>
        Task Poll(string tasktype, string workerid = null, string domain = null);

        /// <summary>
        /// Requeue pending tasks
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="taskType"></param>
        /// <returns>string</returns>
        string RequeuePendingTask(string taskType);

        /// <summary>
        /// Search for tasks based in payload and other parameters
        /// </summary>
        /// <remarks>
        /// use sort options as sort&#x3D;&lt;field&gt;:ASC|DESC e.g. sort&#x3D;name&amp;sort&#x3D;workflowId:DESC. If order is not specified, defaults to ASC
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="start"> (optional, default to 0)</param>
        /// <param name="size"> (optional, default to 100)</param>
        /// <param name="sort"> (optional)</param>
        /// <param name="freeText"> (optional, default to *)</param>
        /// <param name="query"> (optional)</param>
        /// <returns>SearchResultTaskSummary</returns>
        SearchResultTaskSummary Search(int? start = null, int? size = null, string sort = null, string freeText = null, string query = null);

        /// <summary>
        /// Search for tasks based in payload and other parameters
        /// </summary>
        /// <remarks>
        /// use sort options as sort&#x3D;&lt;field&gt;:ASC|DESC e.g. sort&#x3D;name&amp;sort&#x3D;workflowId:DESC. If order is not specified, defaults to ASC
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="start"> (optional, default to 0)</param>
        /// <param name="size"> (optional, default to 100)</param>
        /// <param name="sort"> (optional)</param>
        /// <param name="freeText"> (optional, default to *)</param>
        /// <param name="query"> (optional)</param>
        /// <returns>SearchResultTask</returns>
        SearchResultTask SearchV2(int? start = null, int? size = null, string sort = null, string freeText = null, string query = null);

        /// <summary>
        /// Get Task type queue sizes
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="taskType"> (optional)</param>
        /// <returns>Dictionary&lt;string, int?&gt;</returns>
        Dictionary<string, int?> Size(List<string> taskType = null);

        /// <summary>
        /// Update a task
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <returns>string</returns>
        string UpdateTask(TaskResult body);

        /// <summary>
        /// Update a task By Ref Name
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="workflowId"></param>
        /// <param name="taskRefName"></param>
        /// <param name="status"></param>
        /// <param name="workerid"> (optional)</param>
        /// <returns>string</returns>
        string UpdateTask(Dictionary<string, Object> body, string workflowId, string taskRefName, string status, string workerid = null);

        /// <summary>
        /// Update a task By Ref Name, evaluates the workflow and returns the updated workflow
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="output"></param>
        /// <param name="workflowId"></param>
        /// <param name="taskRefName"></param>
        /// <param name="status"></param>
        /// <param name="workerid"> (optional)</param>
        /// <returns>Workflow</returns>
        Workflow UpdateTaskSync(Dictionary<string, Object> output, string workflowId, string taskRefName, TaskResult.StatusEnum status, string workerid = null);


        /// <summary>
        /// Update a task (v2) — updates the result and returns the next available task in one call.
        /// Supported on newer Conductor servers. Falls back gracefully on older servers.
        /// </summary>
        /// <param name="body">Task result to submit</param>
        /// <param name="workerid">Worker identifier (optional)</param>
        /// <returns>Next task to process, or null if none available or server does not support v2</returns>
        Task UpdateTaskV2(TaskResult body, string workerid = null);

        #endregion Synchronous Operations

        #region Asynchronous Operations
        /// <summary>
        /// Asynchronous Get the details about each queue
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Dictionary&lt;string, long?&gt;</returns>
        ThreadTask.Task<Dictionary<string, long?>> AllAsync();

        /// <summary>
        /// Asynchronous Get the details about each queue
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Dictionary&lt;string, Dictionary&lt;string, Dictionary&lt;string, long?&gt;&gt;&gt;</returns>
        ThreadTask.Task<Dictionary<string, Dictionary<string, Dictionary<string, long?>>>> AllVerboseAsync();

        /// <summary>
        /// Asynchronous Batch poll for a task of a certain type
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="tasktype"></param>
        /// <param name="workerid"> (optional)</param>
        /// <param name="domain"> (optional)</param>
        /// <param name="count"> (optional, default to 1)</param>
        /// <param name="timeout"> (optional, default to 100)</param>
        /// <returns>List&lt;Task&gt;</returns>
        ThreadTask.Task<List<Task>> BatchPollAsync(string tasktype, string workerid = null, string domain = null, int? count = null, int? timeout = null);

        /// <summary>
        /// Asynchronous Get the last poll data for all task types
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="workerSize"> (optional)</param>
        /// <param name="workerOpt"> (optional)</param>
        /// <param name="queueSize"> (optional)</param>
        /// <param name="queueOpt"> (optional)</param>
        /// <param name="lastPollTimeSize"> (optional)</param>
        /// <param name="lastPollTimeOpt"> (optional)</param>
        /// <returns>Dictionary&lt;string, Object&gt;</returns>
        ThreadTask.Task<Dictionary<string, Object>> GetAllPollDataAsync(long? workerSize = null, string workerOpt = null, long? queueSize = null, string queueOpt = null, long? lastPollTimeSize = null, string lastPollTimeOpt = null);

        /// <summary>
        /// Asynchronous Get the external uri where the task payload is to be stored
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="path"></param>
        /// <param name="operation"></param>
        /// <param name="payloadType"></param>
        /// <returns>ExternalStorageLocation</returns>
        ThreadTask.Task<ExternalStorageLocation> GetExternalStorageLocationAsync(string path, string operation, string payloadType);

        /// <summary>
        /// Asynchronous Get the last poll data for a given task type
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="taskType"></param>
        /// <returns>List&lt;PollData&gt;</returns>
        ThreadTask.Task<List<PollData>> GetPollDataAsync(string taskType);

        /// <summary>
        /// Asynchronous Get task by Id
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="taskId"></param>
        /// <returns>Task</returns>
        ThreadTask.Task<Task> GetTaskAsync(string taskId);

        /// <summary>
        /// Asynchronous Get Task Execution Logs
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="taskId"></param>
        /// <returns>List&lt;TaskExecLog&gt;</returns>
        ThreadTask.Task<List<TaskExecLog>> GetTaskLogsAsync(string taskId);

        /// <summary>
        /// Asynchronous Log Task Execution Details
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        void LogAsync(string body, string taskId);

        /// <summary>
        /// Asynchronous Poll for a task of a certain type
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="tasktype"></param>
        /// <param name="workerid"> (optional)</param>
        /// <param name="domain"> (optional)</param>
        /// <returns>Task</returns>
        ThreadTask.Task<Task> PollAsync(string tasktype, string workerid = null, string domain = null);

        /// <summary>
        /// Asynchronous Requeue pending tasks
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="taskType"></param>
        /// <returns>string</returns>
        ThreadTask.Task<string> RequeuePendingTaskAsync(string taskType);

        /// <summary>
        /// Asynchronous Search for tasks based in payload and other parameters
        /// </summary>
        /// <remarks>
        /// use sort options as sort&#x3D;&lt;field&gt;:ASC|DESC e.g. sort&#x3D;name&amp;sort&#x3D;workflowId:DESC. If order is not specified, defaults to ASC
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="start"> (optional, default to 0)</param>
        /// <param name="size"> (optional, default to 100)</param>
        /// <param name="sort"> (optional)</param>
        /// <param name="freeText"> (optional, default to *)</param>
        /// <param name="query"> (optional)</param>
        /// <returns>SearchResultTaskSummary</returns>
        ThreadTask.Task<SearchResultTaskSummary> SearchAsync(int? start = null, int? size = null, string sort = null, string freeText = null, string query = null);

        /// <summary>
        /// Asynchronous Search for tasks based in payload and other parameters
        /// </summary>
        /// <remarks>
        /// use sort options as sort&#x3D;&lt;field&gt;:ASC|DESC e.g. sort&#x3D;name&amp;sort&#x3D;workflowId:DESC. If order is not specified, defaults to ASC
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="start"> (optional, default to 0)</param>
        /// <param name="size"> (optional, default to 100)</param>
        /// <param name="sort"> (optional)</param>
        /// <param name="freeText"> (optional, default to *)</param>
        /// <param name="query"> (optional)</param>
        /// <returns>SearchResultTask</returns>
        ThreadTask.Task<SearchResultTask> SearchV2Async(int? start = null, int? size = null, string sort = null, string freeText = null, string query = null);

        /// <summary>
        /// Asynchronous Get Task type queue sizes
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="taskType"> (optional)</param>
        /// <returns>Dictionary&lt;string, int?&gt;</returns>
        ThreadTask.Task<Dictionary<string, int?>> SizeAsync(List<string> taskType = null);

        /// <summary>
        /// Asynchronous Update a task
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <returns>string</returns>
        ThreadTask.Task<string> UpdateTaskAsync(TaskResult body);

        /// <summary>
        /// Asynchronous Update a task By Ref Name
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="workflowId"></param>
        /// <param name="taskRefName"></param>
        /// <param name="status"></param>
        /// <param name="workerid"> (optional)</param>
        /// <returns>string</returns>
        ThreadTask.Task<string> UpdateTaskAsync(Dictionary<string, Object> body, string workflowId, string taskRefName, string status, string workerid = null);

        /// <summary>
        /// Update a task By Ref Name, evaluates the workflow and returns the updated workflow
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="output"></param>
        /// <param name="workflowId"></param>
        /// <param name="taskRefName"></param>
        /// <param name="status"></param>
        /// <param name="workerid"> (optional)</param>
        /// <returns>Workflow</returns>
        ThreadTask.Task<Workflow> UpdateTaskSyncAsync(Dictionary<string, Object> output, string workflowId, string taskRefName, TaskResult.StatusEnum status, string workerid = null);

        /// <summary>
        /// Asynchronous update a task (v2) — updates the result and returns the next available task.
        /// </summary>
        ThreadTask.Task<Task> UpdateTaskV2Async(TaskResult body, string workerid = null);

        #endregion Asynchronous Operations
    }
}
