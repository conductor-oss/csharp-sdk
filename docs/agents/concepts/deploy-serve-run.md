# Deploy, serve, run

How an agent gets onto the server, and how the runtime is configured.

## Runtime initialization

`new AgentRuntime()` reads connection settings from the environment —
`CONDUCTOR_SERVER_URL` / `CONDUCTOR_AUTH_KEY` / `CONDUCTOR_AUTH_SECRET`, defaulting
to `http://localhost:8080/api` with no auth. Override any of them with
`AgentRuntimeOptions`:

```csharp
await using var runtime = new AgentRuntime(new AgentRuntimeOptions
{
    ServerUrl  = "https://my-server.example.com/api",
    AuthKey    = "...",     // optional; with AuthSecret enables Orkes auth (JWT exchange)
    AuthSecret = "...",
});
```

When both `AuthKey` and `AuthSecret` are set, the runtime configures Orkes
authentication for worker polling automatically. With neither set, it runs in
no-auth mode (local / OSS Conductor).

You can also pass an explicit `Configuration`, sharing it — and its token cache —
with any other domain client built from the same `Configuration`:

```csharp
await using var runtime = new AgentRuntime(myConfiguration, AgentConfig.FromEnv());
```

The runtime is both `IAsyncDisposable` and `IDisposable`; `await using` (or `using`)
shuts down any local tool workers it started.

See [../../connection-authentication.md](../../connection-authentication.md) for the
full connection and auth model.

## Worker tuning and AgentConfig

Local `[Tool]` methods are served by worker poll loops the runtime owns.
`AgentConfig` (the second constructor argument, or `AgentConfig.FromEnv()`) controls
them and a handful of runtime behaviors, with lenient env parsing — invalid or empty
values fall back to the default rather than throwing:

| `AgentConfig` property | Env var | Default | Meaning |
|---|---|---|---|
| `WorkerThreadCount` | `CONDUCTOR_AGENT_WORKER_THREADS` | `1` | Worker threads per task type. |
| `WorkerPollIntervalMs` | `CONDUCTOR_AGENT_WORKER_POLL_INTERVAL` | `100` | Poll interval in milliseconds. |
| `AutoStartWorkers` | `CONDUCTOR_AGENT_AUTO_START_WORKERS` | `true` | Whether run/start/stream auto-register + start local tool workers. |
| `DaemonWorkers` | `CONDUCTOR_AGENT_DAEMON_WORKERS` | `true` | Whether worker threads are background/daemon threads. |
| `StreamingEnabled` | `CONDUCTOR_AGENT_STREAMING_ENABLED` | `true` | Whether `StreamAsync` attempts SSE before falling back to status-polling. |
| `LivenessEnabled` | `CONDUCTOR_AGENT_LIVENESS_ENABLED` | `true` | Whether stateful runs get a liveness monitor. |
| `LivenessStallSeconds` | `CONDUCTOR_AGENT_LIVENESS_STALL_SECONDS` | `30.0` | How long an unpolled tool task may sit before it's flagged as stalled. |
| `LivenessCheckIntervalSeconds` | `CONDUCTOR_AGENT_LIVENESS_CHECK_INTERVAL_SECONDS` | `10.0` | How often the liveness monitor polls the workflow's task list. |

The legacy `AGENTSPAN_*` equivalents are still honored as fallbacks when the
`CONDUCTOR_AGENT_*` name is unset. See [../../upgrading.md](../../upgrading.md).

```csharp
using var runtime = new AgentRuntime();
int threads = runtime.WorkerThreadCount;     // reflects CONDUCTOR_AGENT_WORKER_THREADS
int pollMs  = runtime.WorkerPollIntervalMs;  // reflects CONDUCTOR_AGENT_WORKER_POLL_INTERVAL
```

## The four verbs

Ordered roughly from "just run it" to "CI/CD pipeline":

| Verb | What it does | When |
|---|---|---|
| `RunAsync` / `StartAsync` | Compile + register + start (+ host local workers), then wait or stream. | Day-to-day execution. |
| `DeployAsync` | Compile + register the workflow on the server. No execution, no workers. | CI/CD: push agent definitions. |
| `ServeAsync` | Register local tool workers for already-deployed agents and block until cancelled. | Long-running worker service. |
| `PlanAsync` / `Plan` | Compile to a Conductor `WorkflowDef` and return it. No registration, no execution. | Inspect/debug/validate the compiled workflow. |

**Deploy** (returns one `DeploymentInfo` per agent):

```csharp
var results = await runtime.DeployAsync(docAssistant, opsBot);
foreach (var info in results)
    Console.WriteLine($"{info.AgentName} -> {info.RegisteredName}");
```

**Serve** deploys each agent (idempotently, deploy-before-worker-start) and
registers its local tool workers. By default it blocks until the token is cancelled
— pass an explicit `blocking: false` to return as soon as the workers are up and
polling in the background instead:

```csharp
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

await using var runtime = new AgentRuntime();
await runtime.ServeAsync(cts.Token, docAssistant);   // params Agent[]; blocks until cancelled

// or return immediately once workers are polling:
await runtime.ServeAsync(blocking: false, agents: new[] { docAssistant });
```

**Run a pre-deployed agent by name** (no agentConfig payload, no local workers unless
you also serve them):

```csharp
var handle = await runtime.StartByNameAsync(agent.Name, "Validate change CHG-901.");
var result = await runtime.RunByNameAsync(agent.Name, "Validate change CHG-901.");
```

A common deploy-then-serve recovery pattern:

