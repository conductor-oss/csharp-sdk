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
    public class OrkesMetadataClient : IMetadataClient
    {
        private readonly IMetadataResourceApi _metadataApi;
        private readonly ITagsApi _tagsApi;

        public OrkesMetadataClient(Configuration configuration)
        {
            _metadataApi = configuration.GetClient<MetadataResourceApi>();
            _tagsApi = configuration.GetClient<TagsApi>();
        }

        public OrkesMetadataClient(IMetadataResourceApi metadataApi, ITagsApi tagsApi)
        {
            _metadataApi = metadataApi;
            _tagsApi = tagsApi;
        }

        public void RegisterWorkflowDef(WorkflowDef workflowDef, bool overwrite = false) => _metadataApi.Create(workflowDef, overwrite);
        public ThreadTask.Task RegisterWorkflowDefAsync(WorkflowDef workflowDef, bool overwrite = false) => _metadataApi.CreateAsync(workflowDef, overwrite);

        public void UpdateWorkflowDefinitions(List<WorkflowDef> workflowDefs, bool overwrite = false) => _metadataApi.UpdateWorkflowDefinitions(workflowDefs, overwrite);
        public ThreadTask.Task UpdateWorkflowDefinitionsAsync(List<WorkflowDef> workflowDefs, bool overwrite = false) => _metadataApi.UpdateWorkflowDefinitionsAsync(workflowDefs, overwrite);

        public void UnregisterWorkflowDef(string name, int version) => _metadataApi.UnregisterWorkflowDef(name, version);
        public ThreadTask.Task UnregisterWorkflowDefAsync(string name, int version) => ThreadTask.Task.Run(() => _metadataApi.UnregisterWorkflowDef(name, version));

        public WorkflowDef GetWorkflowDef(string name, int? version = null) => _metadataApi.Get(name, version);
        public ThreadTask.Task<WorkflowDef> GetWorkflowDefAsync(string name, int? version = null) => _metadataApi.GetAsync(name, version);

        public List<WorkflowDef> GetAllWorkflowDefs() => _metadataApi.GetAllWorkflows();
        public ThreadTask.Task<List<WorkflowDef>> GetAllWorkflowDefsAsync() => _metadataApi.GetAllWorkflowsAsync();

        public void RegisterTaskDefs(List<TaskDef> taskDefs) => _metadataApi.RegisterTaskDef(taskDefs);
        public ThreadTask.Task RegisterTaskDefsAsync(List<TaskDef> taskDefs) => _metadataApi.RegisterTaskDefAsync(taskDefs);

        public void UpdateTaskDef(TaskDef taskDef) => _metadataApi.UpdateTaskDef(taskDef);
        public ThreadTask.Task UpdateTaskDefAsync(TaskDef taskDef) => _metadataApi.UpdateTaskDefAsync(taskDef);

        public void UnregisterTaskDef(string taskType) => _metadataApi.UnregisterTaskDef(taskType);
        public ThreadTask.Task UnregisterTaskDefAsync(string taskType) => ThreadTask.Task.Run(() => _metadataApi.UnregisterTaskDef(taskType));

        public TaskDef GetTaskDef(string taskType) => _metadataApi.GetTaskDef(taskType);
        public ThreadTask.Task<TaskDef> GetTaskDefAsync(string taskType) => _metadataApi.GetTaskDefAsync(taskType);

        public List<TaskDef> GetAllTaskDefs() => _metadataApi.GetTaskDefs();
        public ThreadTask.Task<List<TaskDef>> GetAllTaskDefsAsync() => _metadataApi.GetTaskDefsAsync();

        public void AddWorkflowTag(TagObject tag, string workflowName) => _tagsApi.AddWorkflowTag(tag, workflowName);
        public void AddTaskTag(TagObject tag, string taskName) => _tagsApi.AddTaskTag(tag, taskName);
        public List<TagObject> GetWorkflowTags(string workflowName) => _tagsApi.GetWorkflowTags(workflowName);
        public List<TagObject> GetTaskTags(string taskName) => _tagsApi.GetTaskTags(taskName);
        public void SetWorkflowTags(List<TagObject> tags, string workflowName) => _tagsApi.SetWorkflowTags(tags, workflowName);
        public void SetTaskTags(List<TagObject> tags, string taskName) => _tagsApi.SetTaskTags(tags, taskName);
        public void DeleteWorkflowTag(TagObject tag, string workflowName) => _tagsApi.DeleteWorkflowTag(tag, workflowName);
        public void DeleteTaskTag(TagString tag, string taskName) => _tagsApi.DeleteTaskTag(tag, taskName);
    }
}
