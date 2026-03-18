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

namespace Conductor.Client.Interfaces
{
    public interface IWorkflowTaskClient
    {
        List<Task> PollTask(string taskType, string workerId, string domain, int count);
        string UpdateTask(TaskResult result);

        /// <summary>
        /// Update a task and retrieve the next available task in one call (task-update-v2).
        /// Returns the next task if available, or null if the queue is empty or the server
        /// does not support v2 (in which case a separate poll should be performed).
        /// </summary>
        Task UpdateTaskAndGetNext(TaskResult result, string taskType, string workerId, string domain);
    }
}
