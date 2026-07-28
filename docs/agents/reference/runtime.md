# Reference: AgentRuntime

`sealed class AgentRuntime : IAsyncDisposable, IDisposable`. Main entry point.

Constructors:

- `AgentRuntime(Configuration? configuration = null, AgentConfig? settings = null)` —
  primary. `configuration` resolves from the `CONDUCTOR_*` env chain when omitted;
  `settings` defaults to `AgentConfig.FromEnv()`.
- `AgentRuntime(AgentRuntimeOptions options)` — sugar over the above.

| Member | Signature | Notes |
|---|---|---|
| `Client` | `IAgentClient Client { get; }` | The control-plane client (`OrkesAgentClient`). |
| `Schedules` | `Schedules Schedules { get; }` | Cron lifecycle. |
| `WorkerThreadCount` | `int { get; }` | From `AgentConfig.WorkerThreadCount` / `CONDUCTOR_AGENT_WORKER_THREADS`. |
| `WorkerPollIntervalMs` | `int { get; }` | From `AgentConfig.WorkerPollIntervalMs` / `CONDUCTOR_AGENT_WORKER_POLL_INTERVAL`. |
| `RunAsync` | `Task<AgentResult> RunAsync(Agent agent, string prompt, string? sessionId = null, IEnumerable<string>? media = null, Plan? plan = null, RunSettings? runSettings = null, CancellationToken ct = default)` | Run + host workers + wait. |
| `Run` | `AgentResult Run(Agent, string, ...)` | Sync wrapper. |
| `StartAsync` | `Task<AgentHandle> StartAsync(Agent, string, string? = null, IEnumerable<string>? = null, Plan? = null, RunSettings? = null, CancellationToken = default)` | Start, return handle. |
| `Start` | `AgentHandle Start(Agent, string, ...)` | Sync wrapper. |
| `RunByNameAsync` / `StartByNameAsync` | by workflow name | Pre-deployed agents. |
| `StreamAsync` | `IAsyncEnumerable<AgentEvent> StreamAsync(Agent, string, string? = null, IEnumerable<string>? = null, RunSettings? = null, CancellationToken = default)` | Run + stream events. |
| `DeployAsync` | `Task<DeploymentInfo[]> DeployAsync(params Agent[])` ; `Task<DeploymentInfo> DeployAsync(Agent, IEnumerable<Schedule>?)` | Register without executing; second form reconciles schedules. |
| `Deploy` | `DeploymentInfo[] Deploy(params Agent[])` | Sync. |
| `ServeAsync` | `Task ServeAsync(Agent, CancellationToken = default)` ; `Task ServeAsync(CancellationToken = default, params Agent[])` ; `Task ServeAsync(bool blocking, CancellationToken ct = default, params Agent[])` | Host workers; blocks until cancelled unless `blocking: false`. |
| `PlanAsync` / `Plan` | `Task<JsonNode?> PlanAsync(Agent, CancellationToken = default)` | Dry-run compile. |
| `ResumeAsync` / `Resume` | `Task<AgentHandle> ResumeAsync(string executionId, Agent, CancellationToken = default)` | Reattach + re-register workers across restarts. |
| `SendMessageAsync` | `Task SendMessageAsync(string executionId, object message, CancellationToken = default)` | Push to the Workflow Message Queue. |
| `GetStatusAsync` | `Task<AgentStatus> GetStatusAsync(string executionId, CancellationToken = default)` | |
| `RespondAsync` | `Task RespondAsync(string executionId, object response, CancellationToken = default)` | HITL response by id. |
| `ApproveAsync` / `RejectAsync` / `RespondAsync` (event) | `(AgentEvent waitingEvent, ...)` | Event-targeted HITL — targets the event's execution. |

## AgentRuntimeOptions

`sealed class AgentRuntimeOptions` — `string? ServerUrl`, `string? AuthKey`,
`string? AuthSecret`. Any unset value falls back to the corresponding env var.

## AgentConfig

Construction-time knobs with lenient env parsing — invalid or empty values fall back
to the default rather than throwing. Obtain via `AgentConfig.FromEnv()` or set
properties directly.

| Property | Env var | Default |
|---|---|---|
| `WorkerThreadCount` | `CONDUCTOR_AGENT_WORKER_THREADS` | `1` |
| `WorkerPollIntervalMs` | `CONDUCTOR_AGENT_WORKER_POLL_INTERVAL` | `100` |
| `AutoStartWorkers` | `CONDUCTOR_AGENT_AUTO_START_WORKERS` | `true` |
| `DaemonWorkers` | `CONDUCTOR_AGENT_DAEMON_WORKERS` | `true` |
| `StreamingEnabled` | `CONDUCTOR_AGENT_STREAMING_ENABLED` | `true` |
| `LivenessEnabled` | `CONDUCTOR_AGENT_LIVENESS_ENABLED` | `true` |
| `LivenessStallSeconds` | `CONDUCTOR_AGENT_LIVENESS_STALL_SECONDS` | `30.0` |
| `LivenessCheckIntervalSeconds` | `CONDUCTOR_AGENT_LIVENESS_CHECK_INTERVAL_SECONDS` | `10.0` |

Legacy `AGENTSPAN_*` names remain honored as fallbacks. See
[../../upgrading.md](../../upgrading.md).

## RunSettings

`RunSettings(Model?, Temperature?, MaxTokens?, ReasoningEffort?, ThinkingBudgetTokens?)`
— per-run LLM overrides. Only non-null fields override the agent. No `TopP`; it is not
part of the agentConfig wire contract.

## Usage

See [../concepts/deploy-serve-run.md](../concepts/deploy-serve-run.md).
