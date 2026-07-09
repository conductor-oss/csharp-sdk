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
    public interface ISchedulerResourceApi : IApiAccessor
    {
        #region Synchronous Operations
        /// <summary>
        /// Deletes an existing workflow schedule by name
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <returns>Object</returns>
        Object DeleteSchedule(string name);

        /// <summary>
        /// Delete a tag for schedule
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        void DeleteTagForSchedule(List<TagObject> body, string name);

        /// <summary>
        /// Get all existing workflow schedules and optionally filter by workflow name
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="workflowName"> (optional)</param>
        /// <returns>List&lt;WorkflowSchedule&gt;</returns>
        List<WorkflowSchedule> GetAllSchedules(string workflowName = null);

        /// <summary>
        /// Get list of the next x (default 3, max 5) execution times for a scheduler
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="cronExpression"></param>
        /// <param name="scheduleStartTime"> (optional)</param>
        /// <param name="scheduleEndTime"> (optional)</param>
        /// <param name="limit"> (optional, default to 3)</param>
        /// <returns>List&lt;long?&gt;</returns>
        List<long?> GetNextFewSchedules(string cronExpression, long? scheduleStartTime = null, long? scheduleEndTime = null, int? limit = null);

        /// <summary>
        /// Get an existing workflow schedule by name
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <returns>WorkflowSchedule</returns>
        WorkflowSchedule GetSchedule(string name);

        /// <summary>
        /// Get tags by schedule
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <returns>List&lt;TagObject&gt;</returns>
        List<TagObject> GetTagsForSchedule(string name);

        /// <summary>
        /// Pause all scheduling in a single conductor server instance (for debugging only)
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Dictionary&lt;string, Object&gt;</returns>
        Dictionary<string, Object> PauseAllSchedules();

        /// <summary>
        /// Pauses an existing schedule by name
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <returns>Object</returns>
        Object PauseSchedule(string name);

        /// <summary>
        /// Put a tag to schedule
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        void PutTagForSchedule(List<TagObject> body, string name);

        /// <summary>
        /// Requeue all execution records
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Dictionary&lt;string, Object&gt;</returns>
        Dictionary<string, Object> RequeueAllExecutionRecords();

        /// <summary>
        /// Resume all scheduling
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Dictionary&lt;string, Object&gt;</returns>
        Dictionary<string, Object> ResumeAllSchedules();

        /// <summary>
        /// Resume a paused schedule by name
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <returns>Object</returns>
        Object ResumeSchedule(string name);

        /// <summary>
        /// Create or update a schedule for a specified workflow with a corresponding start workflow request
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <returns>Object</returns>
        Object SaveSchedule(SaveScheduleRequest body);

        /// <summary>
        /// Search for workflows based on payload and other parameters
        /// </summary>
        /// <remarks>
        /// use sort options as sort&#x3D;&lt;field&gt;:ASC|DESC e.g. sort&#x3D;name&amp;sort&#x3D;workflowId:DESC. If order is not specified, defaults to ASC.
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="start"> (optional, default to 0)</param>
        /// <param name="size"> (optional, default to 100)</param>
        /// <param name="sort"> (optional)</param>
        /// <param name="freeText"> (optional, default to *)</param>
        /// <param name="query"> (optional)</param>
        /// <returns>SearchResultWorkflowScheduleExecutionModel</returns>
        SearchResultWorkflowScheduleExecutionModel SearchV22(int? start = null, int? size = null, string sort = null, string freeText = null, string query = null);

        /// <summary>
        /// Test timeout - do not use in production
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns></returns>
        void TestTimeout();

        #endregion Synchronous Operations

        #region Asynchronous Operations
        /// <summary>
        /// Deletes an existing workflow schedule by name
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <returns>Object</returns>
        ThreadTask.Task<Object> DeleteScheduleAsync(string name);

        /// <summary>
        /// Asynchronous Delete a tag for schedule
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        ThreadTask.Task DeleteTagForScheduleAsync(List<TagObject> body, string name);

        /// <summary>
        /// Asynchronous Get all existing workflow schedules and optionally filter by workflow name
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="workflowName"> (optional)</param>
        /// <returns>List&lt;WorkflowSchedule&gt;</returns>
        ThreadTask.Task<List<WorkflowSchedule>> GetAllSchedulesAsync(string workflowName = null);

        /// <summary>
        /// Asynchronous Get list of the next x (default 3, max 5) execution times for a scheduler
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="cronExpression"></param>
        /// <param name="scheduleStartTime"> (optional)</param>
        /// <param name="scheduleEndTime"> (optional)</param>
        /// <param name="limit"> (optional, default to 3)</param>
        /// <returns>List&lt;long?&gt;</returns>
        ThreadTask.Task<List<long?>> GetNextFewSchedulesAsync(string cronExpression, long? scheduleStartTime = null, long? scheduleEndTime = null, int? limit = null);

        /// <summary>
        /// Asynchronous Get an existing workflow schedule by name
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <returns>WorkflowSchedule</returns>
        ThreadTask.Task<WorkflowSchedule> GetScheduleAsync(string name);

        /// <summary>
        /// Asynchronous Get tags by schedule
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <returns>List&lt;TagObject&gt;</returns>
        ThreadTask.Task<List<TagObject>> GetTagsForScheduleAsync(string name);

        /// <summary>
        /// Asynchronous Pause all scheduling in a single conductor server instance (for debugging only)
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Dictionary&lt;string, Object&gt;</returns>
        ThreadTask.Task<Dictionary<string, Object>> PauseAllSchedulesAsync();

        /// <summary>
        /// Asynchronous Pauses an existing schedule by name
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <returns>Object</returns>
        ThreadTask.Task<Object> PauseScheduleAsync(string name);

        /// <summary>
        /// Asynchronous Put a tag to schedule
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        ThreadTask.Task PutTagForScheduleAsync(List<TagObject> body, string name);

        /// <summary>
        /// Asynchronous Requeue all execution records
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Dictionary&lt;string, Object&gt;</returns>
        ThreadTask.Task<Dictionary<string, Object>> RequeueAllExecutionRecordsAsync();

        /// <summary>
        /// Asynchronous Resume all scheduling
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Dictionary&lt;string, Object&gt;</returns>
        ThreadTask.Task<Dictionary<string, Object>> ResumeAllSchedulesAsync();

        /// <summary>
        /// Asynchronous Resume a paused schedule by name
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <returns>Object</returns>
        ThreadTask.Task<Object> ResumeScheduleAsync(string name);

        /// <summary>
        /// Asynchronous Create or update a schedule for a specified workflow with a corresponding start workflow request
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <returns>Object</returns>
        ThreadTask.Task<Object> SaveScheduleAsync(SaveScheduleRequest body);

        /// <summary>
        /// Asynchronous Search for workflows based on payload and other parameters
        /// </summary>
        /// <remarks>
        /// use sort options as sort&#x3D;&lt;field&gt;:ASC|DESC e.g. sort&#x3D;name&amp;sort&#x3D;workflowId:DESC. If order is not specified, defaults to ASC.
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="start"> (optional, default to 0)</param>
        /// <param name="size"> (optional, default to 100)</param>
        /// <param name="sort"> (optional)</param>
        /// <param name="freeText"> (optional, default to *)</param>
        /// <param name="query"> (optional)</param>
        /// <returns>SearchResultWorkflowScheduleExecutionModel</returns>
        ThreadTask.Task<SearchResultWorkflowScheduleExecutionModel> SearchV22Async(int? start = null, int? size = null, string sort = null, string freeText = null, string query = null);

        /// <summary>
        /// Asynchronous Test timeout - do not use in production
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns></returns>
        ThreadTask.Task TestTimeoutAsync();
        #endregion Asynchronous Operations
    }
}
