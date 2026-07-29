# Stateful agents

Set `Stateful = true` to pin every worker task for an execution to one worker
process (domain-based routing).

## Why it exists

This is required when a `WaitForMessageTool` runs alongside local tools, so the
worker that waits for messages is the same one that receives them. Without it, a
message can be delivered to a process that isn't the one blocked on the wait.

## Driving a stateful agent

Use `StartAsync` + `SendMessageAsync`:

```csharp
var receive = WaitForMessageTool.Create(name: "wait_for_message",
    description: "Wait for the next external message, then return its content.");

var agent = new Agent("listener")
{
    Model    = "anthropic/claude-sonnet-4-6",
    Stateful = true,
    MaxTurns = 10_000,
    Tools    = [receive, .. ToolRegistry.FromInstance(new ActionTools())],
    Instructions = "Loop: wait_for_message, act on it, repeat until told to stop.",
};

await using var runtime = new AgentRuntime();
var handle = await runtime.StartAsync(agent, "Start listening.");

await runtime.SendMessageAsync(handle.ExecutionId, new { action = "generate-report" });
// ...
await handle.StopAsync();
var result = await handle.WaitAsync();
```

`WaitForMessageTool` requires `conductor.workflow-message-queue.enabled=true` on
the server. A per-tool `[Tool(Stateful = true)]` flag (or `ToolDef.Stateful`) also
marks the parent agent stateful.

## Surviving process restarts

Reattach to a durable execution across process restarts:

```csharp
var handle = await runtime.ResumeAsync(executionId, agent);
```

`ResumeAsync` re-registers the local workers as well as reattaching, which is what
makes the run continue rather than merely being observable.

## Liveness

Stateful runs route their tool tasks to this process's own worker via a per-run
domain. If the process that started the run stops polling — crash, restart, network
partition — the task sits unpolled forever and a blocking `WaitAsync` would hang.

`AgentRuntime` therefore attaches a background liveness monitor to every stateful
run (unless `CONDUCTOR_AGENT_LIVENESS_ENABLED=false`). It polls the workflow's task
list every `LivenessCheckIntervalSeconds` and flags a stall once a
`SCHEDULED`/`IN_PROGRESS` task has gone unpolled past `LivenessStallSeconds`.

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

`WaitAsync` is itself bounded — a 10-minute overall deadline, and it tolerates up to
3 consecutive transient `GetStatus` errors before giving up — so a stateful run can
never hang forever even without a stall being flagged. The monitor is disposed
automatically when the handle's wait completes.

Tuning knobs are in
[deploy-serve-run.md](deploy-serve-run.md#worker-tuning-and-agentconfig); the
reliability rationale is in [../../reliability.md](../../reliability.md).
