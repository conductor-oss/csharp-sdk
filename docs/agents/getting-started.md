# Getting Started

Get an agent running in under 30 seconds.

## 1. Install

The SDK ships as the `conductor-ai` NuGet package (target framework: .NET 8).

```bash
dotnet new console -n MyAgent
cd MyAgent
dotnet add package conductor-ai
```

> Working inside this repository instead of from NuGet? Reference the project directly:
>
> ```xml
> <ItemGroup>
>   <ProjectReference Include="path/to/sdk/csharp/src/Conductor.AI/Conductor.AI.csproj" />
> </ItemGroup>
> ```

## 2. Point at a server

You need a running Conductor server. The defaults assume a local one at
`http://localhost:8080/api`.

| Variable | Default | Description |
|---|---|---|
| `CONDUCTOR_SERVER_URL` | `http://localhost:8080/api` | Server URL. Wins over `AGENTSPAN_SERVER_URL` if both are set. |
| `CONDUCTOR_AUTH_KEY` | — | Auth key. Unset = no-auth mode (local / OSS). Wins over `AGENTSPAN_AUTH_KEY`. |
| `CONDUCTOR_AUTH_SECRET` | — | Auth secret. Set together with the key for Orkes Cloud. Wins over `AGENTSPAN_AUTH_SECRET`. |
| `AGENTSPAN_SERVER_URL` / `AGENTSPAN_AUTH_KEY` / `AGENTSPAN_AUTH_SECRET` | — | Legacy names, still honored as fallbacks when the `CONDUCTOR_*` ones are unset. |

```bash
export CONDUCTOR_SERVER_URL=http://localhost:8080/api
export OPENAI_API_KEY=<YOUR-KEY>
export CONDUCTOR_AGENT_LLM_MODEL=openai/gpt-4o-mini
# Orkes Cloud only:
# export CONDUCTOR_AUTH_KEY=...
# export CONDUCTOR_AUTH_SECRET=...
```

The runtime reads the `CONDUCTOR_*` connection variables on construction. You can also pass them explicitly via `AgentRuntimeOptions` (see [concepts/deploy-serve-run.md](concepts/deploy-serve-run.md#runtime-initialization)).

`CONDUCTOR_AGENT_LLM_MODEL` is a convention of the bundled examples, not something the SDK itself reads — an `Agent` takes its model from the `Model` property. It's shown here because every example under `Conductor.AI.Examples/` picks it up.

## 3. Run an agent

Replace `Program.cs` with:

```csharp
using Conductor.AI;

var agent = new Agent("greeter")
{
    Model        = "anthropic/claude-sonnet-4-6",
    Instructions = "You are a friendly assistant. Keep responses brief.",
};

await using var runtime = new AgentRuntime();
var result = await runtime.RunAsync(agent, "Say hello and tell me a fun fact about C#.");
result.PrintResult();
```

```bash
dotnet run
```

That is the whole loop: define an `Agent`, open an `AgentRuntime`, `await runtime.RunAsync(agent, prompt)`, and read the `AgentResult`. `await using` disposes the runtime (and shuts down any local tool workers) when you are done.

## Reading the result

`RunAsync` returns an [`AgentResult`](reference/api.md#results). Common members:

```csharp
result.PrintResult();                 // formatted summary to stdout
bool ok       = result.IsSuccess;     // Status == Completed
var  output   = result.Output;        // Dictionary<string, object>?; final text is usually output["result"]
var  tokens   = result.TokenUsage;    // TokenUsage? (prompt / completion / total)
var  finish   = result.FinishReason;  // FinishReason? (Stop, Length, Guardrail, Rejected, ...)
string execId = result.ExecutionId;   // durable execution id on the server
```

## Next

- [concepts/tools.md](concepts/tools.md) — tools and credentials.
- [concepts/multi-agent.md](concepts/multi-agent.md) — multi-agent orchestration and handoffs.
- [concepts/guardrails.md](concepts/guardrails.md) — input/output validation.
- [concepts/streaming-hitl.md](concepts/streaming-hitl.md) — streaming and human-in-the-loop.
- [concepts/deploy-serve-run.md](concepts/deploy-serve-run.md) — deploy/serve, the control-plane `IAgentClient`, PLAN_EXECUTE.
- [README.md](README.md) — the full index.
</content>
