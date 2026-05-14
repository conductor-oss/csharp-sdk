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
    public class WorkflowTaskMonitor : IWorkflowTaskMonitor, IDisposable
    {
        private readonly ILogger<WorkflowTaskMonitor> _logger;
        private readonly ReaderWriterLockSlim _mutex;
        private int _runningWorkerCounter;

        public WorkflowTaskMonitor(ILogger<WorkflowTaskMonitor> logger)
        {
            _logger = logger;
            _runningWorkerCounter = 0;
            _mutex = new ReaderWriterLockSlim();
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

        public void Dispose()
        {
            _mutex?.Dispose();
        }
    }
}
