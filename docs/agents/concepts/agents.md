# Agents

`Agent` is the single orchestration primitive — an LLM with optional tools and/or
sub-agents. Everything else in this section builds on it.

## Defining an agent

The name must match `^[a-zA-Z_][a-zA-Z0-9_-]*$`.

Object-initializer style:

```csharp
var agent = new Agent("assistant")
{
    Model        = "anthropic/claude-sonnet-4-6",
    Instructions = "You are helpful.",
    Tools        = tools,             // optional: List<ToolDef>
    Agents       = [subAgent],        // optional: sub-agents (multi-agent)
    Strategy     = Strategy.Handoff,  // required when Agents is non-empty
    MaxTurns     = 10,                // optional
    Temperature  = 0.2,               // optional
    MaxTokens    = 2048,              // optional
};
```

Fluent builder style (`AgentBuilder`):

```csharp
var agent = AgentBuilder.Create("assistant")
    .WithModel("anthropic/claude-sonnet-4-6")
    .WithInstructions("You are helpful.")
    .WithTools(tools.ToArray())
    .WithMaxTurns(10)
    .Build();
```

`Build()` throws `ConfigurationException` if sub-agents are present but no
`Strategy` is set.

## Instructions

A static system prompt:

```csharp
var agent = new Agent("a") { Instructions = "You are helpful." };
```

Dynamic instructions — a `Func<string>` re-evaluated every time the agent is
submitted to the server, so the prompt can reflect current state (date, flags,
fetched context). `InstructionsFn` takes precedence over `Instructions`:

```csharp
var agent = new Agent("a")
{
    InstructionsFn = () => $"You are helpful. Today is {DateTime.UtcNow:yyyy-MM-dd}.",
};

// builder:
AgentBuilder.Create("a").WithInstructions(() => $"Today is {DateTime.UtcNow:d}").Build();
```

Server-side prompt templates are also supported via `PromptTemplate`:

```csharp
agent.PromptTemplateInstructions =
    new PromptTemplate("support_prompt", Variables: new() { ["tone"] = "warm" });
```

## Agents from methods (`[AgentDef]`)

Define agents declaratively on a host object. `[Tool]` / `[Guardrail]` methods on
the same object are attached automatically (filter with the `Tools` / `Guardrails`
properties). An `[AgentDef]` method may return `void` (attribute-only), `string` (a
no-arg method becomes dynamic instructions), or `Agent` (a full factory).

```csharp
internal sealed class AgentHost
{
    [Tool("Greet the user.")]
    public Dictionary<string, object> SayHi() => new() { ["greeting"] = "hello" };

    // returns string -> becomes InstructionsFn; attaches only the say_hi tool
    [AgentDef(Name = "greeter", Tools = new[] { "say_hi" })]
    public string Greeter() => "Be friendly.";

    // void -> defined entirely by the attribute; wires greeter as a sub-agent
    [AgentDef(Name = "coordinator", Tools = new string[0],
              Agents = new[] { "greeter" }, Strategy = Strategy.Sequential)]
    public void Coordinator() { }
}

var host = new AgentHost();

List<Agent> all = Agent.FromInstance(host);            // all [AgentDef] methods
Agent       one = Agent.FromInstance(host, "greeter"); // a single one by name
one.Model = "anthropic/claude-sonnet-4-6";             // supply a model if the attribute left it unset

await using var runtime = new AgentRuntime();
await runtime.RunAsync(one, "Greet the user by calling say_hi.");
```

`[AgentDef]` properties: `Name`, `Model`, `Instructions`, `Tools` (`["*"]` = all,
`[]` = none, or names), `Guardrails`, `Agents` (sub-agent names), `Strategy`,
`MaxTurns`, `MaxTokens`, `Temperature`.

See [reference/agent-definition.md](../reference/agent-definition.md) for the full
attribute surface.

## Next

- [tools.md](tools.md) — giving an agent capabilities
- [multi-agent.md](multi-agent.md) — strategies, pipelines, handoffs
- [deploy-serve-run.md](deploy-serve-run.md) — getting an agent onto the server
