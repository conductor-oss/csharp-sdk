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
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Conductor.Client.Events
{
    /// <summary>
    /// Central event dispatcher that manages listeners and dispatches events.
    /// Thread-safe for listener registration and event dispatch.
    /// </summary>
    public class EventDispatcher
    {
        private static EventDispatcher _instance;
        private static readonly object _lock = new object();

        private readonly List<ITaskRunnerEventListener> _taskRunnerListeners = new List<ITaskRunnerEventListener>();
        private readonly List<IWorkflowEventListener> _workflowListeners = new List<IWorkflowEventListener>();
        private readonly ILogger<EventDispatcher> _logger;

        public EventDispatcher(ILogger<EventDispatcher> logger = null)
        {
            _logger = logger;
        }

        public static EventDispatcher Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new EventDispatcher();
                        }
                    }
                }
                return _instance;
            }
        }

        public void Register(ITaskRunnerEventListener listener)
        {
            lock (_taskRunnerListeners)
            {
                _taskRunnerListeners.Add(listener);
            }
        }

        public void Register(IWorkflowEventListener listener)
        {
            lock (_workflowListeners)
            {
                _workflowListeners.Add(listener);
            }
        }

        public void Unregister(ITaskRunnerEventListener listener)
        {
            lock (_taskRunnerListeners)
            {
                _taskRunnerListeners.Remove(listener);
            }
        }

        public void Unregister(IWorkflowEventListener listener)
        {
            lock (_workflowListeners)
            {
                _workflowListeners.Remove(listener);
            }
        }

        // Task Runner Events

        public void OnPolling(string taskType, string workerId, string domain)
        {
            DispatchTaskRunnerEvent(l => l.OnPolling(taskType, workerId, domain));
        }

        public void OnPollSuccess(string taskType, string workerId, List<Task> tasks)
        {
            DispatchTaskRunnerEvent(l => l.OnPollSuccess(taskType, workerId, tasks));
        }

        public void OnPollEmpty(string taskType, string workerId)
        {
            DispatchTaskRunnerEvent(l => l.OnPollEmpty(taskType, workerId));
        }

        public void OnPollError(string taskType, string workerId, Exception exception)
        {
            DispatchTaskRunnerEvent(l => l.OnPollError(taskType, workerId, exception));
        }

        public void OnTaskExecutionStarted(string taskType, Task task)
        {
            DispatchTaskRunnerEvent(l => l.OnTaskExecutionStarted(taskType, task));
        }

        public void OnTaskExecutionCompleted(string taskType, Task task, TaskResult result)
        {
            DispatchTaskRunnerEvent(l => l.OnTaskExecutionCompleted(taskType, task, result));
        }

        public void OnTaskExecutionFailed(string taskType, Task task, Exception exception)
        {
            DispatchTaskRunnerEvent(l => l.OnTaskExecutionFailed(taskType, task, exception));
        }

        public void OnTaskUpdateSent(string taskType, TaskResult result)
        {
            DispatchTaskRunnerEvent(l => l.OnTaskUpdateSent(taskType, result));
        }

        public void OnTaskUpdateFailed(string taskType, TaskResult result, Exception exception)
        {
            DispatchTaskRunnerEvent(l => l.OnTaskUpdateFailed(taskType, result, exception));
        }

        // Workflow Events

        public void OnWorkflowStarted(string workflowId, string workflowType)
        {
            DispatchWorkflowEvent(l => l.OnWorkflowStarted(workflowId, workflowType));
        }

        public void OnWorkflowCompleted(string workflowId, string workflowType)
        {
            DispatchWorkflowEvent(l => l.OnWorkflowCompleted(workflowId, workflowType));
        }

        public void OnWorkflowFailed(string workflowId, string workflowType, string reason)
        {
            DispatchWorkflowEvent(l => l.OnWorkflowFailed(workflowId, workflowType, reason));
        }

        public void OnWorkflowTerminated(string workflowId, string workflowType, string reason)
        {
            DispatchWorkflowEvent(l => l.OnWorkflowTerminated(workflowId, workflowType, reason));
        }

        public void OnWorkflowPaused(string workflowId, string workflowType)
        {
            DispatchWorkflowEvent(l => l.OnWorkflowPaused(workflowId, workflowType));
        }

        public void OnWorkflowResumed(string workflowId, string workflowType)
        {
            DispatchWorkflowEvent(l => l.OnWorkflowResumed(workflowId, workflowType));
        }

        private void DispatchTaskRunnerEvent(Action<ITaskRunnerEventListener> action)
        {
            ITaskRunnerEventListener[] listeners;
            lock (_taskRunnerListeners)
            {
                listeners = _taskRunnerListeners.ToArray();
            }

            foreach (var listener in listeners)
            {
                try
                {
                    action(listener);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning($"Error dispatching task runner event to listener: {ex.Message}");
                }
            }
        }

        private void DispatchWorkflowEvent(Action<IWorkflowEventListener> action)
        {
            IWorkflowEventListener[] listeners;
            lock (_workflowListeners)
            {
                listeners = _workflowListeners.ToArray();
            }

            foreach (var listener in listeners)
            {
                try
                {
                    action(listener);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning($"Error dispatching workflow event to listener: {ex.Message}");
                }
            }
        }
    }
}
