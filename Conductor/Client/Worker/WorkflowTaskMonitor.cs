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
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace Conductor.Client.Interfaces
{
    public class WorkflowTaskMonitor : IWorkflowTaskMonitor
    {
        private readonly ILogger<WorkflowTaskMonitor> _logger;
        private readonly ReaderWriterLockSlim _mutex;
        private int _runningWorkerCounter;
        private int _consecutivePollErrors;
        private int _totalTasksProcessed;
        private int _totalTaskErrors;
        private int _totalPollErrors;
        private DateTime? _lastPollTime;
        private DateTime? _lastTaskCompletedTime;
        private DateTime? _lastErrorTime;
        private int _maxConsecutiveErrors = 10;

        public WorkflowTaskMonitor(ILogger<WorkflowTaskMonitor> logger, int maxConsecutiveErrors = 10)
        {
            _logger = logger;
            _runningWorkerCounter = 0;
            _mutex = new ReaderWriterLockSlim();
            _maxConsecutiveErrors = maxConsecutiveErrors;
        }

        public void IncrementRunningWorker()
        {
            _mutex.EnterWriteLock();
            try
            {
                _runningWorkerCounter++;
                _logger.LogTrace($"Updated runningWorkerCounter from {_runningWorkerCounter - 1} to {_runningWorkerCounter}");
            }
            finally
            {
                _mutex.ExitWriteLock();
            }
        }

        public int GetRunningWorkers()
        {
            _mutex.EnterReadLock();
            try
            {
                return _runningWorkerCounter;
            }
            finally
            {
                _mutex.ExitReadLock();
            }
        }

        public void RunningWorkerDone()
        {
            _mutex.EnterWriteLock();
            try
            {
                _runningWorkerCounter--;
                _logger.LogTrace($"Updated runningWorkerCounter from {_runningWorkerCounter + 1} to {_runningWorkerCounter}");
            }
            finally
            {
                _mutex.ExitWriteLock();
            }
        }

        public void RecordPollSuccess(int taskCount)
        {
            _mutex.EnterWriteLock();
            try
            {
                _consecutivePollErrors = 0;
                _lastPollTime = DateTime.UtcNow;
            }
            finally
            {
                _mutex.ExitWriteLock();
            }
        }

        public void RecordPollError()
        {
            _mutex.EnterWriteLock();
            try
            {
                _consecutivePollErrors++;
                _totalPollErrors++;
                _lastErrorTime = DateTime.UtcNow;
            }
            finally
            {
                _mutex.ExitWriteLock();
            }
        }

        public void RecordTaskSuccess()
        {
            _mutex.EnterWriteLock();
            try
            {
                _totalTasksProcessed++;
                _lastTaskCompletedTime = DateTime.UtcNow;
            }
            finally
            {
                _mutex.ExitWriteLock();
            }
        }

        public void RecordTaskError()
        {
            _mutex.EnterWriteLock();
            try
            {
                _totalTasksProcessed++;
                _totalTaskErrors++;
                _lastErrorTime = DateTime.UtcNow;
            }
            finally
            {
                _mutex.ExitWriteLock();
            }
        }

        public bool IsHealthy()
        {
            _mutex.EnterReadLock();
            try
            {
                return _consecutivePollErrors < _maxConsecutiveErrors;
            }
            finally
            {
                _mutex.ExitReadLock();
            }
        }

        public WorkerHealthStatus GetHealthStatus()
        {
            _mutex.EnterReadLock();
            try
            {
                return new WorkerHealthStatus
                {
                    IsHealthy = _consecutivePollErrors < _maxConsecutiveErrors,
                    RunningWorkers = _runningWorkerCounter,
                    ConsecutivePollErrors = _consecutivePollErrors,
                    TotalTasksProcessed = _totalTasksProcessed,
                    TotalTaskErrors = _totalTaskErrors,
                    TotalPollErrors = _totalPollErrors,
                    LastPollTime = _lastPollTime,
                    LastTaskCompletedTime = _lastTaskCompletedTime,
                    LastErrorTime = _lastErrorTime
                };
            }
            finally
            {
                _mutex.ExitReadLock();
            }
        }
    }
}
