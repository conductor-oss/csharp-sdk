# Schedules and events

Triggering work without a client calling `StartWorkflow`.

## Workflow schedules

Cron schedules for workflows are managed through `SchedulerResourceApi`:

```csharp
using Conductor.Api;
using Conductor.Client;

var schedulerClient = configuration.GetClient<SchedulerResourceApi>();
```

See [api-map.md](api-map.md#scheduling-and-events).

## Agent schedules

The agent layer has a first-class schedule API — `runtime.Schedules` — with declarative
reconciliation on deploy:

```csharp
using Conductor.AI.Scheduling;

await runtime.DeployAsync(agent, new[]
{
    new Schedule
    {
        Name        = "weekday-9am",
        Cron        = "0 0 9 * * MON-FRI",
        Timezone    = "America/Los_Angeles",
        Description = "Weekday morning digest",
    },
});
```

`Cron` is a **6-field Quartz** expression with seconds precision — not 5-field Unix cron.
Full lifecycle and reconciliation semantics:
[agents/concepts/scheduling.md](agents/concepts/scheduling.md).

## Events

`EventResourceApi` manages event handlers and queues. An event handler reacts to a message
on a queue by starting a workflow or completing a task, which is how Conductor integrates
with external message buses.

`EventTask` publishes an event from inside a workflow — see
[workflows.md](workflows.md#task-types).

## Waiting for external input

Three different mechanisms, for three different situations:

| Mechanism | Waits for | Use |
|---|---|---|
| `WaitTask` | A duration or timestamp | Deliberate delay. |
| `WaitForWebhookTask` | An inbound webhook | An external system will call back. |
| `HumanTask` | Human input | A person must act. |

For agents, the equivalents are `WaitForMessageTool` (Workflow Message Queue) and
`HumanTool`. See
[agents/concepts/stateful.md](agents/concepts/stateful.md) and
[agents/concepts/streaming-hitl.md](agents/concepts/streaming-hitl.md).

## Message queue

`runtime.SendMessageAsync(executionId, message)` pushes into the Workflow Message Queue,
which a waiting `WaitForMessageTool` dequeues. This requires
`conductor.workflow-message-queue.enabled=true` on the server, and the agent must be
`Stateful = true` so the waiting worker is the one that receives the message.

At the core-SDK level the equivalents are `SendWorkflowMessageAsync` and `SignalAsync` on
`IAgentClient` — see [agents/reference/client.md](agents/reference/client.md).

## Choosing between a schedule and an event

A schedule fires on time; an event fires on a fact. If the trigger is "something
happened elsewhere", an event handler avoids the polling interval a schedule imposes.
