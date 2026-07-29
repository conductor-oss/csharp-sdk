# LangGraph

> **Not available in the .NET SDK.** There is no LangGraph adapter in
> `conductor-ai`. This page exists to keep the documentation structure aligned with
> the sibling SDKs; see the [Python SDK LangGraph guide](https://github.com/conductor-oss/python-sdk/blob/main/docs/agents/frameworks/langgraph.md)
> if you need it there.

## What to use instead

LangGraph models an agent as a state graph with explicit nodes and edges. The native
agent API expresses the same shape:

| LangGraph concept | .NET equivalent |
|---|---|
| Node | An `Agent` in the `Agents` list |
| Edge | `AllowedTransitions` entry |
| Conditional edge | `OnCondition` handoff, or `Strategy.Router` with a classifier |
| Cycle / loop | `Strategy.RoundRobin` or `Strategy.Swarm` with `MaxTurns` |
| Terminal state | `Termination` condition, or a `TextGate` on a pipeline |
| Checkpointer | Durable executions — every run is resumable via `ResumeAsync` |

```csharp
var team = new Agent("graph")
{
    Agents   = [planner, worker, reviewer],
    Strategy = Strategy.RoundRobin,
    MaxTurns = 6,
    AllowedTransitions = new()
    {
        ["planner"]  = ["worker"],
        ["worker"]   = ["reviewer"],
        ["reviewer"] = ["worker"],     // cycle back on rejection
    },
};
```

See [../concepts/multi-agent.md](../concepts/multi-agent.md) for the full transition
model and [../concepts/termination.md](../concepts/termination.md) for stop conditions.

## Determinism

Where LangGraph relies on your process to hold graph state, Conductor persists it
server-side, so a run survives process restarts without a checkpointer. For a plan
executed deterministically rather than decided turn-by-turn, see
`Strategy.PlanExecute` in
[../concepts/deploy-serve-run.md](../concepts/deploy-serve-run.md#plans-and-plan_execute).
