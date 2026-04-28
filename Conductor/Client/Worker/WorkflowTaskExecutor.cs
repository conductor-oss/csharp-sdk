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
using Conductor.Client.Interfaces;
using Conductor.Client.Extensions;
using Conductor.Client.Telemetry;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using Conductor.Client.Models;

namespace Conductor.Client.Worker
{
    internal class WorkflowTaskExecutor : IWorkflowTaskExecutor
    {
        private static TimeSpan SLEEP_FOR_TIME_SPAN_ON_WORKER_ERROR = TimeSpan.FromMilliseconds(10);
        private static int UPDATE_TASK_RETRY_COUNT_LIMIT = 5;

        private readonly ILogger<WorkflowTaskExecutor> _logger;
        private readonly IWorkflowTask _worker;
        private readonly IWorkflowTaskClient _taskClient;
        private readonly WorkflowTaskExecutorConfiguration _workerSettings;
        private readonly WorkflowTaskMonitor _workflowTaskMonitor;
        private readonly MetricsCollector _metrics;

        // Adaptive backoff state
        private TimeSpan _currentBackoff;
        private int _consecutiveEmptyPolls;

        // task-update-v2: once the server returns 404/405, fall back to v1 for all future updates
        private bool _useUpdateV2 = true;

        public WorkflowTaskExecutor(
            ILogger<WorkflowTaskExecutor> logger,
            IWorkflowTaskClient client,
            IWorkflowTask worker,
            WorkflowTaskMonitor workflowTaskMonitor,
            MetricsCollector metrics = null)
        {
            _logger = logger;
            _taskClient = client;
            _worker = worker;
            _workerSettings = worker.WorkerSettings;
            _workflowTaskMonitor = workflowTaskMonitor;
            _metrics = metrics;
            _currentBackoff = _workerSettings.PollInterval;
            _consecutiveEmptyPolls = 0;
        }

        public WorkflowTaskExecutor(
            ILogger<WorkflowTaskExecutor> logger,
            IWorkflowTaskClient client,
            IWorkflowTask worker,
            WorkflowTaskExecutorConfiguration workflowTaskConfiguration,
            WorkflowTaskMonitor workflowTaskMonitor,
            MetricsCollector metrics = null)
        {
            _logger = logger;
            _taskClient = client;
            _worker = worker;
            _workflowTaskMonitor = workflowTaskMonitor;
            _metrics = metrics;
            _workerSettings = worker.WorkerSettings;
            _currentBackoff = _workerSettings.PollInterval;
            _consecutiveEmptyPolls = 0;
        }

        public System.Threading.Tasks.Task Start(CancellationToken token)
        {
            if (token != CancellationToken.None)
                token.ThrowIfCancellationRequested();

            var thread = System.Threading.Tasks.Task.Run(() => StartWithAutoRestart(token));
            _logger.LogInformation(
                $"[{_workerSettings.WorkerId}] Started worker"
                + $", taskName: {_worker.TaskType}"
                + $", domain: {_workerSettings.Domain}"
                + $", pollInterval: {_workerSettings.PollInterval}"
                + $", batchSize: {_workerSettings.BatchSize}"
            );

            if (token != CancellationToken.None)
                token.ThrowIfCancellationRequested();

            return thread;
        }

        private void StartWithAutoRestart(CancellationToken token)
        {
            var restartCount = 0;
            while (true)
            {
                try
                {
                    Work4Ever(token);
                    return;
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception e)
                {
                    restartCount++;
                    if (_workerSettings.MaxRestartAttempts > 0 && restartCount > _workerSettings.MaxRestartAttempts)
                    {
                        _logger.LogError(
                            $"[{_workerSettings.WorkerId}] Worker exceeded max restart attempts ({_workerSettings.MaxRestartAttempts})"
                            + $", taskName: {_worker.TaskType}. Giving up."
                        );
                        throw;
                    }

                    _logger.LogWarning(
                        $"[{_workerSettings.WorkerId}] Worker crashed, restarting (attempt {restartCount}/{_workerSettings.MaxRestartAttempts})"
                        + $", taskName: {_worker.TaskType}"
                        + $", error: {e.Message}"
                    );
                    ConductorMetrics.WorkerRestartCount.Add(1,
                        new KeyValuePair<string, object>("taskType", _worker.TaskType));
                    Sleep(_workerSettings.RestartDelay);
                }
            }
        }

