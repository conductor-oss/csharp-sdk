# Multi-agent

Set `Agents` and a `Strategy` to compose agents into a system.

## Strategies

| Strategy | Behavior |
|---|---|
| `Handoff` | Parent LLM delegates to a sub-agent. |
| `Sequential` | Agents run in order; each output feeds the next. |
| `Parallel` | All sub-agents run concurrently; results aggregated. |
| `Router` | A dedicated `Router` agent classifies and routes to one specialist. |
| `RoundRobin` | Sub-agents take turns. |
| `Random` | A sub-agent is picked at random. |
| `Swarm` | Collaborative swarm with handoff triggers. |
| `Manual` | Caller selects the next agent. |
| `PlanExecute` | Plan-and-execute harness (see [../../workflow-lifecycle.md](../../workflow-lifecycle.md)). |

Handoff team:

```csharp
var support = new Agent("support")
{
    Model        = "anthropic/claude-sonnet-4-6",
    Instructions = "Route requests to the right specialist: billing, technical, or sales.",
    Agents       = [billingAgent, technicalAgent, salesAgent],
    Strategy     = Strategy.Handoff,
};
```

Router with a dedicated classifier:

```csharp
var team = new Agent("dev_team")
{
    Agents   = [planner, coder, reviewer],
    Strategy = Strategy.Router,
    Router   = selector,   // a classifier Agent
};
```

## Sequential pipelines

The `>>` operator is equivalent to a `Strategy.Sequential` parent over `[a, b, c]`:

```csharp
var pipeline = researcher >> writer >> editor;
var result   = await runtime.RunAsync(pipeline, "AI agents in 2025");
```

## Constrained transitions

Constrain who may transition to whom with `AllowedTransitions`:

```csharp
var team = new Agent("code_review")
{
    Agents   = [developer, reviewer, approver],
    Strategy = Strategy.RoundRobin,
    MaxTurns = 6,
    AllowedTransitions = new()
    {
        ["developer"] = ["reviewer"],
        ["reviewer"]  = ["developer", "approver"],
        ["approver"]  = ["developer"],
    },
};
```

## Handoffs

In a `Swarm`, `Handoff` triggers transfer control to another agent when no
explicit transfer tool was called. Build them with the three trigger types and
attach via `Agent.Handoffs` (or `.WithHandoffs(...)`):

```csharp
var agent = new Agent("triage")
{
    Strategy = Strategy.Swarm,
    Agents   = [refundSpecialist, supervisor],
    Handoffs =
    [
        OnTextMention.Of("refund", "refund_specialist"),
        OnToolResult.Of("check_eligibility", "refund_specialist", "eligible"),
        new OnCondition("supervisor",
            ctx => ctx.TryGetValue("result", out var r) && (r?.ToString()?.Length ?? 0) > 500),
    ],
};
```

- `OnTextMention.Of(text, target)` — fires when the agent output contains `text`.
- `OnToolResult.Of(toolName, target)` / `OnToolResult.Of(toolName, target, resultContains)` — fires when a tool returns (optionally containing a substring).
- `new OnCondition(target, predicate)` — fires when your predicate over the context map returns true. The context carries `result`, `messages`, `tool_name`, `tool_result`.

## Scatter-gather

`Agent.ScatterGather(name, worker, ...)` builds a coordinator that fans a worker
agent out in parallel and aggregates the results.

## Agents as tools

Handoff delegation transfers control. To call an agent inline like a function
instead, wrap it with `AgentTool.Create(agent)` — see
[tools.md](tools.md#built-in-tool-factories).

## Per-run overrides and sub-agents

`RunSettings` overrides mutate the serialized **root** agent config before `start`,
so sub-agents in a multi-agent strategy keep their own settings — there is no
cascade. See [deploy-serve-run.md](deploy-serve-run.md#runsettings--per-run-overrides).
