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
using Conductor.Client.Extensions;
using Conductor.Client.Interfaces;
using Conductor.Client.Models;
using Conductor.Client.Worker;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.Worker
{
    [WorkerTask]
    public class FunctionalWorkers
    {
        private static Random _random;

        static FunctionalWorkers()
        {
            _random = new Random();
        }

        // Polls for 5 task every 100ms
        [WorkerTask("test-sdk-csharp-task", 5, "taskDomain", 100, "simpleWorker")]
        public static TaskResult SimpleWorker(Conductor.Client.Models.Task task)
        {
            return task.Completed();
        }

        // Polls for 5 tasks every 420ms
        [WorkerTask("test-sdk-csharp-task", 5, "taskDomain", 420, "lazyWorker")]
        public TaskResult LazyWorker(Conductor.Client.Models.Task task)
        {
            var timeSpan = System.TimeSpan.FromMilliseconds(_random.Next(100, 900));
            System.Threading.Tasks.Task.Delay(timeSpan).GetAwaiter().GetResult();
            return task.Completed();
        }
    }

    public class ClassWorker : IWorkflowTask
    {
        public string TaskType { get; }

        public WorkflowTaskExecutorConfiguration WorkerSettings { get; }

        public ClassWorker(string taskType = "random_task_type")
        {
            TaskType = taskType;
            WorkerSettings = new WorkflowTaskExecutorConfiguration();
        }

        public System.Threading.Tasks.Task<TaskResult> Execute(Conductor.Client.Models.Task task, CancellationToken token)
        {
            if (token != CancellationToken.None && token.IsCancellationRequested)
                return System.Threading.Tasks.Task.FromException<TaskResult>(new Exception("Token request Cancelled"));

            return System.Threading.Tasks.Task.FromException<TaskResult>(new NotImplementedException());
        }

        public TaskResult Execute(Conductor.Client.Models.Task task)
        {
            throw new NotImplementedException();
        }
    }
}
