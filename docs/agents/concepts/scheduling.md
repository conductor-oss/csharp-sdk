# Scheduling

Cron triggers attach to a deployed agent. The lifecycle API is `runtime.Schedules`
(equivalently `runtime.Client.Schedules`).

## Declarative deploy

```csharp
using Conductor.AI.Scheduling;

var agent = new Agent("eng_digest") { Model = "anthropic/claude-sonnet-4-6", Instructions = "..." };

// Upsert these schedules, prune any others for this agent.
await runtime.DeployAsync(agent, new[]
{
    new Schedule
    {
        Name        = "weekday-9am",
        Cron        = "0 0 9 * * MON-FRI",          // 6-field Quartz (seconds precision)
        Timezone    = "America/Los_Angeles",
        Input       = new Dictionary<string, object?> { ["channel"] = "#eng" },
        Description = "Weekday morning digest",
    },
});
```

### Reconciliation semantics

`DeployAsync(agent, schedules)`:

| `schedules` | Effect |
|---|---|
| `null` | Leaves existing schedules untouched. |
| empty collection | Purges all schedules for the agent. |
| non-empty collection | Upserts those and prunes the rest. |

Pass `Array.Empty<Schedule>()` to clear. Schedule `Name`s are unique per agent; the
SDK prefixes the wire name as `{agent}-{name}`.

## Managing individual schedules

Operations are keyed by the **wire name** returned by `ListAsync` — not the short
name you supplied:

```csharp
IReadOnlyList<ScheduleInfo> infos = await runtime.Schedules.ListAsync(agent.Name);
var wire = infos[0].Name;

await runtime.Schedules.PauseAsync(wire, reason: "cooldown");
var info = await runtime.Schedules.GetAsync(wire);
await runtime.Schedules.ResumeAsync(wire);
string execId = await runtime.Schedules.RunNowAsync(info);
IReadOnlyList<long> nextFires = await runtime.Schedules.PreviewNextAsync("0 0 9 * * MON-FRI", n: 5);
await runtime.Schedules.DeleteAsync(wire);
```

You can also deploy and reconcile in one call on the client:
`await runtime.Client.ScheduleAsync(agent, schedules)`.

## Cron format

`Cron` is a 6-field Quartz expression with seconds precision — note this differs
from 5-field Unix cron. `0 0 9 * * ?` is 9 AM daily; `0 0 9 * * MON-FRI` is 9 AM on
weekdays. Validate an expression without deploying via `PreviewNextAsync`, and
`Schedule.Validate()` throws on bad input.

## Errors

Scheduling raises `ScheduleException` and its subtypes `ScheduleNotFound`,
`ScheduleNameConflict`, and `InvalidCronExpression`.

## Reference

`Schedule`, `ScheduleInfo`, and the `Schedules` methods are tabulated in
[reference/api.md](../reference/api.md#schedule--schedules). For non-agent workflow
scheduling see [../../schedules-events.md](../../schedules-events.md).
