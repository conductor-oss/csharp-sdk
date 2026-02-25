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
using Conductor.Client.Events;
using Conductor.Client.Models;
using System;
using System.Collections.Generic;
using ConductorTask = Conductor.Client.Models.Task;

namespace Conductor.Examples
{
    /// <summary>
    /// Demonstrates the event listener system for monitoring worker and workflow events.
    /// </summary>
    public class EventListenerExample
    {
        public static void Run()
        {
            Console.WriteLine("=== Event Listener Example ===\n");

            var dispatcher = EventDispatcher.Instance;

            // Register a task runner listener
            var taskListener = new LoggingTaskRunnerListener();
            dispatcher.Register(taskListener);

            // Register a workflow listener
            var workflowListener = new LoggingWorkflowListener();
            dispatcher.Register(workflowListener);

            // Simulate events (in real usage, these are dispatched by the worker framework)
            Console.WriteLine("Simulating events...\n");

            dispatcher.OnPolling("my_task", "worker-1", "default");
            dispatcher.OnPollSuccess("my_task", "worker-1", new List<ConductorTask>());
            dispatcher.OnPollEmpty("my_task", "worker-1");

            dispatcher.OnWorkflowStarted("wf-abc-123", "my_workflow");
            dispatcher.OnWorkflowCompleted("wf-abc-123", "my_workflow");

            // Unregister listeners
            dispatcher.Unregister(taskListener);
            dispatcher.Unregister(workflowListener);

            Console.WriteLine("\nEvent Listener Example completed!");
        }
    }

    /// <summary>
    /// Example listener that logs all task runner events.
    /// </summary>
    public class LoggingTaskRunnerListener : ITaskRunnerEventListener
    {
        public void OnPolling(string taskType, string workerId, string domain)
            => Console.WriteLine($"  [TaskRunner] Polling: taskType={taskType}, workerId={workerId}, domain={domain}");

        public void OnPollSuccess(string taskType, string workerId, List<ConductorTask> tasks)
            => Console.WriteLine($"  [TaskRunner] Poll success: taskType={taskType}, taskCount={tasks.Count}");

        public void OnPollEmpty(string taskType, string workerId)
            => Console.WriteLine($"  [TaskRunner] Poll empty: taskType={taskType}");

        public void OnPollError(string taskType, string workerId, Exception exception)
            => Console.WriteLine($"  [TaskRunner] Poll error: taskType={taskType}, error={exception.Message}");

        public void OnTaskExecutionStarted(string taskType, ConductorTask task)
            => Console.WriteLine($"  [TaskRunner] Execution started: taskType={taskType}, taskId={task.TaskId}");

        public void OnTaskExecutionCompleted(string taskType, ConductorTask task, TaskResult result)
            => Console.WriteLine($"  [TaskRunner] Execution completed: taskType={taskType}, status={result.Status}");

        public void OnTaskExecutionFailed(string taskType, ConductorTask task, Exception exception)
            => Console.WriteLine($"  [TaskRunner] Execution failed: taskType={taskType}, error={exception.Message}");

        public void OnTaskUpdateSent(string taskType, TaskResult result)
            => Console.WriteLine($"  [TaskRunner] Update sent: taskType={taskType}, taskId={result.TaskId}");

        public void OnTaskUpdateFailed(string taskType, TaskResult result, Exception exception)
            => Console.WriteLine($"  [TaskRunner] Update failed: taskType={taskType}, error={exception.Message}");
    }

    /// <summary>
    /// Example listener that logs all workflow events.
    /// </summary>
    public class LoggingWorkflowListener : IWorkflowEventListener
    {
        public void OnWorkflowStarted(string workflowId, string workflowType)
            => Console.WriteLine($"  [Workflow] Started: id={workflowId}, type={workflowType}");

        public void OnWorkflowCompleted(string workflowId, string workflowType)
            => Console.WriteLine($"  [Workflow] Completed: id={workflowId}, type={workflowType}");

        public void OnWorkflowFailed(string workflowId, string workflowType, string reason)
            => Console.WriteLine($"  [Workflow] Failed: id={workflowId}, reason={reason}");

        public void OnWorkflowTerminated(string workflowId, string workflowType, string reason)
            => Console.WriteLine($"  [Workflow] Terminated: id={workflowId}, reason={reason}");

        public void OnWorkflowPaused(string workflowId, string workflowType)
            => Console.WriteLine($"  [Workflow] Paused: id={workflowId}");

        public void OnWorkflowResumed(string workflowId, string workflowType)
            => Console.WriteLine($"  [Workflow] Resumed: id={workflowId}");
    }
}
