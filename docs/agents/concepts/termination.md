# Termination

Composable stop conditions on `Agent.Termination`, plus the lighter-weight
`TextGate` for sequential pipelines.

## Termination conditions

Combine with `&` (AND) and `|` (OR).

```csharp
var agent = new Agent("researcher")
{
    Termination = new TextMentionTermination("DONE"),
};

// composed
var term   = new MaxMessageTermination(10) | new TextMentionTermination("DONE");
var budget = new TokenUsageTermination(maxTotalTokens: 50_000);
```

Available: `TextMentionTermination`, `StopMessageTermination`,
`MaxMessageTermination`, `TokenUsageTermination`, and the `AndTermination` /
`OrTermination` composites produced by the operators.

## Text gates

A `TextGate` stops a sequential pipeline after the agent if its output contains
the sentinel text. It is compiled server-side, so there is no worker round-trip:

```csharp
var checker = new Agent("checker") { Model = "openai/gpt-4o", Gate = new TextGate("STOP") };
var fixer   = new Agent("fixer")   { Model = "openai/gpt-4o" };
var pipeline = checker >> fixer;   // halts after checker if its output contains "STOP"
```

`new TextGate(text, caseSensitive: true)` — set `caseSensitive: false` to match
loosely.

## Gate vs termination

They solve different problems:

- **`Termination`** bounds a single agent's own turn loop — how long *this* agent
  keeps going.
- **`Gate`** halts a *pipeline* after this agent, so downstream agents never run.

## Turn and token ceilings

`MaxTurns` on the agent is a hard ceiling independent of `Termination`, and
`MaxTokens` bounds a single completion. `TokenUsageTermination` is the cumulative
budget across the run.

## Reading why a run stopped

`AgentResult.FinishReason` reports the cause — `Stop`, `Length`, `ToolCalls`,
`Error`, `Cancelled`, `Timeout`, `Guardrail`, or `Rejected`. See
[reference/api.md](../reference/api.md#results).
