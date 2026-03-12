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
using System;

namespace Conductor.Definition.TaskType
{
    public class WaitTask : Task
    {
        private static string DURATION_PARAMETER = "duration";
        private static string UNTIL_PARAMETER = "until";

        public WaitTask(string taskReferenceName, TimeSpan duration) : base(taskReferenceName, WorkflowTask.WorkflowTaskTypeEnum.WAIT)
        {
            WithInput(DURATION_PARAMETER, duration.TotalSeconds.ToString() + "s");
        }

        public WaitTask(string taskReferenceName, DateTime until) : base(taskReferenceName, WorkflowTask.WorkflowTaskTypeEnum.WAIT)
        {
            WithInput(UNTIL_PARAMETER, until.ToString("yyyy-MM-dd HH:mm z"));
        }
    }
}
