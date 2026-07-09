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
using Conductor.Client.Interfaces;
using Conductor.Client.Models;
using System.Collections.Generic;

namespace Conductor.Client.Worker
{
    public class WorkflowTaskHttpClient : IWorkflowTaskClient
    {
        private readonly TaskResourceApi _client;
        public WorkflowTaskHttpClient(Configuration configuration)
        {
            _client = configuration.GetClient<TaskResourceApi>();
        }

        public List<Task> PollTask(string taskType, string workerId, string domain, int count = 1)
        {
            return _client.BatchPoll(taskType, workerId, domain, count);
        }

        public string UpdateTask(TaskResult result)
        {
            return _client.UpdateTask(result);
        }

        public async System.Threading.Tasks.Task<List<Task>> PollTaskAsync(string taskType, string workerId, string domain, int count = 1)
        {
            return await _client.BatchPollAsync(taskType, workerId, domain, count);
        }

        public async System.Threading.Tasks.Task<string> UpdateTaskAsync(TaskResult result)
        {
            return await _client.UpdateTaskAsync(result);
        }
    }
}