        private void Work4Ever(CancellationToken token)
        {
            while (true)
            {
                try
                {
                    if (token != CancellationToken.None)
                        token.ThrowIfCancellationRequested();

                    if (IsWorkerPaused())
                    {
                        _logger.LogDebug(
                            $"[{_workerSettings.WorkerId}] Worker paused via environment variable '{_workerSettings.PauseEnvironmentVariable}'"
                            + $", taskName: {_worker.TaskType}"
                        );
                        Sleep(_workerSettings.PauseCheckInterval);
                        continue;
                    }

                    WorkOnce(token);
                }
                catch (System.OperationCanceledException canceledException)
                {
                    _logger.LogTrace(
                        $"[{_workerSettings.WorkerId}] Operation Cancelled: {canceledException.Message}"
                        + $", taskName: {_worker.TaskType}"
                        + $", domain: {_worker.WorkerSettings.Domain}"
                        + $", batchSize: {_workerSettings.BatchSize}"
                    );
                    Sleep(SLEEP_FOR_TIME_SPAN_ON_WORKER_ERROR);
                }
                catch (Exception e)
                {
                    _metrics?.RecordUncaughtException();
                    _logger.LogError(
                        $"[{_workerSettings.WorkerId}] worker error: {e.Message}"
                        + $", taskName: {_worker.TaskType}"
                        + $", domain: {_worker.WorkerSettings.Domain}"
                        + $", batchSize: {_workerSettings.BatchSize}"
                    );
                    IncreaseBackoff();
                    Sleep(_currentBackoff);
                }
            }
        }

        private async void WorkOnce(CancellationToken token)
        {
            if (token != CancellationToken.None)
                token.ThrowIfCancellationRequested();

            var tasks = PollTasks();
            if (tasks.Count == 0)
            {
                IncreaseBackoff();
                Sleep(_currentBackoff);
                return;
            }

            ResetBackoff();

            var uniqueBatchId = Guid.NewGuid();
            _logger.LogTrace(
                $"[{_workerSettings.WorkerId}] Processing tasks batch"
                + $", Task batch unique Id: {uniqueBatchId}"
            );

            await System.Threading.Tasks.Task.Run(() => ProcessTasks(tasks, token));

            _logger.LogTrace(
                $"[{_workerSettings.WorkerId}] Completed tasks batch"
                + $", Task batch unique Id: {uniqueBatchId}"
            );
        }

        private List<Models.Task> PollTasks()
        {
            _logger.LogTrace(
                $"[{_workerSettings.WorkerId}] Polling for worker"
                + $", taskType: {_worker.TaskType}"
                + $", domain: {_workerSettings.Domain}"
                + $", batchSize: {_workerSettings.BatchSize}"
            );

            var availableWorkerCounter = _workerSettings.BatchSize - _workflowTaskMonitor.GetRunningWorkers();
            if (availableWorkerCounter < 1)
            {
                _logger.LogDebug("All workers are busy");
                _metrics?.RecordTaskExecutionQueueFull(_worker.TaskType);
                return new List<Task>();
            }

            var tags = new KeyValuePair<string, object>("taskType", _worker.TaskType);
            ConductorMetrics.TaskPollCount.Add(1, tags);

            try
            {
                Models.Task[] tasks;
                using (ConductorMetrics.Time(ConductorMetrics.TaskPollLatency, tags))
                {
                    var result = _taskClient.PollTask(_worker.TaskType, _workerSettings.WorkerId, _workerSettings.Domain, availableWorkerCounter);
                    tasks = result == null ? Array.Empty<Models.Task>() : result.ToArray();
                }

                _logger.LogTrace(
                    $"[{_workerSettings.WorkerId}] Polled {tasks.Length} tasks"
                    + $", taskType: {_worker.TaskType}"
                    + $", domain: {_workerSettings.Domain}"
                    + $", batchSize: {_workerSettings.BatchSize}"
                );

                if (tasks.Length == 0)
                {
                    ConductorMetrics.TaskPollEmptyCount.Add(1, tags);
                    EventDispatcher.Instance.OnPollEmpty(_worker.TaskType, _workerSettings.WorkerId);
                }
                else
                {
                    ConductorMetrics.TaskPollSuccessCount.Add(1, tags);
                    EventDispatcher.Instance.OnPollSuccess(_worker.TaskType, _workerSettings.WorkerId, new List<Models.Task>(tasks));
                }

                _workflowTaskMonitor.RecordPollSuccess(tasks.Length);
                return new List<Models.Task>(tasks);
            }
            catch (Exception e)
            {
                _logger.LogTrace(
                    $"[{_workerSettings.WorkerId}] Polling error: {e.Message}"
                    + $", taskType: {_worker.TaskType}"
                    + $", domain: {_workerSettings.Domain}"
                    + $", batchSize: {_workerSettings.BatchSize}"
                );
                ConductorMetrics.TaskPollErrorCount.Add(1, tags);
                EventDispatcher.Instance.OnPollError(_worker.TaskType, _workerSettings.WorkerId, e);
                _workflowTaskMonitor.RecordPollError();
                return new List<Task>();
            }
        }

