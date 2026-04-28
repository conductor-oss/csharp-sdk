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
using System;

namespace Conductor.Client.Worker
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class WorkerTask : Attribute
    {
        public string TaskType { get; set; }
        public WorkflowTaskExecutorConfiguration WorkerSettings { get; set; }

        /// <summary>
        /// When true, the task definition is automatically registered with the Conductor server
        /// on worker startup. Requires an <see cref="Interfaces.IMetadataClient"/> registered in DI.
        /// </summary>
        public bool RegisterTaskDef { get; set; } = false;

        /// <summary>
        /// Optional description stored in the task definition when <see cref="RegisterTaskDef"/> is true.
        /// </summary>
        public string Description { get; set; } = null;

        /// <summary>
        /// Task timeout in seconds written to the task definition when <see cref="RegisterTaskDef"/> is true.
        /// 0 means no timeout.
        /// </summary>
        public int TimeoutSeconds { get; set; } = 0;

        public WorkerTask()
        {
            WorkerSettings = new WorkflowTaskExecutorConfiguration();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkerTask" /> class.
        /// </summary>
        /// <param name="taskType"></param>
        /// <param name="batchSize"></param>
        /// <param name="domain"></param>
        /// <param name="pollIntervalMs"></param>
        /// <param name="workerId"></param>
        /// <param name="registerTaskDef">Auto-register the task definition on startup</param>
        /// <param name="description">Task description (used when registering)</param>
        /// <param name="timeoutSeconds">Task timeout in seconds (used when registering)</param>
        public WorkerTask(string taskType = default, int batchSize = default, string domain = default,
            int pollIntervalMs = default, string workerId = default,
            bool registerTaskDef = false, string description = null, int timeoutSeconds = 0)
        {
            TaskType = taskType;
            WorkerSettings = new WorkflowTaskExecutorConfiguration
            {
                BatchSize = batchSize,
                Domain = domain,
                PollInterval = TimeSpan.FromMilliseconds(pollIntervalMs),
                WorkerId = workerId,
            };
            RegisterTaskDef = registerTaskDef;
            Description = description;
            TimeoutSeconds = timeoutSeconds;
        }
    }
}
