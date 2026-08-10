# Reliability

Conductor's durability model puts state on the server. Understanding which failures the
server absorbs — and which it does not — is most of operating this SDK well.

## The division of responsibility

| Concern | Owner |
|---|---|
| Retries | Server, per task definition |
| Timeouts | Server, per task and workflow definition |
| Task state and history | Server |
| Scheduling and rescheduling | Server |
| Executing the task body | Your worker |
| Idempotency of side effects | Your worker |

A worker that implements its own retry loop is duplicating the server's job and hiding
failures from the execution history.

## Worker idempotency

The server may reschedule a task whose worker died mid-execution. It cannot know how far
the worker got. So a worker must tolerate re-execution:

- Make writes idempotent — upsert rather than insert, key by a stable id.
- Derive that id from task input (or `CorrelationId`), not from a fresh GUID.
- Treat "already done" as success rather than an error.

## Retries for tools

Agent tools carry their own retry configuration:

| Setting | Default |
|---|---|
| `RetryCount` | 2 |
| `RetryDelaySeconds` | 2 |
| `RetryPolicy` | `fixed`, `linear_backoff`, `exponential_backoff` |

```csharp
[Tool("Call a flaky upstream API.", RetryCount = 5, RetryDelaySeconds = 3,
      RetryPolicy = "exponential_backoff")]
public Dictionary<string, object> CallUpstream(string id) => /* ... */;
```

`TerminalToolException` signals an unrecoverable failure — it stops the retry cycle
rather than burning through the remaining attempts on an error that will never succeed.

## Timeouts

Timeouts are properties of the definition, enforced server-side. A worker that stops
responding does not stop the clock; the task times out and is handled per the definition.

`TimeoutSeconds` on a tool bounds that tool's execution. `Agent.TimeoutSeconds` bounds
the agent.

## The unpolled-task failure mode

The most common way a Conductor system appears to hang is not a crash. A task sits
`SCHEDULED` because **no worker is polling for its task type** — wrong `TaskType`
string, worker host not started, or the worker process died. The server waits
indefinitely; nothing errors.

For stateful agent runs this is worse, because tasks are pinned to one process's domain,
so no other worker can pick them up. That is why the agent runtime ships a liveness
monitor.

## Liveness for stateful agent runs

`AgentRuntime` attaches a background monitor to every stateful run unless
`CONDUCTOR_AGENT_LIVENESS_ENABLED=false`. It polls the workflow's task list every
`LivenessCheckIntervalSeconds` and flags a stall once a `SCHEDULED`/`IN_PROGRESS` task has
gone unpolled past `LivenessStallSeconds`.

```csharp
try
{
    var result = await handle.WaitAsync();
}
catch (WorkerStallException ex)
{
    // ex.TaskReferenceName / ex.ExecutionId — the worker handling this run's
    // domain may have died; the task itself is still SCHEDULED on the server.
}
```

`WaitAsync` is additionally bounded — a 10-minute overall deadline, tolerating up to 3
consecutive transient `GetStatus` errors — so a stateful run can never hang forever even
without a stall being flagged.

See [agents/concepts/stateful.md](agents/concepts/stateful.md).

## Surviving process restarts

An execution is durable server-side, so a worker restart does not lose it.
`runtime.ResumeAsync(executionId, agent)` reattaches *and* re-registers the local
workers, which is what makes the run continue rather than merely be observable.

## Streaming degradation

`StreamAsync` attempts SSE and falls back to status-polling, so a proxy that blocks SSE
degrades rather than fails. If the server actively rejects the connection you get
`SSEUnavailableException`.

## Intervening after the fact

`WorkflowBulkResourceApi` retries, restarts, or terminates many executions at once — the
tool for recovering after a bad deploy. See
[workflow-lifecycle.md](workflow-lifecycle.md#intervening).
