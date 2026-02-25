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

namespace Conductor.Definition.TaskType.LlmTasks
{
    public class ToolSpecTask : Task
    {
        public ToolSpecTask(string taskReferenceName, string name, string description, Dictionary<string, object> inputSchema = null) : base(taskReferenceName, WorkflowTask.WorkflowTaskTypeEnum.TOOLSPEC)
        {
            WithInput("name", name);
            WithInput("description", description);
            if (inputSchema != null) WithInput("inputSchema", inputSchema);
        }
    }
}
