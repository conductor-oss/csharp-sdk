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
using Conductor.Client.Interfaces;
using Conductor.Client.Extensions;
using Conductor.Client.Telemetry;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        }

        public System.Threading.Tasks.Task Start(CancellationToken token)
        {
            if (token != CancellationToken.None)
                token.ThrowIfCancellationRequested();

            var thread = System.Threading.Tasks.Task.Run(() => Work4Ever(token));
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

        private void Work4Ever(CancellationToken token)
        {
            while (true)
            {
                try
                {
                    if (token != CancellationToken.None)
                        token.ThrowIfCancellationRequested();

                    WorkOnce(token);
                }
                catch (System.OperationCanceledException canceledException)
                {
                    //Do nothing the operation was cancelled
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
                    Sleep(SLEEP_FOR_TIME_SPAN_ON_WORKER_ERROR);
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
                Sleep(_workerSettings.PollInterval);
                return;
            }

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
            var runningWorkers = _workflowTaskMonitor.GetRunningWorkers();
            _metrics?.RecordActiveWorkers(_worker.TaskType, runningWorkers);
            var availableWorkerCounter = _workerSettings.BatchSize - runningWorkers;
            if (availableWorkerCounter < 1)
            {
                _logger.LogDebug("All workers are busy");
                _metrics?.RecordTaskExecutionQueueFull(_worker.TaskType);
                return new List<Task>();
            }

            _metrics?.RecordTaskPoll(_worker.TaskType);
            var pollStopwatch = Stopwatch.StartNew();
            try
            {
                var tasks = _taskClient.PollTask(_worker.TaskType, _workerSettings.WorkerId, _workerSettings.Domain,
                    availableWorkerCounter);
                pollStopwatch.Stop();
                _metrics?.RecordTaskPollTime(_worker.TaskType, pollStopwatch.Elapsed.TotalSeconds);

                if (tasks == null)
                {
                    tasks = new List<Models.Task>();
                }

                _logger.LogTrace(
                    $"[{_workerSettings.WorkerId}] Polled {tasks.Count} tasks"
                    + $", taskType: {_worker.TaskType}"
                    + $", domain: {_workerSettings.Domain}"
                    + $", batchSize: {_workerSettings.BatchSize}"
                );
                return tasks;
            }
            catch (Exception e)
            {
                pollStopwatch.Stop();
                _metrics?.RecordTaskPollTime(_worker.TaskType, pollStopwatch.Elapsed.TotalSeconds);
                _metrics?.RecordTaskPollError(_worker.TaskType, e.GetType().Name);
                _logger.LogTrace(
                    $"[{_workerSettings.WorkerId}] Polling error: {e.Message} "
                    + $", taskType: {_worker.TaskType}"
                    + $", domain: {_workerSettings.Domain}"
                    + $", batchSize: {_workerSettings.BatchSize}"
                );
                return new List<Task>();
            }
        }

        private async void ProcessTasks(List<Models.Task> tasks, CancellationToken token)
        {
            List<System.Threading.Tasks.Task> threads = new List<System.Threading.Tasks.Task>();
            if (tasks == null || tasks.Count == 0)
            {
                return;
            }

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

            var executeStopwatch = Stopwatch.StartNew();
            try
            {
                TaskResult taskResult =
                    new TaskResult(taskId: task.TaskId, workflowInstanceId: task.WorkflowInstanceId);

                if (token == CancellationToken.None)
                    taskResult = _worker.Execute(task);
                else
                    taskResult = await _worker.Execute(task, token);

                executeStopwatch.Stop();
                _metrics?.RecordTaskExecuteTime(_worker.TaskType, executeStopwatch.Elapsed.TotalSeconds);

                _logger.LogTrace(
                    $"[{_workerSettings.WorkerId}] Done processing task for worker"
                    + $", taskType: {_worker.TaskType}"
                    + $", domain: {_workerSettings.Domain}"
                    + $", taskId: {task.TaskId}"
                    + $", workflowId: {task.WorkflowInstanceId}"
                    + $", CancelToken: {token}"
                );
                UpdateTask(taskResult);
            }
            catch (Exception e)
            {
                executeStopwatch.Stop();
                _metrics?.RecordTaskExecuteTime(_worker.TaskType, executeStopwatch.Elapsed.TotalSeconds);
                _metrics?.RecordTaskExecuteError(_worker.TaskType, e.GetType().Name);
                _logger.LogError(
                    $"[{_workerSettings.WorkerId}] Failed to process task for worker, reason: {e.Message}"
                    + $", taskType: {_worker.TaskType}"
                    + $", domain: {_workerSettings.Domain}"
                    + $", taskId: {task.TaskId}"
                    + $", workflowId: {task.WorkflowInstanceId}"
                    + $", CancelToken: {token}"
                );
                var taskResult = task.Failed(e.Message);
                UpdateTask(taskResult);
            }
            finally
            {
                if (token == CancellationToken.None)
                    token.ThrowIfCancellationRequested();
                _workflowTaskMonitor.RunningWorkerDone();
            }
        }

        private void UpdateTask(Models.TaskResult taskResult)
        {
            taskResult.WorkerId = taskResult.WorkerId ?? _workerSettings.WorkerId;
            RecordTaskResultSize(taskResult);
            var updateStopwatch = Stopwatch.StartNew();
            for (var attemptCounter = 0; attemptCounter < UPDATE_TASK_RETRY_COUNT_LIMIT; attemptCounter += 1)
            {
                try
                {
                    // Retries in increasing time intervals (0s, 2s, 4s, 8s...)
                    if (attemptCounter > 0)
                    {
                        Sleep(TimeSpan.FromSeconds(1 << attemptCounter));
                    }

                    _taskClient.UpdateTask(taskResult);
                    updateStopwatch.Stop();
                    _metrics?.RecordTaskUpdateTime(_worker.TaskType, updateStopwatch.Elapsed.TotalSeconds);
                    _logger.LogTrace(
                        $"[{_workerSettings.WorkerId}] Done updating task"
                        + $", taskType: {_worker.TaskType}"
                        + $", domain: {_workerSettings.Domain}"
                        + $", taskId: {taskResult.TaskId}"
                        + $", workflowId: {taskResult.WorkflowInstanceId}"
                    );
                    return;
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
                }
            }

            updateStopwatch.Stop();
            _metrics?.RecordTaskUpdateError(_worker.TaskType);
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

        private void Sleep(TimeSpan timeSpan)
        {
            _logger.LogDebug($"[{_workerSettings.WorkerId}] Sleeping for {timeSpan.Milliseconds}ms");
            Thread.Sleep(timeSpan);
        }

        private void LogInfo()
        {
        }
    }
}