        private async void ProcessTasks(List<Models.Task> tasks, CancellationToken token)
        {
            List<System.Threading.Tasks.Task> threads = new List<System.Threading.Tasks.Task>();
            if (tasks == null || tasks.Count == 0)
                return;

            foreach (var task in tasks)
            {
                if (token != CancellationToken.None)
                    token.ThrowIfCancellationRequested();

                _workflowTaskMonitor.IncrementRunningWorker();
                threads.Add(System.Threading.Tasks.Task.Run(() => ProcessTask(task, token)));
            }

            await System.Threading.Tasks.Task.WhenAll(threads);
        }

        private async void ProcessTask(Models.Task task, CancellationToken token)
        {
            if (token != CancellationToken.None)
                token.ThrowIfCancellationRequested();

            _logger.LogTrace(
                $"[{_workerSettings.WorkerId}] Processing task for worker"
                + $", taskType: {_worker.TaskType}"
                + $", domain: {_workerSettings.Domain}"
                + $", taskId: {task.TaskId}"
                + $", workflowId: {task.WorkflowInstanceId}"
                + $", CancelToken: {token}"
            );

            var tags = new KeyValuePair<string, object>("taskType", _worker.TaskType);
            ConductorMetrics.TaskExecutionCount.Add(1, tags);
            EventDispatcher.Instance.OnTaskExecutionStarted(_worker.TaskType, task);

            Timer leaseTimer = null;
            if (_workerSettings.LeaseExtensionEnabled)
                leaseTimer = StartLeaseExtensionTimer(task);

            try
            {
                TaskResult taskResult;
                using (ConductorMetrics.Time(ConductorMetrics.TaskExecutionLatency, tags))
                {
                    if (token == CancellationToken.None)
                        taskResult = _worker.Execute(task);
                    else
                        taskResult = await _worker.Execute(task, token);
                }

                _logger.LogTrace(
                    $"[{_workerSettings.WorkerId}] Done processing task for worker"
                    + $", taskType: {_worker.TaskType}"
                    + $", domain: {_workerSettings.Domain}"
                    + $", taskId: {task.TaskId}"
                    + $", workflowId: {task.WorkflowInstanceId}"
                    + $", CancelToken: {token}"
                );

                ConductorMetrics.TaskExecutionSuccessCount.Add(1, tags);
                EventDispatcher.Instance.OnTaskExecutionCompleted(_worker.TaskType, task, taskResult);

                var nextTask = UpdateTask(taskResult);
                _workflowTaskMonitor.RecordTaskSuccess();

                if (nextTask != null && (token == CancellationToken.None || !token.IsCancellationRequested))
                {
                    _workflowTaskMonitor.IncrementRunningWorker();
                    _ = System.Threading.Tasks.Task.Run(() => ProcessTask(nextTask, token));
                }
            }
            catch (NonRetryableException e)
            {
                _logger.LogError(
                    $"[{_workerSettings.WorkerId}] Non-retryable failure for task"
                    + $", taskType: {_worker.TaskType}"
                    + $", taskId: {task.TaskId}"
                    + $", reason: {e.Message}"
                );
                ConductorMetrics.TaskExecutionErrorCount.Add(1, tags);
                EventDispatcher.Instance.OnTaskExecutionFailed(_worker.TaskType, task, e);

                var taskResult = new TaskResult(taskId: task.TaskId, workflowInstanceId: task.WorkflowInstanceId);
                taskResult.Status = TaskResult.StatusEnum.FAILEDWITHTERMINALERROR;
                taskResult.ReasonForIncompletion = e.Message;
                taskResult.Logs = new List<TaskExecLog>
                {
                    new TaskExecLog { Log = e.ToString(), CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }
                };
                UpdateTask(taskResult);
                _workflowTaskMonitor.RecordTaskError();
            }
            catch (Exception e)
            {
                _logger.LogError(
                    $"[{_workerSettings.WorkerId}] Failed to process task for worker, reason: {e.Message}"
                    + $", taskType: {_worker.TaskType}"
                    + $", domain: {_workerSettings.Domain}"
                    + $", taskId: {task.TaskId}"
                    + $", workflowId: {task.WorkflowInstanceId}"
                    + $", CancelToken: {token}"
                );
                ConductorMetrics.TaskExecutionErrorCount.Add(1, tags);
                EventDispatcher.Instance.OnTaskExecutionFailed(_worker.TaskType, task, e);

                var taskResult = task.Failed(e.Message);
                taskResult.Logs = new List<TaskExecLog>
                {
                    new TaskExecLog { Log = e.ToString(), CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }
                };
                UpdateTask(taskResult);
                _workflowTaskMonitor.RecordTaskError();
            }
            finally
            {
                leaseTimer?.Dispose();

                if (token != CancellationToken.None)
                    token.ThrowIfCancellationRequested();
                _workflowTaskMonitor.RunningWorkerDone();
            }
        }

