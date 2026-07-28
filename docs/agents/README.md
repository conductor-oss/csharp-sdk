# Conductor AI Agents (.NET) — Documentation

The Conductor .NET SDK's durable AI-agent layer — long-running, dynamic plan-execute,
and event-driven AI agents.

- **Package:** `conductor-ai` (NuGet)
- **Target:** .NET 8
- **Namespace:** `Conductor.AI`

## Start here

[getting-started.md](getting-started.md) — install, env vars, and a running agent in
under 30 seconds.

## Concepts

| Doc | Covers |
|---|---|
| [concepts/agents.md](concepts/agents.md) | Defining an agent, instructions, `[AgentDef]`. |
| [concepts/tools.md](concepts/tools.md) | `[Tool]` methods, built-in factories, credentials. |
| [concepts/multi-agent.md](concepts/multi-agent.md) | Strategies, pipelines, handoffs, transitions. |
| [concepts/guardrails.md](concepts/guardrails.md) | Input/output validation, regex and LLM guardrails. |
| [concepts/termination.md](concepts/termination.md) | Stop conditions and text gates. |
| [concepts/callbacks.md](concepts/callbacks.md) | Lifecycle hooks and `CallbackHandler`. |
| [concepts/streaming-hitl.md](concepts/streaming-hitl.md) | Event streams, approvals, human-in-the-loop. |
| [concepts/structured-output.md](concepts/structured-output.md) | `OutputType` and schema-enforced results. |
| [concepts/stateful.md](concepts/stateful.md) | Domain-routed workers, message queues, liveness. |
| [concepts/scheduling.md](concepts/scheduling.md) | Cron triggers and the schedule lifecycle. |
| [concepts/deploy-serve-run.md](concepts/deploy-serve-run.md) | Runtime options, the four verbs, worker tuning, PLAN_EXECUTE. |

## Frameworks

Author agents in the shape of another framework and run them on the same runtime.

| Doc | Status |
|---|---|
| [frameworks/openai.md](frameworks/openai.md) | Supported — `conductor-ai-openai` |
| [frameworks/google-adk.md](frameworks/google-adk.md) | Supported — `conductor-ai-google-adk` |
| [frameworks/semantic-kernel.md](frameworks/semantic-kernel.md) | Supported — `conductor-ai-semantic-kernel` (.NET only) |
| [frameworks/langchain.md](frameworks/langchain.md) | Not available in .NET |
| [frameworks/langgraph.md](frameworks/langgraph.md) | Not available in .NET |
| [frameworks/claude-agent-sdk.md](frameworks/claude-agent-sdk.md) | Not available in .NET |

## Reference

| Doc | Covers |
|---|---|
| [reference/api.md](reference/api.md) | The public surface, one section per type. |
| [reference/runtime.md](reference/runtime.md) | `AgentRuntime`, `AgentRuntimeOptions`, `AgentConfig`, `RunSettings`. |
| [reference/client.md](reference/client.md) | `IAgentClient` control plane. |
| [reference/agent-definition.md](reference/agent-definition.md) | The `[AgentDef]` attribute surface. |
| [reference/agent-schema.md](reference/agent-schema.md) | The serialized `agentConfig` wire format. |
| [reference/agent-schema.json](reference/agent-schema.json) | Machine-readable schema. |

## At a glance

```csharp
using Conductor.AI;

var agent = new Agent("greeter")
{
    Model        = "anthropic/claude-sonnet-4-6",
    Instructions = "You are a friendly assistant. Keep responses brief.",
};

await using var runtime = new AgentRuntime();
var result = await runtime.RunAsync(agent, "Say hello!");
result.PrintResult();
```

You need a running Conductor server (default `http://localhost:8080/api`). See
[getting-started.md](getting-started.md).

## Core SDK

Workflows, workers, and the non-agent client surface are documented one level up in
[../README.md](../README.md).
