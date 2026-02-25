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
    public class OrkesWorkflowClient : IWorkflowClient
    {
        private readonly IWorkflowResourceApi _workflowApi;
        private readonly IWorkflowBulkResourceApi _bulkApi;

        public OrkesWorkflowClient(Configuration configuration)
        {
            _workflowApi = configuration.GetClient<WorkflowResourceApi>();
            _bulkApi = configuration.GetClient<WorkflowBulkResourceApi>();
        }

        public OrkesWorkflowClient(IWorkflowResourceApi workflowApi, IWorkflowBulkResourceApi bulkApi)
        {
            _workflowApi = workflowApi;
            _bulkApi = bulkApi;
        }

        public string StartWorkflow(StartWorkflowRequest startWorkflowRequest) => _workflowApi.StartWorkflow(startWorkflowRequest);
        public ThreadTask.Task<string> StartWorkflowAsync(StartWorkflowRequest startWorkflowRequest) => _workflowApi.StartWorkflowAsync(startWorkflowRequest);

        public WorkflowRun ExecuteWorkflow(StartWorkflowRequest startWorkflowRequest, string requestId, string waitUntilTaskRef = null, int? version = null)
            => _workflowApi.ExecuteWorkflow(startWorkflowRequest, requestId, startWorkflowRequest.Name, version ?? startWorkflowRequest.Version, waitUntilTaskRef);
        public ThreadTask.Task<WorkflowRun> ExecuteWorkflowAsync(StartWorkflowRequest startWorkflowRequest, string requestId, string waitUntilTaskRef = null, int? version = null)
            => _workflowApi.ExecuteWorkflowAsync(startWorkflowRequest, requestId, startWorkflowRequest.Name, version ?? startWorkflowRequest.Version, waitUntilTaskRef);

        public void PauseWorkflow(string workflowId) => _workflowApi.PauseWorkflow(workflowId);
        public ThreadTask.Task PauseWorkflowAsync(string workflowId) => ThreadTask.Task.Run(() => _workflowApi.PauseWorkflow(workflowId));

        public void ResumeWorkflow(string workflowId) => _workflowApi.ResumeWorkflow(workflowId);
        public ThreadTask.Task ResumeWorkflowAsync(string workflowId) => ThreadTask.Task.Run(() => _workflowApi.ResumeWorkflow(workflowId));

        public void Terminate(string workflowId, string reason = null, bool triggerFailureWorkflow = false)
            => _workflowApi.Terminate(workflowId, reason, triggerFailureWorkflow);
        public ThreadTask.Task TerminateAsync(string workflowId, string reason = null, bool triggerFailureWorkflow = false)
            => ThreadTask.Task.Run(() => _workflowApi.Terminate(workflowId, reason, triggerFailureWorkflow));

        public void Restart(string workflowId, bool useLatestDefinitions = false) => _workflowApi.Restart(workflowId, useLatestDefinitions);
        public ThreadTask.Task RestartAsync(string workflowId, bool useLatestDefinitions = false) => ThreadTask.Task.Run(() => _workflowApi.Restart(workflowId, useLatestDefinitions));

        public void Retry(string workflowId, bool resumeSubworkflowTasks = false) => _workflowApi.Retry(workflowId, resumeSubworkflowTasks);
        public ThreadTask.Task RetryAsync(string workflowId, bool resumeSubworkflowTasks = false) => ThreadTask.Task.Run(() => _workflowApi.Retry(workflowId, resumeSubworkflowTasks));

        public string Rerun(RerunWorkflowRequest rerunWorkflowRequest, string workflowId) => _workflowApi.Rerun(rerunWorkflowRequest, workflowId);
        public ThreadTask.Task<string> RerunAsync(RerunWorkflowRequest rerunWorkflowRequest, string workflowId) => _workflowApi.RerunAsync(rerunWorkflowRequest, workflowId);

        public void SkipTaskFromWorkflow(string workflowId, string taskReferenceName, SkipTaskRequest skipTaskRequest)
            => _workflowApi.SkipTaskFromWorkflow(workflowId, taskReferenceName, skipTaskRequest);
        public ThreadTask.Task SkipTaskFromWorkflowAsync(string workflowId, string taskReferenceName, SkipTaskRequest skipTaskRequest)
            => ThreadTask.Task.Run(() => _workflowApi.SkipTaskFromWorkflow(workflowId, taskReferenceName, skipTaskRequest));

        public Workflow GetWorkflow(string workflowId, bool includeTasks = true) => _workflowApi.GetWorkflow(workflowId, includeTasks);
        public ThreadTask.Task<Workflow> GetWorkflowAsync(string workflowId, bool includeTasks = true) => _workflowApi.GetWorkflowAsync(workflowId, includeTasks);

        public WorkflowStatus GetWorkflowStatusSummary(string workflowId, bool includeOutput = false, bool includeVariables = false)
            => _workflowApi.GetWorkflowStatusSummary(workflowId, includeOutput, includeVariables);
        public ThreadTask.Task<WorkflowStatus> GetWorkflowStatusSummaryAsync(string workflowId, bool includeOutput = false, bool includeVariables = false)
            => _workflowApi.GetWorkflowStatusSummaryAsync(workflowId, includeOutput, includeVariables);

        public void DeleteWorkflow(string workflowId, bool archiveWorkflow = true) => _workflowApi.Delete(workflowId, archiveWorkflow);
        public ThreadTask.Task DeleteWorkflowAsync(string workflowId, bool archiveWorkflow = true) => ThreadTask.Task.Run(() => _workflowApi.Delete(workflowId, archiveWorkflow));

        public Dictionary<string, List<Workflow>> GetWorkflowsByCorrelationIds(List<string> correlationIds, string workflowName, bool includeClosed = false, bool includeTasks = false)
            => _workflowApi.GetWorkflows(correlationIds, workflowName, includeClosed, includeTasks);
        public ThreadTask.Task<Dictionary<string, List<Workflow>>> GetWorkflowsByCorrelationIdsAsync(List<string> correlationIds, string workflowName, bool includeClosed = false, bool includeTasks = false)
            => _workflowApi.GetWorkflowsAsync(correlationIds, workflowName, includeClosed, includeTasks);

        public Workflow UpdateVariables(string workflowId, Dictionary<string, object> variables) => _workflowApi.UpdateWorkflowVariables(workflowId, variables);
        public ThreadTask.Task<Workflow> UpdateVariablesAsync(string workflowId, Dictionary<string, object> variables) => _workflowApi.UpdateWorkflowVariablesAsync(workflowId, variables);

        public WorkflowRun UpdateWorkflowState(string workflowId, WorkflowStateUpdate request, List<string> waitUntilTaskRefs = null, int? waitForSeconds = null)
            => _workflowApi.UpdateWorkflow(workflowId, request, waitUntilTaskRefs, waitForSeconds);
        public ThreadTask.Task<WorkflowRun> UpdateWorkflowStateAsync(string workflowId, WorkflowStateUpdate request, List<string> waitUntilTaskRefs = null, int? waitForSeconds = null)
            => _workflowApi.UpdateWorkflowAsync(workflowId, request, waitUntilTaskRefs, waitForSeconds);

        public ScrollableSearchResultWorkflowSummary Search(string query = null, string freeText = null, int? start = null, int? size = null, string sort = null)
            => _workflowApi.Search(start: start, size: size, sort: sort, freeText: freeText ?? "*", query: query);
        public ThreadTask.Task<ScrollableSearchResultWorkflowSummary> SearchAsync(string query = null, string freeText = null, int? start = null, int? size = null, string sort = null)
            => _workflowApi.SearchAsync(start: start, size: size, sort: sort, freeText: freeText ?? "*", query: query);

        public Workflow TestWorkflow(WorkflowTestRequest testRequest) => _workflowApi.TestWorkflow(testRequest);
        public ThreadTask.Task<Workflow> TestWorkflowAsync(WorkflowTestRequest testRequest) => _workflowApi.TestWorkflowAsync(testRequest);

        public BulkResponse PauseBulk(List<string> workflowIds) => _bulkApi.PauseWorkflow(workflowIds);
        public BulkResponse ResumeBulk(List<string> workflowIds) => _bulkApi.ResumeWorkflow(workflowIds);
        public BulkResponse RestartBulk(List<string> workflowIds, bool useLatestDefinitions = false) => _bulkApi.Restart(workflowIds, useLatestDefinitions);
        public BulkResponse RetryBulk(List<string> workflowIds) => _bulkApi.Retry(workflowIds);
        public BulkResponse TerminateBulk(List<string> workflowIds, string reason = null, bool triggerFailureWorkflow = false) => _bulkApi.Terminate(workflowIds, reason, triggerFailureWorkflow);
    }
}