        private Timer StartLeaseExtensionTimer(Models.Task task)
        {
            var thresholdMs = (int)_workerSettings.LeaseExtensionThreshold.TotalMilliseconds;
            return new Timer(
                callback: _ =>
                {
                    try
                    {
                        _logger.LogDebug(
                            $"[{_workerSettings.WorkerId}] Extending lease for task"
                            + $", taskId: {task.TaskId}"
                            + $", workflowId: {task.WorkflowInstanceId}"
                        );
                        var extendResult = new TaskResult(
                            taskId: task.TaskId,
                            workflowInstanceId: task.WorkflowInstanceId
                        );
                        extendResult.Status = TaskResult.StatusEnum.INPROGRESS;
                        extendResult.CallbackAfterSeconds = (int)(_workerSettings.LeaseExtensionThreshold.TotalSeconds);
                        _taskClient.UpdateTask(extendResult);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(
                            $"[{_workerSettings.WorkerId}] Failed to extend task lease: {ex.Message}"
                            + $", taskId: {task.TaskId}"
                        );
                    }
                },
                state: null,
                dueTime: thresholdMs,
                period: thresholdMs
            );
        }

        /// <summary>
        /// Updates a task result and returns the next task if the server supports task-update-v2,
        /// or null when falling back to v1. Falls back permanently on HTTP 404/405.
        /// </summary>
        internal Models.Task UpdateTask(Models.TaskResult taskResult)
        {
            taskResult.WorkerId = taskResult.WorkerId ?? _workerSettings.WorkerId;
            RecordTaskResultSize(taskResult);
            var tags = new KeyValuePair<string, object>("taskType", _worker.TaskType);
            ConductorMetrics.TaskUpdateCount.Add(1, tags);

            for (var attemptCounter = 0; attemptCounter < UPDATE_TASK_RETRY_COUNT_LIMIT; attemptCounter += 1)
            {
                try
                {
                    if (attemptCounter > 0)
                    {
                        ConductorMetrics.TaskUpdateRetryCount.Add(1, tags);
                        Sleep(TimeSpan.FromSeconds(1 << attemptCounter));
                    }

                    Models.Task nextTask;
                    using (ConductorMetrics.Time(ConductorMetrics.TaskUpdateLatency, tags))
                    {
                        if (_useUpdateV2)
                            nextTask = _taskClient.UpdateTaskAndGetNext(taskResult, _worker.TaskType, _workerSettings.WorkerId, _workerSettings.Domain);
                        else
                        {
                            _taskClient.UpdateTask(taskResult);
                            nextTask = null;
                        }
                    }

                    _logger.LogTrace(
                        $"[{_workerSettings.WorkerId}] Done updating task"
                        + $", taskType: {_worker.TaskType}"
                        + $", domain: {_workerSettings.Domain}"
                        + $", taskId: {taskResult.TaskId}"
                        + $", workflowId: {taskResult.WorkflowInstanceId}"
                        + (nextTask != null ? $", nextTaskId: {nextTask.TaskId}" : ", no next task")
                    );
                    EventDispatcher.Instance.OnTaskUpdateSent(_worker.TaskType, taskResult);
                    return nextTask;
                }
                catch (ApiException e) when (_useUpdateV2 && (e.ErrorCode == 404 || e.ErrorCode == 405))
                {
                    _useUpdateV2 = false;
                    _logger.LogWarning(
                        $"[{_workerSettings.WorkerId}] Server does not support task-update-v2 (HTTP {e.ErrorCode}), falling back to v1"
                        + $", taskType: {_worker.TaskType}"
                    );
                    _taskClient.UpdateTask(taskResult);
                    EventDispatcher.Instance.OnTaskUpdateSent(_worker.TaskType, taskResult);
                    return null;
                }
                catch (Exception e)
                {
                    _logger.LogError(
                        $"[{_workerSettings.WorkerId}] Failed to update task, reason: {e.Message}"
                        + $", taskType: {_worker.TaskType}"
                        + $", domain: {_workerSettings.Domain}"
                        + $", taskId: {taskResult.TaskId}"
                        + $", workflowId: {taskResult.WorkflowInstanceId}"
                    );
                    ConductorMetrics.TaskUpdateErrorCount.Add(1, tags);
                }
            }

            _metrics?.RecordTaskUpdateError(_worker.TaskType);
            EventDispatcher.Instance.OnTaskUpdateFailed(_worker.TaskType, taskResult, new Exception("Failed to update task after retries"));
            throw new Exception("Failed to update task after retries");
        }

