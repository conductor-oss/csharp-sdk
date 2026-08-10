# Semantic Kernel

Bridges Microsoft Semantic Kernel plugins. If you already have classes with
`[KernelFunction]`-annotated methods, hand them straight to `SemanticKernelAgent.From`
and each function becomes a tool.

| | |
|---|---|
| Package | `conductor-ai-semantic-kernel` |
| Namespace | `Conductor.AI.SemanticKernel` |
| Entry point | `SemanticKernelAgent.From(...)` |

```bash
dotnet add package conductor-ai-semantic-kernel
```

Inside this repo, reference
`Conductor.AI.SemanticKernel/Conductor.AI.SemanticKernel.csproj` directly.

> This adapter is specific to the .NET SDK — Semantic Kernel is a .NET-first
> framework and has no counterpart in the Python or Java SDK docs.

## From a `[KernelFunction]` object

```csharp
using System.ComponentModel;
using Conductor.AI;
using Conductor.AI.SemanticKernel;
using Microsoft.SemanticKernel;

internal sealed class CalculatorPlugin
{
    [KernelFunction, Description("Add two integers and return their sum.")]
    public int Add(
        [Description("first number")]  int a,
        [Description("second number")] int b) => a + b;
}

var agent = SemanticKernelAgent.From(
    name:         "sk_calc_agent",
    model:        "anthropic/claude-sonnet-4-6",
    instructions: "You are a calculator. Use the tools to answer math questions.",
    new CalculatorPlugin());

await using var runtime = new AgentRuntime();
var result = await runtime.RunAsync(agent, "What is 17 + 25?");
result.PrintResult();
```

## From a prebuilt `KernelPlugin`

```csharp
KernelPlugin plugin = KernelPluginFactory.CreateFromObject(new CalculatorPlugin(), "calc");

var agent = SemanticKernelAgent.From(
    name:         "sk_kernelplugin",
    model:        "anthropic/claude-sonnet-4-6",
    instructions: "Solve arithmetic using the calc plugin.",
    plugin);
```

`SemanticKernelAgent.From(name, model, instructions, params object[] plugins)` accepts
any mix of `[KernelFunction]` objects and `KernelPlugin` instances.
`SemanticKernelAgent.IsSemanticKernelPlugin(obj)` reports whether an object qualifies.

## How it differs from the other adapters

This adapter builds a plain `Agent` with **no** `Framework` tag — there is no
server-side normalizer involved. The kernel functions run as local worker tools,
invoked through the `KernelFunction` itself, so Semantic Kernel's own argument
coercion and async unwrapping apply.

Because the tools are local workers, these agents must run through `AgentRuntime`
rather than the bare `IAgentClient` — see
[../concepts/deploy-serve-run.md](../concepts/deploy-serve-run.md#the-iagentclient-control-plane).