```csharp
await runtime.DeployAsync(agent);
var handle = await runtime.StartByNameAsync(agent.Name, prompt);

using var cts = new CancellationTokenSource();
var serveTask = runtime.ServeAsync(cts.Token, agent);   // worker service comes up after start
// ... poll runtime.GetStatusAsync(handle.ExecutionId) until complete ...
cts.Cancel();
var result = await handle.WaitAsync();
```

**Plan** (dry-run compile):

```csharp
var workflowDef = await runtime.PlanAsync(agent);   // JsonNode? — the compiled WorkflowDef
```

## The IAgentClient control plane

`IAgentClient` (implemented by `OrkesAgentClient`) is the control-plane client for
the `/agent/*` API — compile, deploy, start, status, respond, stream — plus
convenience `RunAsync` / `StartAsync` / `DeployAsync` / `ScheduleAsync`. It rides the
same `ApiClient`/`Configuration` as the rest of the SDK. Obtain one via
`OrkesApiClient.GetAgentClient()` or `Configuration.GetAgentClient()`, both sharing
that `Configuration`'s token cache (no separate token client).

The runtime exposes its own client as `runtime.Client`:

```csharp
await using var runtime = new AgentRuntime();
IAgentClient client = runtime.Client;
```

**Run is control-plane only.** `client.RunAsync(...)` starts the agent and polls to a
result but does **not** register or poll local tool workers. Use it for LLM-only
agents, agents with server-side tools (HTTP/MCP/media/RAG), or pre-deployed
workflows. Agents with local `[Tool]` functions must run through `AgentRuntime`,
which owns worker orchestration.

```csharp
// control-plane run (no local workers)
var result = await runtime.Client.RunAsync(llmOnlyAgent, "Summarize this.");

// or build a client directly on a Configuration
var configuration = new Configuration { BasePath = "http://localhost:8080/api" };
using var standalone = configuration.GetAgentClient();
var handle = await standalone.StartAsync(agent, "Hello");
```

Full member list: [reference/client.md](../reference/client.md).

## RunSettings — per-run overrides

`RunSettings` carries per-invocation LLM overrides on top of an `Agent`'s own
settings — `Model`, `Temperature`, `MaxTokens`, `ReasoningEffort`,
`ThinkingBudgetTokens`. There is no `TopP`; it isn't part of the agentConfig wire
contract. Only the fields you set override the agent; everything else is left as the
agent defined it.

```csharp
var result = await runtime.RunAsync(agent, "Summarize this",
    runSettings: new RunSettings(Model: "openai/gpt-4o", Temperature: 0.2, MaxTokens: 2048));
```

Overrides mutate the serialized **root** agent config before `start`, so they flow
into the root agent's LLM tasks without needing a new server field — sub-agents in a
multi-agent strategy keep their own settings, with no cascade.

## Plans and PLAN_EXECUTE

`Strategy.PlanExecute` builds a plan-and-compile harness: a `Planner` agent produces
a JSON plan that the server executes deterministically, with an optional `Fallback`
agent for recovery.

```csharp
var planner = new Agent("planner") { Model = "anthropic/claude-sonnet-4-6", Instructions = "..." };

var harness = new Agent("onboarding")
{
    Model    = "anthropic/claude-sonnet-4-6",
    Strategy = Strategy.PlanExecute,
    Planner  = planner,
    Fallback = fallback,            // optional; absent => plan failures terminate
    FallbackMaxTurns = 3,
    Tools    = ToolRegistry.FromInstance(new OnboardingTools()),
};
```

**Planner context** grounds the planner in domain rules on every invocation — inline
text and/or fetched URLs with credentialed headers. Only valid with
`Strategy.PlanExecute`:

```csharp
using Conductor.AI.Plans;

harness.PlannerContext =
[
    Context.FromText("Onboarding phases in order: validate_kyc, create_account, send_welcome_email."),
    Context.FromUrl("https://docs.example.com/onboarding.md",
        headers: new() { ["Authorization"] = "Bearer ${CONFLUENCE_TOKEN}" },
        required: true, maxBytes: 8192),
];
// builder: .WithPlannerContext("rule one", "rule two") or .WithPlannerContext(Context.FromUrl(...))
```

**Supplying a deterministic plan** skips the planner LLM entirely. Build a `Plan` of
`Step`s; wire one step's whole output into another with `new Ref("step_id")` — the
referenced step must be in `DependsOn`:

```csharp
using Conductor.AI.Plans;

var plan = new Plan
{
    Steps =
    {
        new Step("produce")
        {
            Operations = { new Op("produce", new() { ["record_id"] = "r-001" }) },
        },
        new Step("enrich")
        {
            DependsOn  = { "produce" },
            Operations = { new Op("enrich", new() { ["record"] = new Ref("produce") }) },
        },
        new Step("report")
        {
            DependsOn  = { "produce", "enrich" },
            Operations =
            {
                new Op("report", new()
                {
                    ["record"]   = new Ref("produce"),
                    ["enriched"] = new Ref("enrich"),
                }),
            },
        },
    },
};

var result = await runtime.RunAsync(harness, "demo", plan: plan);
```

An `Op` either calls a tool with literal `Args` (as above) or generates its args at
run time with an LLM via
`Op.WithGenerate(tool, new Generate { Instructions = ..., OutputSchema = ... })`. A
`Plan` may also carry top-level `Validation`, `OnSuccess`, and `OnFailure` actions.

The simpler `Agent.EnablePlanning = true` is unrelated: it just augments the system
prompt with a "plan first, then execute" preamble (a Google ADK feature), without the
PLAN_EXECUTE harness.