        private void RecordTaskResultSize(Models.TaskResult taskResult)
        {
            if (_metrics == null || taskResult.OutputData == null)
                return;
            try
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(taskResult.OutputData);
                _metrics.RecordTaskResultSize(_worker.TaskType, json.Length);
            }
            catch
            {
                // Don't let metrics serialization failures disrupt task processing.
            }
        }

        private bool IsWorkerPaused()
        {
            if (string.IsNullOrEmpty(_workerSettings.PauseEnvironmentVariable))
                return false;

            var value = Environment.GetEnvironmentVariable(_workerSettings.PauseEnvironmentVariable);
            return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }

        private void IncreaseBackoff()
        {
            _consecutiveEmptyPolls++;
            var newBackoff = TimeSpan.FromMilliseconds(
                _workerSettings.PollInterval.TotalMilliseconds * Math.Pow(_workerSettings.PollBackoffMultiplier, _consecutiveEmptyPolls)
            );
            _currentBackoff = newBackoff > _workerSettings.MaxPollBackoffInterval
                ? _workerSettings.MaxPollBackoffInterval
                : newBackoff;

            _logger.LogTrace(
                $"[{_workerSettings.WorkerId}] Backoff increased to {_currentBackoff.TotalMilliseconds}ms"
                + $", consecutiveEmptyPolls: {_consecutiveEmptyPolls}"
                + $", taskType: {_worker.TaskType}"
            );
        }

        private void ResetBackoff()
        {
            if (_consecutiveEmptyPolls > 0)
            {
                _logger.LogTrace(
                    $"[{_workerSettings.WorkerId}] Backoff reset to {_workerSettings.PollInterval.TotalMilliseconds}ms"
                    + $", taskType: {_worker.TaskType}"
                );
            }
            _consecutiveEmptyPolls = 0;
            _currentBackoff = _workerSettings.PollInterval;
        }

        private void Sleep(TimeSpan timeSpan)
        {
            _logger.LogDebug($"[{_workerSettings.WorkerId}] Sleeping for {timeSpan.TotalMilliseconds}ms");
            Thread.Sleep(timeSpan);
        }
    }
}
