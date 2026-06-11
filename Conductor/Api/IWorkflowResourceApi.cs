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
    public interface IWorkflowResourceApi : IApiAccessor
    {
        #region Synchronous Operations
        /// <summary>
        /// Starts the decision task for a workflow
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        void Decide(string workflowId);

        /// <summary>
        /// Removes the workflow from the system
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="workflowId"></param>
        /// <param name="archiveWorkflow"> (optional, default to true)</param>
        /// <returns></returns>
        void Delete(string workflowId, bool? archiveWorkflow = null);

        /// <summary>
        /// Execute a workflow synchronously
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="requestId"></param>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <param name="waitUntilTaskRef"> (optional)</param>
        /// <returns>WorkflowRun</returns>
        WorkflowRun ExecuteWorkflow(StartWorkflowRequest body, string requestId, string name, int? version, string waitUntilTaskRef = null);

        /// <summary>
        /// Update the value of the workflow variables for the given workflow id 
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="variables"></param>
        /// <returns>Workflow</returns>
        Workflow UpdateWorkflowVariables(string workflowId, Dictionary<string, Object> variables);

        /// <summary>
        /// Gets the workflow by workflow id
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="workflowId"></param>
        /// <param name="includeTasks"> (optional, default to true)</param>
        /// <param name="summarize"> (optional, default to false)</param>
        /// <returns>Workflow</returns>
        Workflow GetExecutionStatus(string workflowId, bool? includeTasks = null, bool? summarize = null);

        /// <summary>
        /// Gets the workflow by workflow id
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="workflowId"></param>
        /// <param name="includeTasks"> (optional, default to true)</param>
        /// <returns>Workflow</returns>
        Workflow GetWorkflow(string workflowId, bool? includeTasks = null);

        /// <summary>
        /// Gets the workflow tasks by workflow id
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="workflowId"></param>
        /// <param name="start"> (optional, default to 0)</param>
        /// <param name="count"> (optional, default to 15)</param>
        /// <param name="status"> (optional)</param>
        /// <returns>TaskListSearchResultSummary</returns>
        TaskListSearchResultSummary GetExecutionStatusTaskList(string workflowId, int? start = null, int? count = null, string status = null);

        /// <summary>
        /// Get the uri and path of the external storage where the workflow payload is to be stored
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
        /// Retrieve all the running workflows
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <param name="version"> (optional, default to 1)</param>
        /// <param name="startTime"> (optional)</param>
        /// <param name="endTime"> (optional)</param>
        /// <returns>List&lt;string&gt;</returns>
        List<string> GetRunningWorkflow(string name, int? version = null, long? startTime = null, long? endTime = null);

        /// <summary>
        /// Gets the workflow by workflow id
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="workflowId"></param>
        /// <param name="includeOutput"> (optional, default to false)</param>
        /// <param name="includeVariables"> (optional, default to false)</param>
        /// <returns>WorkflowStatus</returns>
        WorkflowStatus GetWorkflowStatusSummary(string workflowId, bool? includeOutput = null, bool? includeVariables = null);

        /// <summary>
        /// Lists workflows for the given correlation id list
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="name"></param>
        /// <param name="includeClosed"> (optional, default to false)</param>
        /// <param name="includeTasks"> (optional, default to false)</param>
        /// <returns>Dictionary&lt;string, List&lt;Workflow&gt;&gt;</returns>
        Dictionary<string, List<Workflow>> GetWorkflows(List<string> body, string name, bool? includeClosed = null, bool? includeTasks = null);

        /// <summary>
        /// Lists workflows for the given correlation id list and workflow name list
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="includeClosed"> (optional, default to false)</param>
        /// <param name="includeTasks"> (optional, default to false)</param>
        /// <returns>Dictionary&lt;string, List&lt;Workflow&gt;&gt;</returns>
        Dictionary<string, List<Workflow>> GetWorkflows(CorrelationIdsSearchRequest body, bool? includeClosed = null, bool? includeTasks = null);

        /// <summary>
        /// Lists workflows for the given correlation id
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <param name="correlationId"></param>
        /// <param name="includeClosed"> (optional, default to false)</param>
        /// <param name="includeTasks"> (optional, default to false)</param>
        /// <returns>List&lt;Workflow&gt;</returns>
        List<Workflow> GetWorkflows(string name, string correlationId, bool? includeClosed = null, bool? includeTasks = null);

        /// <summary>
        /// Pauses the workflow
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        void PauseWorkflow(string workflowId);

        /// <summary>
        /// Jump workflow execution to given task
        /// </summary>
        /// <remarks>
        /// Jump workflow execution to given task.
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="input"></param>
        /// <param name="workflowId"></param>
        /// <param name="taskReferenceName"> (optional)</param>
        /// <returns></returns>
        void JumpToTask(string workflowId, Dictionary<string, Object> input, string taskReferenceName = null);


        /// <summary>
        /// Reruns the workflow from a specific task
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="workflowId"></param>
        /// <returns>string</returns>
        string Rerun(RerunWorkflowRequest body, string workflowId);

        /// <summary>
        /// Resets callback times of all non-terminal SIMPLE tasks to 0
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        void ResetWorkflow(string workflowId);

        /// <summary>
        /// Restarts a completed workflow
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="workflowId"></param>
        /// <param name="useLatestDefinitions"> (optional, default to false)</param>
        /// <returns></returns>
        void Restart(string workflowId, bool? useLatestDefinitions = null);

        /// <summary>
        /// Resumes the workflow
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        void ResumeWorkflow(string workflowId);

        /// <summary>
        /// Retries the last failed task
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="workflowId"></param>
        /// <param name="resumeSubworkflowTasks"> (optional, default to false)</param>
        /// <returns></returns>
        void Retry(string workflowId, bool? resumeSubworkflowTasks = null);

        /// <summary>
        /// Search for workflows based on payload and other parameters
        /// </summary>
        /// <remarks>
        /// use sort options as sort&#x3D;&lt;field&gt;:ASC|DESC e.g. sort&#x3D;name&amp;sort&#x3D;workflowId:DESC. If order is not specified, defaults to ASC.
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="queryId"> (optional)</param>
        /// <param name="start"> (optional, default to 0)</param>
        /// <param name="size"> (optional, default to 100)</param>
        /// <param name="sort"> (optional)</param>
        /// <param name="freeText"> (optional, default to *)</param>
        /// <param name="query"> (optional)</param>
        /// <param name="skipCache"> (optional, default to false)</param>
        /// <returns>ScrollableSearchResultWorkflowSummary</returns>
        ScrollableSearchResultWorkflowSummary Search(string queryId = null, int? start = null, int? size = null, string sort = null, string freeText = null, string query = null, bool? skipCache = null);

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
        /// <returns>SearchResultWorkflow</returns>
        SearchResultWorkflow SearchV2(int? start = null, int? size = null, string sort = null, string freeText = null, string query = null);

        /// <summary>
        /// Search for workflows based on task parameters
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
        /// <returns>SearchResultWorkflowSummary</returns>
        SearchResultWorkflowSummary SearchWorkflowsByTasks(int? start = null, int? size = null, string sort = null, string freeText = null, string query = null);

        /// <summary>
        /// Search for workflows based on task parameters
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
        /// <returns>SearchResultWorkflow</returns>
        SearchResultWorkflow SearchWorkflowsByTasksV2(int? start = null, int? size = null, string sort = null, string freeText = null, string query = null);

        /// <summary>
        /// Skips a given task from a current running workflow
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="workflowId"></param>
        /// <param name="taskReferenceName"></param>
        /// <param name="skipTaskRequest"></param>
        /// <returns></returns>
        void SkipTaskFromWorkflow(string workflowId, string taskReferenceName, SkipTaskRequest skipTaskRequest);

        /// <summary>
        /// Start a new workflow with StartWorkflowRequest, which allows task to be executed in a domain
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <returns>string</returns>
        string StartWorkflow(StartWorkflowRequest body);

        /// <summary>
        /// Start a new workflow. Returns the ID of the workflow instance that can be later used for tracking
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="name"></param>
        /// <param name="version"> (optional)</param>
        /// <param name="correlationId"> (optional)</param>
        /// <param name="priority"> (optional, default to 0)</param>
        /// <returns>string</returns>
        string StartWorkflow(string name, Dictionary<string, Object> body, int? version = null, string correlationId = null, int? priority = null);

        /// <summary>
        /// Terminate workflow execution
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="workflowId"></param>
        /// <param name="reason"> (optional)</param>
        /// <param name="triggerFailureWorkflow"> (optional, default to false)</param>
        /// <returns></returns>
        void Terminate(string workflowId, string reason = null, bool? triggerFailureWorkflow = null);

        /// <summary>
        /// Test workflow execution using mock data
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <returns>Workflow</returns>
        Workflow TestWorkflow(WorkflowTestRequest body);

        /// <summary>
        /// Force upload all completed workflows to document store
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Object</returns>
        Object UploadCompletedWorkflows();

        /// <summary>
        /// Update a workflow state by updating variables or in progress task Updates the workflow variables, tasks and triggers evaluation.
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="request"></param>
        /// <param name="workflowId"></param>
        /// <param name="waitUntilTaskRefs"> (optional)</param>
        /// <param name="waitForSeconds"> (optional, default to 10)</param>
        /// <returns>WorkflowRun</returns>
        WorkflowRun UpdateWorkflow(string workflowId, WorkflowStateUpdate request,
            List<string> waitUntilTaskRefs = null, int? waitForSeconds = null);

        #endregion Synchronous Operations

        #region Asynchronous Operations
        /// <summary>
        /// Asynchronous Starts the decision task for a workflow
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        ThreadTask.Task DecideAsync(string workflowId);

        /// <summary>
        /// Asynchronous Removes the workflow from the system
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="workflowId"></param>
        /// <param name="archiveWorkflow"> (optional, default to true)</param>
        /// <returns></returns>
        ThreadTask.Task DeleteAsync(string workflowId, bool? archiveWorkflow = null);

        /// <summary>
        /// Asynchronous Execute a workflow 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="requestId"></param>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <param name="waitUntilTaskRef"> (optional)</param>
        /// <returns>WorkflowRun</returns>
        ThreadTask.Task<WorkflowRun> ExecuteWorkflowAsync(StartWorkflowRequest body, string requestId, string name, int? version, string waitUntilTaskRef = null);

        /// <summary>
        /// Asynchronous Update the value of the workflow variables for the given workflow id 
        /// </summary>
        /// <param name="workflowId"></param>
        /// /// <param name="variables"></param>
        /// <returns>Workflow</returns>
        ThreadTask.Task<Workflow> UpdateWorkflowVariablesAsync(string workflowId, Dictionary<string, Object> variables);


        /// <summary>
        /// Asynchronous Gets the workflow by workflow id
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="workflowId"></param>
        /// <param name="includeTasks"> (optional, default to true)</param>
        /// <param name="summarize"> (optional, default to false)</param>
        /// <returns>Workflow</returns>
        ThreadTask.Task<Workflow> GetExecutionStatusAsync(string workflowId, bool? includeTasks = null, bool? summarize = null);

        /// <summary>
        /// Asynchronous Gets the workflow by workflow id
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="workflowId"></param>
        /// <param name="includeTasks"> (optional, default to true)</param>
        /// <returns>Workflow</returns>
        ThreadTask.Task<Workflow> GetWorkflowAsync(string workflowId, bool? includeTasks = null);

        /// <summary>
        /// Asynchronous Gets the workflow tasks by workflow id
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="workflowId"></param>
        /// <param name="start"> (optional, default to 0)</param>
        /// <param name="count"> (optional, default to 15)</param>
        /// <param name="status"> (optional)</param>
        /// <returns>TaskListSearchResultSummary</returns>
        ThreadTask.Task<TaskListSearchResultSummary> GetExecutionStatusTaskListAsync(string workflowId, int? start = null, int? count = null, string status = null);

        /// <summary>
        /// Asynchronous Get the uri and path of the external storage where the workflow payload is to be stored
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
        /// Asynchronous Retrieve all the running workflows
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <param name="version"> (optional, default to 1)</param>
        /// <param name="startTime"> (optional)</param>
        /// <param name="endTime"> (optional)</param>
        /// <returns>List&lt;string&gt;</returns>
        ThreadTask.Task<List<string>> GetRunningWorkflowAsync(string name, int? version = null, long? startTime = null, long? endTime = null);

        /// <summary>
        /// Asynchronous Gets the workflow by workflow id
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="workflowId"></param>
        /// <param name="includeOutput"> (optional, default to false)</param>
        /// <param name="includeVariables"> (optional, default to false)</param>
        /// <returns>WorkflowStatus</returns>
        ThreadTask.Task<WorkflowStatus> GetWorkflowStatusSummaryAsync(string workflowId, bool? includeOutput = null, bool? includeVariables = null);

        /// <summary>
        /// Asynchronous Lists workflows for the given correlation id list
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="name"></param>
        /// <param name="includeClosed"> (optional, default to false)</param>
        /// <param name="includeTasks"> (optional, default to false)</param>
        /// <returns>Dictionary&lt;string, List&lt;Workflow&gt;&gt;</returns>
        ThreadTask.Task<Dictionary<string, List<Workflow>>> GetWorkflowsAsync(List<string> body, string name, bool? includeClosed = null, bool? includeTasks = null);

        /// <summary>
        /// Asynchronous Lists workflows for the given correlation id list and workflow name list
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="includeClosed"> (optional, default to false)</param>
        /// <param name="includeTasks"> (optional, default to false)</param>
        /// <returns>Dictionary&lt;string, List&lt;Workflow&gt;&gt;</returns>
        ThreadTask.Task<Dictionary<string, List<Workflow>>> GetWorkflowsAsync(CorrelationIdsSearchRequest body, bool? includeClosed = null, bool? includeTasks = null);

        /// <summary>
        /// Asynchronous Lists workflows for the given correlation id
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <param name="correlationId"></param>
        /// <param name="includeClosed"> (optional, default to false)</param>
        /// <param name="includeTasks"> (optional, default to false)</param>
        /// <returns>List&lt;Workflow&gt;</returns>
        ThreadTask.Task<List<Workflow>> GetWorkflowsAsync(string name, string correlationId, bool? includeClosed = null, bool? includeTasks = null);

        /// <summary>
        /// Asynchronous Pauses the workflow
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        ThreadTask.Task PauseWorkflowAsync(string workflowId);

        /// <summary>
        /// Asynchronous Jump workflow execution to given task
        /// </summary>
        /// <remarks>
        /// Jump workflow execution to given task.
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="input"></param>
        /// <param name="workflowId"></param>
        /// <param name="taskReferenceName"> (optional)</param>
        /// <returns></returns>
        ThreadTask.Task JumpToTaskAsync(string workflowId, Dictionary<string, Object> input, string taskReferenceName = null);


        /// <summary>
        /// Asynchronous Reruns the workflow from a specific task
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="workflowId"></param>
        /// <returns>string</returns>
        ThreadTask.Task<string> RerunAsync(RerunWorkflowRequest body, string workflowId);

        /// <summary>
        /// Asynchronous Resets callback times of all non-terminal SIMPLE tasks to 0
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        ThreadTask.Task ResetWorkflowAsync(string workflowId);

        /// <summary>
        /// Asynchronous Restarts a completed workflow
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="workflowId"></param>
        /// <param name="useLatestDefinitions"> (optional, default to false)</param>
        /// <returns></returns>
        ThreadTask.Task RestartAsync(string workflowId, bool? useLatestDefinitions = null);

        /// <summary>
        /// Asynchronous Resumes the workflow
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        ThreadTask.Task ResumeWorkflowAsync(string workflowId);

        /// <summary>
        /// Asynchronous Retries the last failed task
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="workflowId"></param>
        /// <param name="resumeSubworkflowTasks"> (optional, default to false)</param>
        /// <returns></returns>
        ThreadTask.Task RetryAsync(string workflowId, bool? resumeSubworkflowTasks = null);

        /// <summary>
        /// Asynchronous Search for workflows based on payload and other parameters
        /// </summary>
        /// <remarks>
        /// use sort options as sort&#x3D;&lt;field&gt;:ASC|DESC e.g. sort&#x3D;name&amp;sort&#x3D;workflowId:DESC. If order is not specified, defaults to ASC.
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="queryId"> (optional)</param>
        /// <param name="start"> (optional, default to 0)</param>
        /// <param name="size"> (optional, default to 100)</param>
        /// <param name="sort"> (optional)</param>
        /// <param name="freeText"> (optional, default to *)</param>
        /// <param name="query"> (optional)</param>
        /// <param name="skipCache"> (optional, default to false)</param>
        /// <returns>ScrollableSearchResultWorkflowSummary</returns>
        ThreadTask.Task<ScrollableSearchResultWorkflowSummary> SearchAsync(string queryId = null, int? start = null, int? size = null, string sort = null, string freeText = null, string query = null, bool? skipCache = null);

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
        /// <returns>SearchResultWorkflow</returns>
        ThreadTask.Task<SearchResultWorkflow> SearchV2Async(int? start = null, int? size = null, string sort = null, string freeText = null, string query = null);

        /// <summary>
        /// Asynchronous Search for workflows based on task parameters
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
        /// <returns>SearchResultWorkflowSummary</returns>
        ThreadTask.Task<SearchResultWorkflowSummary> SearchWorkflowsByTasksAsync(int? start = null, int? size = null, string sort = null, string freeText = null, string query = null);

        /// <summary>
        /// Asynchronous Search for workflows based on task parameters
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
        /// <returns>SearchResultWorkflow</returns>
        ThreadTask.Task<SearchResultWorkflow> SearchWorkflowsByTasksV2Async(int? start = null, int? size = null, string sort = null, string freeText = null, string query = null);

        /// <summary>
        /// Asynchronous Skips a given task from a current running workflow
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="workflowId"></param>
        /// <param name="taskReferenceName"></param>
        /// <param name="skipTaskRequest"></param>
        /// <returns></returns>
        ThreadTask.Task SkipTaskFromWorkflowAsync(string workflowId, string taskReferenceName, SkipTaskRequest skipTaskRequest);

        /// <summary>
        /// Asynchronous Start a new workflow with StartWorkflowRequest, which allows task to be executed in a domain
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <returns>string</returns>
        ThreadTask.Task<string> StartWorkflowAsync(StartWorkflowRequest body);

        /// <summary>
        /// Asynchronous Start a new workflow. Returns the ID of the workflow instance that can be later used for tracking
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="name"></param>
        /// <param name="version"> (optional)</param>
        /// <param name="correlationId"> (optional)</param>
        /// <param name="priority"> (optional, default to 0)</param>
        /// <returns>string</returns>
        ThreadTask.Task<string> StartWorkflowAsync(string name, Dictionary<string, Object> body, int? version = null, string correlationId = null, int? priority = null);

        /// <summary>
        /// Asynchronous Terminate workflow execution
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="workflowId"></param>
        /// <param name="reason"> (optional)</param>
        /// <param name="triggerFailureWorkflow"> (optional, default to false)</param>
        /// <returns></returns>
        ThreadTask.Task TerminateAsync(string workflowId, string reason = null, bool? triggerFailureWorkflow = null);

        /// <summary>
        /// Asynchronous Test workflow execution using mock data
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <returns>Workflow</returns>
        ThreadTask.Task<Workflow> TestWorkflowAsync(WorkflowTestRequest body);

        /// <summary>
        /// Asynchronous Force upload all completed workflows to document store
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Object</returns>
        ThreadTask.Task<Object> UploadCompletedWorkflowsAsync();

        /// <summary>
        /// Update a workflow state by updating variables or in progress task Updates the workflow variables, tasks and triggers evaluation.
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="request"></param>
        /// <param name="workflowId"></param>
        /// <param name="waitUntilTaskRefs"> (optional)</param>
        /// <param name="waitForSeconds"> (optional, default to 10)</param>
        /// <returns>WorkflowRun</returns>
        ThreadTask.Task<WorkflowRun> UpdateWorkflowAsync(string workflowId, WorkflowStateUpdate request,
            List<string> waitUntilTaskRefs = null, int? waitForSeconds = null);

        #endregion Asynchronous Operations
    }
}
