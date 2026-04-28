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
    public interface IWorkflowClient
    {
        string StartWorkflow(StartWorkflowRequest startWorkflowRequest);
        ThreadTask.Task<string> StartWorkflowAsync(StartWorkflowRequest startWorkflowRequest);

        WorkflowRun ExecuteWorkflow(StartWorkflowRequest startWorkflowRequest, string requestId, string waitUntilTaskRef = null, int? version = null);
        ThreadTask.Task<WorkflowRun> ExecuteWorkflowAsync(StartWorkflowRequest startWorkflowRequest, string requestId, string waitUntilTaskRef = null, int? version = null);

        void PauseWorkflow(string workflowId);
        ThreadTask.Task PauseWorkflowAsync(string workflowId);

        void ResumeWorkflow(string workflowId);
        ThreadTask.Task ResumeWorkflowAsync(string workflowId);

        void Terminate(string workflowId, string reason = null, bool triggerFailureWorkflow = false);
        ThreadTask.Task TerminateAsync(string workflowId, string reason = null, bool triggerFailureWorkflow = false);

        void Restart(string workflowId, bool useLatestDefinitions = false);
        ThreadTask.Task RestartAsync(string workflowId, bool useLatestDefinitions = false);

        void Retry(string workflowId, bool resumeSubworkflowTasks = false);
        ThreadTask.Task RetryAsync(string workflowId, bool resumeSubworkflowTasks = false);

        string Rerun(RerunWorkflowRequest rerunWorkflowRequest, string workflowId);
        ThreadTask.Task<string> RerunAsync(RerunWorkflowRequest rerunWorkflowRequest, string workflowId);

        void SkipTaskFromWorkflow(string workflowId, string taskReferenceName, SkipTaskRequest skipTaskRequest);
        ThreadTask.Task SkipTaskFromWorkflowAsync(string workflowId, string taskReferenceName, SkipTaskRequest skipTaskRequest);

        Workflow GetWorkflow(string workflowId, bool includeTasks = true);
        ThreadTask.Task<Workflow> GetWorkflowAsync(string workflowId, bool includeTasks = true);

        WorkflowStatus GetWorkflowStatusSummary(string workflowId, bool includeOutput = false, bool includeVariables = false);
        ThreadTask.Task<WorkflowStatus> GetWorkflowStatusSummaryAsync(string workflowId, bool includeOutput = false, bool includeVariables = false);

        void DeleteWorkflow(string workflowId, bool archiveWorkflow = true);
        ThreadTask.Task DeleteWorkflowAsync(string workflowId, bool archiveWorkflow = true);

        Dictionary<string, List<Workflow>> GetWorkflowsByCorrelationIds(List<string> correlationIds, string workflowName, bool includeClosed = false, bool includeTasks = false);
        ThreadTask.Task<Dictionary<string, List<Workflow>>> GetWorkflowsByCorrelationIdsAsync(List<string> correlationIds, string workflowName, bool includeClosed = false, bool includeTasks = false);

        Workflow UpdateVariables(string workflowId, Dictionary<string, object> variables);
        ThreadTask.Task<Workflow> UpdateVariablesAsync(string workflowId, Dictionary<string, object> variables);

        WorkflowRun UpdateWorkflowState(string workflowId, WorkflowStateUpdate request, List<string> waitUntilTaskRefs = null, int? waitForSeconds = null);
        ThreadTask.Task<WorkflowRun> UpdateWorkflowStateAsync(string workflowId, WorkflowStateUpdate request, List<string> waitUntilTaskRefs = null, int? waitForSeconds = null);

        ScrollableSearchResultWorkflowSummary Search(string query = null, string freeText = null, int? start = null, int? size = null, string sort = null);
        ThreadTask.Task<ScrollableSearchResultWorkflowSummary> SearchAsync(string query = null, string freeText = null, int? start = null, int? size = null, string sort = null);

        Workflow TestWorkflow(WorkflowTestRequest testRequest);
        ThreadTask.Task<Workflow> TestWorkflowAsync(WorkflowTestRequest testRequest);

        BulkResponse PauseBulk(List<string> workflowIds);
        BulkResponse ResumeBulk(List<string> workflowIds);
        BulkResponse RestartBulk(List<string> workflowIds, bool useLatestDefinitions = false);
        BulkResponse RetryBulk(List<string> workflowIds);
        BulkResponse TerminateBulk(List<string> workflowIds, string reason = null, bool triggerFailureWorkflow = false);
    }
}
