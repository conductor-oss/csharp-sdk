# Debugging

Diagnosing a specific execution. For aggregate metrics see
[observability.md](observability.md).

## Start with the execution

Every execution has a durable id and a UI page:

```
http://localhost:8080/execution/<workflow-id>
```

The task list there shows which task is current, its status, and its input and output.
Most questions are answered before writing any code.

Programmatically:

```csharp
var workflowClient = configuration.GetClient<WorkflowResourceApi>();
var execution = workflowClient.GetExecutionStatus(workflowId);
```

For agents, `GetWorkflowAsync` returns the raw workflow **including task inputs**, where
`GetExecutionAsync` returns a summary that omits them. When a tool received the wrong
arguments, the raw form is the one that shows it.

## Tasks stuck in SCHEDULED

By far the most common symptom, and it is not an error state — the server is waiting for
a worker to claim the task. Check, in order:

1. **Task type mismatch.** `IWorkflowTask.TaskType` must exactly match the task type in
   the workflow definition. A typo produces exactly this symptom, silently.
2. **Worker host not started.** `await host.StartAsync()` — easy to omit in a sample.
3. **Worker process died.** Check the process; poll loops stop with it.
4. **Wrong server.** A worker pointed at a different `CONDUCTOR_SERVER_URL` than the one
   the execution is on polls an empty queue forever.
5. **Stateful agent runs.** Tasks are pinned to the starting process's domain, so no other
   worker can pick them up. See [reliability.md](reliability.md#liveness-for-stateful-agent-runs).

`TaskResourceApi` exposes queue sizes, which distinguishes "nothing is being scheduled"
from "things are queued and nobody is polling".

## Authentication failures

A 401 or 403 surfaces as `ApiException`. Check:

- `CONDUCTOR_AUTH_KEY` and `CONDUCTOR_AUTH_SECRET` are **both** set — one alone puts the
  client in no-auth mode.
- The key has permission for the resource. RBAC failures are 403, not 401.
- You are reusing one `Configuration`. Many `Configuration` objects means many token
  exchanges, which can hit rate limits.

The startup check in
[server-setup.md](server-setup.md#verifying-the-connection) surfaces these at boot rather
than inside a poll loop.

## Agent runs that never finish

| Symptom | Likely cause |
|---|---|
| Stops at a `Waiting` event | A HITL pause nobody answered. Under multi-agent strategies, approve the **event's** execution, not the root — see [agents/concepts/streaming-hitl.md](agents/concepts/streaming-hitl.md#event-targeted-hitl). |
| `WorkerStallException` | The worker owning a stateful run's domain died. |
| Returns immediately with no output | A `TextGate` matched, or a `Termination` condition fired. Check `FinishReason`. |
| `CredentialNotFoundException` | A declared credential was not delivered. Fail-closed by design — see [security.md](security.md#local-tool-credentials-are-fail-closed). |
| Tool never called | The LLM did not choose it. Check the tool description, and consider `RequiredTools`. |

`AgentResult.FinishReason` distinguishes these: `Stop`, `Length`, `ToolCalls`, `Error`,
`Cancelled`, `Timeout`, `Guardrail`, `Rejected`.

## Inspecting what an agent compiles to

An agent becomes a Conductor `WorkflowDef`. Dry-run the compile without registering or
executing:

```csharp
var workflowDef = await runtime.PlanAsync(agent);   // JsonNode?
Console.WriteLine(workflowDef?.ToJsonString());
```

This is the ground truth for "why is my agent shaped like that" questions, and the
authority when [agents/reference/agent-schema.md](agents/reference/agent-schema.md)
disagrees with reality.

## Streaming as a debugging tool

Streaming shows the decision sequence — `Thinking`, `ToolCall`, `ToolResult`,
`GuardrailPass`/`GuardrailFail`, `Handoff` — rather than just the final answer. For
diagnosing why an agent did something, it is usually faster than reading the execution
history.

```csharp
await foreach (var ev in runtime.StreamAsync(agent, prompt))
    Console.WriteLine($"{ev.Type}: {ev.Content ?? ev.ToolName}");
```

## Logging

The worker host takes a `LogLevel`:

```csharp
var host = WorkflowTaskHost.CreateWorkerHost(
    Microsoft.Extensions.Logging.LogLevel.Debug, new GreetWorker());
```

`ApplicationLogging` in `Conductor.Client.Extensions` is the logging entry point;
`Tracing` covers the agent layer.

## Reproducing in a test

`Tests/Integration/` runs against a live server, and the agent E2E suites in
`Conductor.AI.E2eTests/` are organised by feature — often the fastest way to reproduce a
suspected SDK bug is to extend the matching suite. See
[workflow-testing.md](workflow-testing.md).
