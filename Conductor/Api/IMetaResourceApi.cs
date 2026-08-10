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
    public interface IMetadataResourceApi : IApiAccessor
    {
        #region Synchronous Operations
        /// <summary>
        /// Create a new workflow definition
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="overwrite"> (optional, default to false)</param>
        /// <returns>Object</returns>
        Object Create(WorkflowDef body, bool? overwrite = null);

        /// <summary>
        /// Retrieves workflow definition along with blueprint
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <param name="version"> (optional)</param>
        /// <param name="metadata"> (optional, default to false)</param>
        /// <returns>WorkflowDef</returns>
        WorkflowDef Get(string name, int? version = null, bool? metadata = null);

        /// <summary>
        /// Retrieves all workflow definition along with blueprint
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="access"> (optional, default to READ)</param>
        /// <param name="metadata"> (optional, default to false)</param>
        /// <param name="tagKey"> (optional)</param>
        /// <param name="tagValue"> (optional)</param>
        /// <param name="_short"> (optional, default to false)</param>
        /// <returns>List&lt;WorkflowDef&gt;</returns>
        List<WorkflowDef> GetAllWorkflows(string access = null, bool? metadata = null, string tagKey = null, string tagValue = null, bool? _short = null);

        /// <summary>
        /// Gets the task definition
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="tasktype"></param>
        /// <param name="metadata"> (optional, default to false)</param>
        /// <returns>Object</returns>
        TaskDef GetTaskDef(string tasktype, bool? metadata = null);

        /// <summary>
        /// Gets all task definition
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="access"> (optional, default to READ)</param>
        /// <param name="metadata"> (optional, default to false)</param>
        /// <param name="tagKey"> (optional)</param>
        /// <param name="tagValue"> (optional)</param>
        /// <returns>List&lt;TaskDef&gt;</returns>
        List<TaskDef> GetTaskDefs(string access = null, bool? metadata = null, string tagKey = null, string tagValue = null);

        /// <summary>
        /// Create or update task definition(s)
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <returns>Object</returns>
        Object RegisterTaskDef(List<TaskDef> body);

        /// <summary>
        /// Remove a task definition
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="tasktype"></param>
        /// <returns></returns>
        void UnregisterTaskDef(string tasktype);

        /// <summary>
        /// Removes workflow definition. It does not remove workflows associated with the definition.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        void UnregisterWorkflowDef(string name, int? version);

        /// <summary>
        /// Create or update workflow definition(s)
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="overwrite"> (optional, default to true)</param>
        /// <returns>Object</returns>
        Object UpdateWorkflowDefinitions(List<WorkflowDef> body, bool? overwrite = null);

        /// <summary>
        /// Update an existing task
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <returns>Object</returns>
        Object UpdateTaskDef(TaskDef body);

        /// <summary>
        /// Upload all workflows and tasks definitions to S3
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Object</returns>
        Object UploadWorkflowsAndTasksDefinitionsToS3();

        #endregion Synchronous Operations

        #region Asynchronous Operations
        /// <summary>
        /// Asynchronous Create a new workflow definition
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="overwrite"> (optional, default to false)</param>
        /// <returns>Object</returns>
        ThreadTask.Task<Object> CreateAsync(WorkflowDef body, bool? overwrite = null);

        /// <summary>
        /// Asynchronous Retrieves workflow definition along with blueprint
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <param name="version"> (optional)</param>
        /// <param name="metadata"> (optional, default to false)</param>
        /// <returns>WorkflowDef</returns>
        ThreadTask.Task<WorkflowDef> GetAsync(string name, int? version = null, bool? metadata = null);

        /// <summary>
        /// Asynchronous Retrieves all workflow definition along with blueprint
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="access"> (optional, default to READ)</param>
        /// <param name="metadata"> (optional, default to false)</param>
        /// <param name="tagKey"> (optional)</param>
        /// <param name="tagValue"> (optional)</param>
        /// <param name="_short"> (optional, default to false)</param>
        /// <returns>List&lt;WorkflowDef&gt;</returns>
        ThreadTask.Task<List<WorkflowDef>> GetAllWorkflowsAsync(string access = null, bool? metadata = null, string tagKey = null, string tagValue = null, bool? _short = null);

        /// <summary>
        /// Asynchronous Gets the task definition
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="tasktype"></param>
        /// <param name="metadata"> (optional, default to false)</param>
        /// <returns>Object</returns>
        ThreadTask.Task<TaskDef> GetTaskDefAsync(string tasktype, bool? metadata = null);

        /// <summary>
        /// Asynchronous Gets all task definition
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="access"> (optional, default to READ)</param>
        /// <param name="metadata"> (optional, default to false)</param>
        /// <param name="tagKey"> (optional)</param>
        /// <param name="tagValue"> (optional)</param>
        /// <returns>List&lt;TaskDef&gt;</returns>
        ThreadTask.Task<List<TaskDef>> GetTaskDefsAsync(string access = null, bool? metadata = null, string tagKey = null, string tagValue = null);

        /// <summary>
        /// Asynchronous Create or update task definition(s)
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <returns>Object</returns>
        ThreadTask.Task<Object> RegisterTaskDefAsync(List<TaskDef> body);

        /// <summary>
        /// Asynchronous Remove a task definition
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="tasktype"></param>
        /// <returns></returns>
        ThreadTask.Task UnregisterTaskDefAsync(string tasktype);

        /// <summary>
        /// Asynchronous Removes workflow definition. It does not remove workflows associated with the definition.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        ThreadTask.Task UnregisterWorkflowDefAsync(string name, int? version);

        /// <summary>
        /// Asynchronous Create or update workflow definition(s)
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="overwrite"> (optional, default to true)</param>
        /// <returns>Object</returns>
        ThreadTask.Task<Object> UpdateWorkflowDefinitionsAsync(List<WorkflowDef> body, bool? overwrite = null);

        /// <summary>
        /// Asynchronous Update an existing task
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <returns>Object</returns>
        ThreadTask.Task<Object> UpdateTaskDefAsync(TaskDef body);

        /// <summary>
        /// Asynchronous Upload all workflows and tasks definitions to S3
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Object</returns>
        ThreadTask.Task<Object> UploadWorkflowsAndTasksDefinitionsToS3Async();
        #endregion Asynchronous Operations
    }
}
