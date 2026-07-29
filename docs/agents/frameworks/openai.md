# OpenAI Agents

Mirrors the OpenAI Agents SDK shape. The SDK routes the agent through
`framework="openai"` and the server's `OpenAINormalizer` consumes it. Model names
without a provider prefix are auto-prefixed with `openai/` server-side.

| | |
|---|---|
| Package | `conductor-ai-openai` |
| Namespace | `Conductor.AI.OpenAI` |
| Entry point | `OpenAIAgent.Builder()` / `OpenAIAgent.From(...)` |

```bash
dotnet add package conductor-ai-openai
```

Inside this repo, reference `Conductor.AI.OpenAI/Conductor.AI.OpenAI.csproj` directly.

## Basic agent

```csharp
using Conductor.AI;
using Conductor.AI.OpenAI;

var agent = OpenAIAgent.Builder()
    .Name("greeter")
    .Instructions("You are a friendly assistant. Keep responses concise.")
    .Model("anthropic/claude-sonnet-4-6")
    .Build();

await using var runtime = new AgentRuntime();
var result = await runtime.RunAsync(agent, "Say hello and share a fun fact about C#.");
result.PrintResult();
```

## Tools

Pass objects whose public methods carry `[Tool]`; they are scanned via
`ToolRegistry.FromInstance` and become worker tools:

```csharp
var agent = OpenAIAgent.Builder()
    .Name("multi_tool_agent")
    .Instructions("Use the weather and calculator tools to answer questions.")
    .Model("anthropic/claude-sonnet-4-6")
    .Tools(new WeatherTools())          // [Tool]-annotated object(s)
    .Build();

internal sealed class WeatherTools
{
    [Tool(Name = "get_weather", Description = "Get the current weather for a city.")]
    public string GetWeather(string city) => $"Sunny in {city}.";
}
```

Use `.ToolDefs(...)` to add already-built `ToolDef`s (HTTP, MCP, etc.) — see
[../concepts/tools.md](../concepts/tools.md).

## Handoffs

The OpenAI "handoffs" list of sub-agents the LLM can transfer to:

```csharp
var triage = OpenAIAgent.Builder()
    .Name("customer_service_triage")
    .Instructions("Triage the request and hand off to the right specialist.")
    .Model("anthropic/claude-sonnet-4-6")
    .Handoffs(orderAgent, refundAgent, salesAgent)
    .Build();
```

## Shortcuts

`OpenAIAgent.From(name, model, instructions, params object[] toolObjects)` is a
one-liner equivalent. A structured-output type name can be set via
`.OutputType("MyType")`.

## What still applies

The adapter builds a normal `Agent`, so everything in
[../concepts/](../concepts/agents.md) still applies and you run it with the same
`AgentRuntime`.
