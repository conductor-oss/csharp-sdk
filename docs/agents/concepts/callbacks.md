# Callbacks

Two equivalent ways to hook the agent lifecycle.

## Inline delegate fields

Quick, per-agent:

```csharp
var agent = new Agent("monitored")
{
    BeforeModelCallback = messages =>
    {
        Console.WriteLine($"[before_model] sending {messages?.Count ?? 0} messages");
        return [];   // empty dict = continue normally; non-empty = skip the LLM / override
    },
    AfterModelCallback = llmResult =>
    {
        Console.WriteLine($"[after_model] {llmResult?.Length ?? 0} chars");
        return [];   // empty = keep response; non-empty = override
    },
};
```

There are six delegate slots: `BeforeAgentCallback` / `AfterAgentCallback`,
`BeforeModelCallback` / `AfterModelCallback`, `BeforeToolCallback` /
`AfterToolCallback`. The before/after-agent and before/after-tool variants take a
`Dictionary<string, JsonElement>` kwargs map.

## `CallbackHandler` subclasses

Composable and reusable across agents. Override only the hooks you care about, then
register a list via `Agent.Callbacks`. Handlers run in list order and the first
non-empty return short-circuits.

```csharp
internal sealed class ToolStartLogger : CallbackHandler
{
    public override Dictionary<string, object>? OnToolStart(Dictionary<string, JsonElement> kwargs)
    {
        Console.WriteLine("[before_tool]");
        return null;   // observe only
    }
}

var agent = new Agent("a") { Callbacks = [new ToolStartLogger()] };
// or: AgentBuilder.Create("a").WithCallbacks(new ToolStartLogger()).Build();
```

Hooks: `OnAgentStart` / `OnAgentEnd` / `OnModelStart` / `OnModelEnd` /
`OnToolStart` / `OnToolEnd`.

## Return-value semantics

The return value is the control channel, and it is uniform across both styles:

- **empty or `null`** — observe only; the run continues unchanged
- **non-empty** — override. Before-hooks skip the underlying operation and use your
  value; after-hooks replace the result.

This is what makes callbacks usable for caching, redaction, and short-circuiting —
not just logging.

## Positions on the wire

Callback positions map to server task names: `before_agent`, `after_agent`,
`before_model`, `after_model`, `before_tool`, `after_tool`. Those names appear in
execution traces, which is useful when [debugging](../../debugging.md).

## Callbacks vs guardrails

Callbacks are general-purpose interception. [Guardrails](guardrails.md) are the
purpose-built path for validation with retry/raise/fix/escalate semantics — reach
for those when the goal is enforcing a policy rather than observing a lifecycle.
