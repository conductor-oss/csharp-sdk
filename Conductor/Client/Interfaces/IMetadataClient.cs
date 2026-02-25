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
    public interface IMetadataClient
    {
        void RegisterWorkflowDef(WorkflowDef workflowDef, bool overwrite = false);
        ThreadTask.Task RegisterWorkflowDefAsync(WorkflowDef workflowDef, bool overwrite = false);

        void UpdateWorkflowDefinitions(List<WorkflowDef> workflowDefs, bool overwrite = false);
        ThreadTask.Task UpdateWorkflowDefinitionsAsync(List<WorkflowDef> workflowDefs, bool overwrite = false);

        void UnregisterWorkflowDef(string name, int version);
        ThreadTask.Task UnregisterWorkflowDefAsync(string name, int version);

        WorkflowDef GetWorkflowDef(string name, int? version = null);
        ThreadTask.Task<WorkflowDef> GetWorkflowDefAsync(string name, int? version = null);

        List<WorkflowDef> GetAllWorkflowDefs();
        ThreadTask.Task<List<WorkflowDef>> GetAllWorkflowDefsAsync();

        void RegisterTaskDefs(List<TaskDef> taskDefs);
        ThreadTask.Task RegisterTaskDefsAsync(List<TaskDef> taskDefs);

        void UpdateTaskDef(TaskDef taskDef);
        ThreadTask.Task UpdateTaskDefAsync(TaskDef taskDef);

        void UnregisterTaskDef(string taskType);
        ThreadTask.Task UnregisterTaskDefAsync(string taskType);

        TaskDef GetTaskDef(string taskType);
        ThreadTask.Task<TaskDef> GetTaskDefAsync(string taskType);

        List<TaskDef> GetAllTaskDefs();
        ThreadTask.Task<List<TaskDef>> GetAllTaskDefsAsync();

        void AddWorkflowTag(TagObject tag, string workflowName);
        void AddTaskTag(TagObject tag, string taskName);
        List<TagObject> GetWorkflowTags(string workflowName);
        List<TagObject> GetTaskTags(string taskName);
        void SetWorkflowTags(List<TagObject> tags, string workflowName);
        void SetTaskTags(List<TagObject> tags, string taskName);
        void DeleteWorkflowTag(TagObject tag, string workflowName);
        void DeleteTaskTag(TagString tag, string taskName);
    }
}
