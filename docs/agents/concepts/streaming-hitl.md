# Streaming and human-in-the-loop

Streaming and HITL share one mechanism: the event stream. A run that needs a human
emits a `Waiting` event and pauses until you respond.

## Streaming

`StartAsync` returns an `AgentHandle`; iterate its `StreamAsync()`, or use
`runtime.StreamAsync(agent, prompt)` directly:

```csharp
await using var runtime = new AgentRuntime();

await foreach (var ev in runtime.StreamAsync(agent, "Write a haiku about C#."))
{
    switch (ev.Type)
    {
        case EventType.Thinking:    Console.WriteLine($"[thinking] {ev.Content}"); break;
        case EventType.ToolCall:    Console.WriteLine($"[tool_call] {ev.ToolName}({ev.Args})"); break;
        case EventType.ToolResult:  Console.WriteLine($"[tool_result] {ev.ToolName} -> {ev.Result}"); break;
        case EventType.Handoff:     Console.WriteLine($"[handoff] -> {ev.Target}"); break;
        case EventType.Waiting:     Console.WriteLine("[waiting...]"); break;
        case EventType.Done:        Console.WriteLine($"Done: {ev.Content} ({ev.Status})"); break;
        case EventType.Error:       Console.WriteLine($"[error] {ev.Content}"); break;
    }
}
```

Event types: `Thinking`, `ToolCall`, `ToolResult`, `GuardrailPass`,
`GuardrailFail`, `Waiting`, `Handoff`, `Message`, `Error`, `Done`.

### Events on a waited result

`AgentResult.Events` carries the run's tool activity — a `ToolCall`/`ToolResult` pair
per tool call, closed by a terminal `Done`, or `Error` for a run that did not
complete. It is reconstructed from the finished execution's tasks, so it is never
null but is narrower than the live stream: the server also emits `Thinking`,
`Handoff`, guardrail and per-failed-task events, and none of those survives into the
terminal record. Stream the run when the events themselves are the point.

Streaming attempts SSE first and falls back to status-polling. Disable SSE entirely
with `CONDUCTOR_AGENT_STREAMING_ENABLED=false` — see
[deploy-serve-run.md](deploy-serve-run.md#worker-tuning-and-agentconfig). If the
server rejects the SSE connection the SDK raises `SSEUnavailableException`.

## Human-in-the-loop

When a tool has `ApprovalRequired = true` (or the agent calls `HumanTool`), the
execution emits a `Waiting` event and pauses. Respond via the handle.

```csharp
var handle = await runtime.StartAsync(agent, prompt);

await foreach (var ev in handle.StreamAsync())
{
    if (ev.Type == EventType.Waiting)
    {
        await handle.ApproveAsync();                 // approve
        // await handle.ApproveAsync("looks good");  // approve with a comment
        // await handle.RejectAsync("not authorized");
    }
}
```

For a `HumanTool` question, read the pending tool args and send a structured reply:

```csharp
case EventType.Waiting:
    var status = await handle.GetStatusAsync();
    var pending = status.PendingTool ?? new();
    // ...read pending["args"] for the question...
    await handle.RespondAsync(new { answer = Console.ReadLine() });
    break;
```

### Event-targeted HITL

Under multi-agent strategies the HUMAN task can live in a sub-execution, so respond
to the *event's* execution, not the root. Pass the `Waiting` event itself:

```csharp
await handle.ApproveAsync(ev);                 // targets ev.ExecutionId
await handle.RejectAsync(ev, "reason");
await handle.RespondAsync(ev, new { answer = "..." });
// the same overloads exist on runtime: runtime.ApproveAsync(ev), runtime.RejectAsync(ev, reason)
```

This is the most common HITL mistake — approving the root execution when the pause
lives in a sub-execution leaves the run waiting forever.

### Polling instead of streaming

Wait for the pause without a stream:

```csharp
if (await handle.WaitUntilWaitingAsync(TimeSpan.FromSeconds(30)))
    await handle.ApproveAsync();
// also: await handle.IsWaitingAsync()
```

## Stopping a run

```csharp
await handle.StopAsync();           // graceful: finishes the current step, COMPLETED
await handle.CancelAsync("reason"); // immediate: TERMINATED
```

## Reference

`AgentHandle`, `AgentEvent`, and `AgentStatus` members are tabulated in
[reference/api.md](../reference/api.md#results).
