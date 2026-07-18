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
using Conductor.Client;
using Conductor.Client.Models;
using Task = System.Threading.Tasks.Task;

namespace Conductor.AI;

/// <summary>
/// Watches a stateful run's workflow for a worker stall (spec R11). For
/// stateful runs, tasks are routed to this process's workers via a per-run
/// domain — if the worker dies, the server-side task sits with
/// <c>pollCount=0</c> forever and a blocking wait would hang indefinitely.
/// Every <c>checkIntervalSeconds</c>, fetches the workflow with its tasks; a
/// <c>SCHEDULED</c>/<c>IN_PROGRESS</c> task that has had zero polls for at
/// least <c>stallSeconds</c> flags a stall. Stops on its own once the
/// workflow reaches a terminal state, or when disposed.
/// </summary>
internal sealed class ServerLivenessMonitor : IAsyncDisposable
{
    private readonly WorkflowResourceApi _workflowClient;
    private readonly string _executionId;
    private readonly double _stallSeconds;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _loop;
    private volatile string? _stalledTaskRef;

    internal ServerLivenessMonitor(
        Configuration configuration, string executionId, double stallSeconds, double checkIntervalSeconds)
    {
        _workflowClient = new WorkflowResourceApi(configuration);
        _executionId = executionId;
        _stallSeconds = stallSeconds;
        _loop = Task.Run(() => RunAsync(TimeSpan.FromSeconds(checkIntervalSeconds), _cts.Token));
    }

    /// <summary>The stalled task's reference name, or null if no stall has been observed yet.</summary>
    public string? StalledTaskRef => _stalledTaskRef;

    private async Task RunAsync(TimeSpan checkInterval, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try { await Task.Delay(checkInterval, ct); }
            catch (OperationCanceledException) { return; }

            Workflow? wf;
            try { wf = await _workflowClient.GetExecutionStatusAsync(_executionId, includeTasks: true); }
            catch { continue; } // transient — try again next tick

            if (wf?.Status is Workflow.StatusEnum.COMPLETED or Workflow.StatusEnum.FAILED
                or Workflow.StatusEnum.TIMEDOUT or Workflow.StatusEnum.TERMINATED)
                return; // terminal — nothing left to watch

            var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            foreach (var task in wf?.Tasks ?? new List<Conductor.Client.Models.Task>())
            {
                var isPending = task.Status is Conductor.Client.Models.Task.StatusEnum.SCHEDULED
                    or Conductor.Client.Models.Task.StatusEnum.INPROGRESS;
                if (!isPending || (task.PollCount ?? 0) > 0) continue;

                var scheduledAtMs = task.ScheduledTime ?? task.StartTime ?? 0;
                if (scheduledAtMs <= 0) continue;

                var ageSeconds = (nowMs - scheduledAtMs) / 1000.0;
                if (ageSeconds >= _stallSeconds)
                {
                    _stalledTaskRef = task.ReferenceTaskName ?? task.TaskDefName ?? "unknown";
                    return; // stall recorded — the flag is sticky, stop polling
                }
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        try { await _loop; }
        catch (OperationCanceledException) { }
        catch { /* the loop only ever logs-and-continues on its own errors */ }
        _cts.Dispose();
    }
}
