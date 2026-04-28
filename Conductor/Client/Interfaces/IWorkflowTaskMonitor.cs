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
using System.Threading;

namespace Conductor.Client.Interfaces
{
    public interface IWorkflowTaskMonitor
    {
        void IncrementRunningWorker();
        int GetRunningWorkers();
        void RunningWorkerDone();
        void RecordPollSuccess(int taskCount);
        void RecordPollError();
        void RecordTaskSuccess();
        void RecordTaskError();
        bool IsHealthy();
        WorkerHealthStatus GetHealthStatus();
    }

    public class WorkerHealthStatus
    {
        public bool IsHealthy { get; set; }
        public int RunningWorkers { get; set; }
        public int ConsecutivePollErrors { get; set; }
        public int TotalTasksProcessed { get; set; }
        public int TotalTaskErrors { get; set; }
        public int TotalPollErrors { get; set; }
        public System.DateTime? LastPollTime { get; set; }
        public System.DateTime? LastTaskCompletedTime { get; set; }
        public System.DateTime? LastErrorTime { get; set; }
    }
}