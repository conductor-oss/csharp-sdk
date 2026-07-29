# Google ADK

Mirrors the Google ADK (Agent Development Kit) shape. Consumed by the server's
`GoogleADKNormalizer`.

| | |
|---|---|
| Package | `conductor-ai-google-adk` |
| Namespace | `Conductor.AI.GoogleADK` |
| Entry point | `GoogleADKAgent.Builder()` / `GoogleADKAgent.From(...)` |

```bash
dotnet add package conductor-ai-google-adk
```

Inside this repo, reference `Conductor.AI.GoogleADK/Conductor.AI.GoogleADK.csproj`
directly.

## Differences from OpenAI

At the wire level:

- `Instruction` (singular), not `Instructions`
- `SubAgents`, not handoffs
- bare model names like `"gemini-2.0-flash"` are prefixed with `"google_gemini/"`
  server-side

```csharp
using Conductor.AI;
using Conductor.AI.GoogleADK;

var agent = GoogleADKAgent.Builder()
    .Name("greeter")
    .Model("gemini-2.0-flash")
    .Instruction("You are a friendly assistant. Keep responses concise.")  // note: singular
    .Build();

await using var runtime = new AgentRuntime();
var result = await runtime.RunAsync(agent, "Say hello and share a fun fact about ML.");
result.PrintResult();
```

## Tools and sub-agents

Tools work the same way as the OpenAI adapter (`.Tools(new MyTools())` /
`.ToolDefs(...)`). Delegate to children with `.SubAgents(child1, child2)`.

Shortcut: `GoogleADKAgent.From(name, model, instruction, params object[] toolObjects)`.

## Planning preamble

`Agent.EnablePlanning = true` augments the system prompt with a "plan first, then
execute" preamble — this is a Google ADK feature, and it is distinct from
`Strategy.PlanExecute`. See
[../concepts/deploy-serve-run.md](../concepts/deploy-serve-run.md#plans-and-plan_execute).

## What still applies

The adapter builds a normal `Agent`, so everything in
[../concepts/](../concepts/agents.md) still applies and you run it with the same
`AgentRuntime`.
