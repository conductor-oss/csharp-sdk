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
using Conductor.Api;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Harness
{
    /// <summary>
    /// Opt-in control-plane probe that exercises UUID-bearing workflow lookup
    /// endpoints so <c>http_api_client_request_seconds</c> picks up entries
    /// with <c>uri=/workflow/{workflowId}</c>. Default harness traffic only
    /// hits bounded, no-path-param URLs, making the high-cardinality concern
    /// on the <c>uri</c> label invisible without this probe.
    ///
    /// <para><b>Default off.</b> Runs only when HARNESS_PROBE_RATE_PER_SEC > 0.</para>
    /// <para><b>Side-effect-free.</b> Only issues read calls (GetExecutionStatus).</para>
    /// <para><b>Self-bounded.</b> Fixed-size FIFO of workflow IDs.</para>
    /// </summary>
    public sealed class WorkflowStatusProbe : IDisposable
    {
        private const int MaxTrackedIds = 256;

        private readonly WorkflowResourceApi _workflowClient;
        private readonly int _callsPerSecond;
        private readonly ILogger<WorkflowStatusProbe> _logger;
        private readonly ConcurrentQueue<string> _recentIds = new();
        private readonly Timer _timer;
        private readonly Random _random = new();
        private int _idCount;

        public WorkflowStatusProbe(
            WorkflowResourceApi workflowClient,
            int callsPerSecond,
            ILogger<WorkflowStatusProbe> logger)
        {
            _workflowClient = workflowClient;
            _callsPerSecond = callsPerSecond;
            _logger = logger;

            if (_callsPerSecond > 0)
            {
                _timer = new Timer(_ => Tick(), null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
                _logger.LogInformation(
                    "WorkflowStatusProbe started: rate={Rate}/sec, retainedIds<={Max}",
                    _callsPerSecond, MaxTrackedIds);
            }
            else
            {
                _logger.LogInformation("WorkflowStatusProbe disabled (HARNESS_PROBE_RATE_PER_SEC<=0)");
            }
        }

        /// <summary>
        /// Capture a workflow ID for later probing. Thread-safe; intended to
        /// be wired as the idSink callback of WorkflowGovernor.
        /// </summary>
        public void Offer(string workflowId)
        {
            if (string.IsNullOrWhiteSpace(workflowId))
                return;

            _recentIds.Enqueue(workflowId);
            while (Interlocked.Increment(ref _idCount) > MaxTrackedIds)
            {
                if (_recentIds.TryDequeue(out _))
                    Interlocked.Decrement(ref _idCount);
                else
                    break;
            }
            // Correct for the increment above if we didn't actually exceed
            if (Volatile.Read(ref _idCount) <= MaxTrackedIds)
                return;
        }

        private void Tick()
        {
            int available = _recentIds.Count;
            int budget = Math.Min(_callsPerSecond, available);

            for (int i = 0; i < budget; i++)
            {
                if (!_recentIds.TryDequeue(out var id))
                    return;

                _recentIds.Enqueue(id);

                try
                {
                    if (_random.Next(2) == 0)
                        _workflowClient.GetExecutionStatus(id, includeTasks: false);
                    else
                        _workflowClient.GetExecutionStatus(id, includeTasks: false, summarize: true);
                }
                catch (Exception e)
                {
                    _logger.LogDebug("Probe: lookup failed for {Id}: {Error}", id, e.Message);
                }
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
