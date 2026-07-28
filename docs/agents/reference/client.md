# Reference: IAgentClient

`interface IAgentClient : IDisposable, IAsyncDisposable`, implemented by
`sealed class OrkesAgentClient`.

Obtain one via `OrkesApiClient.GetAgentClient()` or `Configuration.GetAgentClient()`
— both share that `Configuration`'s token cache, so there is no separate token client
— or use the runtime's own instance via `runtime.Client`.

## Control-plane convenience

- `RunAsync(Agent, ...)`
- `StartAsync(Agent, ...)`
- `DeployAsync(params Agent[])`
- `ScheduleAsync(Agent, IEnumerable<Schedule>, ct)`
- `Schedules` (property)

**Run is control-plane only** — no local tool workers are registered or polled. Agents
with local `[Tool]` methods must run through `AgentRuntime`.

## Lower level

| Member | Notes |
|---|---|
| `StartAsync(JsonObject)` | Raw start. |
| `DeployAsync(JsonObject)` | Raw deploy. |
| `CompileAsync(JsonObject)` | Compile without registering. |
| `GetStatusAsync` | Execution status. |
| `GetExecutionAsync` | Summary view; omits task inputs. |
| `GetWorkflowAsync` | Raw workflow, including task inputs. |
| `ListExecutionsAsync` | |
| `RespondAsync` | HITL response. |
| `SignalAsync` | Delivers `{"message": ...}` to the execution. |
| `PauseAgentAsync` / `UnpauseAgentAsync` | |
| `StreamEventsAsync` | SSE event stream. |
| `StartWorkflowByNameAsync` | Pre-deployed agents. |
| `SendWorkflowMessageAsync` | Workflow Message Queue. |
| `StopAgentAsync` | Graceful stop. |
| `CancelAgentAsync` | Immediate termination. |

There is no client-side credential resolution method. Credential delivery is entirely
server-driven per execution via the `runtimeMetadata` wire contract — see
[../concepts/tools.md](../concepts/tools.md#credentials) and
[../../security.md](../../security.md).

## Standalone use

```csharp
var configuration = new Configuration { BasePath = "http://localhost:8080/api" };
using var standalone = configuration.GetAgentClient();
var handle = await standalone.StartAsync(agent, "Hello");
```

See [../concepts/deploy-serve-run.md](../concepts/deploy-serve-run.md#the-iagentclient-control-plane).
