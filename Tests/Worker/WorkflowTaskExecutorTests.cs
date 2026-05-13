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
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conductor.Client.Interfaces;
using Conductor.Client.Models;
using Conductor.Client.Telemetry;
using Conductor.Client.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Tests.Worker
{
    public class WorkflowTaskExecutorTests : IDisposable
    {
        private readonly MetricsCollector _metrics = new();
        private readonly MeterListener _listener = new();
        private readonly List<RecordedMeasurement> _recorded = new();

        public WorkflowTaskExecutorTests()
        {
            _listener.InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == MetricsCollector.MeterName)
                    listener.EnableMeasurementEvents(instrument);
            };

            _listener.SetMeasurementEventCallback<long>((instrument, value, tags, _) =>
                _recorded.Add(new RecordedMeasurement(instrument.Name, value, tags.ToArray())));
            _listener.SetMeasurementEventCallback<double>((instrument, value, tags, _) =>
                _recorded.Add(new RecordedMeasurement(instrument.Name, value, tags.ToArray())));
            _listener.SetMeasurementEventCallback<int>((instrument, value, tags, _) =>
                _recorded.Add(new RecordedMeasurement(instrument.Name, value, tags.ToArray())));

            _listener.Start();
        }

        public void Dispose()
        {
            _metrics.Dispose();
            _listener.Dispose();
        }

        [Fact]
        public async Task SuccessfulPoll_RecordsPollMetrics()
        {
            var taskClient = new FakeTaskClient(returnTasks: new List<Conductor.Client.Models.Task>());
            var executor = CreateExecutor(taskClient);

            await RunOnceAndWait(executor);

            Assert.Contains(_recorded, r => r.Name == "task_poll_total");
            var pollTime = _recorded.FirstOrDefault(r => r.Name == "task_poll_time_seconds");
            Assert.NotNull(pollTime);
            AssertTag(pollTime, "status", "SUCCESS");
        }

        [Fact]
        public async Task FailedPoll_RecordsPollErrorMetrics()
        {
            var taskClient = new FakeTaskClient(pollException: new Exception("connection refused"));
            var executor = CreateExecutor(taskClient);

            await RunOnceAndWait(executor);

            Assert.Contains(_recorded, r => r.Name == "task_poll_error_total");
            var pollError = _recorded.First(r => r.Name == "task_poll_error_total");
            AssertTag(pollError, "exception", "Exception");

            var pollTime = _recorded.FirstOrDefault(r => r.Name == "task_poll_time_seconds");
            Assert.NotNull(pollTime);
            AssertTag(pollTime, "status", "FAILURE");
        }

        [Fact]
        public async Task QueueFull_RecordsQueueFullMetric()
        {
            var taskClient = new FakeTaskClient(returnTasks: new List<Conductor.Client.Models.Task>());
            var monitor = new WorkflowTaskMonitor(NullLogger<WorkflowTaskMonitor>.Instance);
            // Fill up the monitor to capacity (batchSize=1 so 1 running = full)
            monitor.IncrementRunningWorker();
            var worker = new FakeWorker("test_task", batchSize: 1);
            var executor = new WorkflowTaskExecutor(
                NullLogger<WorkflowTaskExecutor>.Instance,
                taskClient, worker, monitor, _metrics);

            await RunOnceAndWait(executor);

            Assert.Contains(_recorded, r => r.Name == "task_execution_queue_full_total");
            var metric = _recorded.First(r => r.Name == "task_execution_queue_full_total");
            AssertTag(metric, "taskType", "test_task");
            monitor.Dispose();
        }

        [Fact]
        public async Task CancellationToken_BreaksWorkLoop()
        {
            var taskClient = new FakeTaskClient(returnTasks: new List<Conductor.Client.Models.Task>());
            var executor = CreateExecutor(taskClient);

            using var cts = new CancellationTokenSource();
            var runTask = executor.Start(cts.Token);

            // Give it a moment to start, then cancel
            await Task.Delay(100);
            cts.Cancel();

            // The task should complete (not hang forever)
            var completed = await Task.WhenAny(runTask, Task.Delay(5000));
            Assert.Equal(runTask, completed);
        }

        private WorkflowTaskExecutor CreateExecutor(
            IWorkflowTaskClient taskClient,
            string taskType = "test_task",
            int batchSize = 10)
        {
            var worker = new FakeWorker(taskType, batchSize);
            var monitor = new WorkflowTaskMonitor(NullLogger<WorkflowTaskMonitor>.Instance);
            return new WorkflowTaskExecutor(
                NullLogger<WorkflowTaskExecutor>.Instance,
                taskClient, worker, monitor, _metrics);
        }

        private static async Task RunOnceAndWait(WorkflowTaskExecutor executor)
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            try
            {
                await executor.Start(cts.Token);
            }
            catch (OperationCanceledException) { }
        }

        private static void AssertTag(RecordedMeasurement measurement, string key, string expectedValue)
        {
            var tag = measurement.Tags.FirstOrDefault(t => t.Key == key);
            Assert.Equal(expectedValue, (string)tag.Value);
        }

        private record RecordedMeasurement(
            string Name,
            object Value,
            KeyValuePair<string, object>[] Tags);

        private class FakeTaskClient : IWorkflowTaskClient
        {
            private readonly List<Conductor.Client.Models.Task> _tasks;
            private readonly Exception _pollException;
            private readonly Exception _updateException;

            public int PollCount { get; private set; }
            public int UpdateCount { get; private set; }

            public FakeTaskClient(
                List<Conductor.Client.Models.Task> returnTasks = null,
                Exception pollException = null,
                Exception updateException = null)
            {
                _tasks = returnTasks;
                _pollException = pollException;
                _updateException = updateException;
            }

            public List<Conductor.Client.Models.Task> PollTask(string taskType, string workerId, string domain, int count)
            {
                PollCount++;
                if (_pollException != null) throw _pollException;
                return _tasks ?? new List<Conductor.Client.Models.Task>();
            }

            public string UpdateTask(TaskResult result)
            {
                UpdateCount++;
                if (_updateException != null) throw _updateException;
                return result.TaskId;
            }

            public Task<List<Conductor.Client.Models.Task>> PollTaskAsync(string taskType, string workerId, string domain, int count)
            {
                return Task.FromResult(PollTask(taskType, workerId, domain, count));
            }

            public Task<string> UpdateTaskAsync(TaskResult result)
            {
                return Task.FromResult(UpdateTask(result));
            }
        }

        private class FakeWorker : IWorkflowTask
        {
            public string TaskType { get; }
            public WorkflowTaskExecutorConfiguration WorkerSettings { get; }

            public FakeWorker(string taskType, int batchSize = 10)
            {
                TaskType = taskType;
                WorkerSettings = new WorkflowTaskExecutorConfiguration
                {
                    BatchSize = batchSize,
                    PollInterval = TimeSpan.FromMilliseconds(50),
                    Domain = "test",
                    WorkerId = "test-worker-1"
                };
            }

            public TaskResult Execute(Conductor.Client.Models.Task task)
            {
                return new TaskResult(taskId: task.TaskId, workflowInstanceId: task.WorkflowInstanceId)
                {
                    Status = TaskResult.StatusEnum.COMPLETED
                };
            }

            public Task<TaskResult> Execute(Conductor.Client.Models.Task task, CancellationToken token)
            {
                return Task.FromResult(Execute(task));
            }
        }
    }
}
