# Workflow lifecycle

What happens to an execution between start and finish, and how to intervene.

## States

An execution is `RUNNING`, then reaches one of `COMPLETED`, `FAILED`, `TERMINATED`,
`TIMED_OUT`, or `PAUSED`. Tasks within it move through `SCHEDULED`, `IN_PROGRESS`, and
then a terminal state of their own.

The distinction that matters operationally: a **`SCHEDULED` task with no worker polling
it** is not an error state. The server is waiting, indefinitely, for someone to claim it.
See [debugging.md](debugging.md#tasks-stuck-in-scheduled).

## Starting

```csharp
var workflowId = executor.StartWorkflow(new StartWorkflowRequest
{
    Name = "greetings",
    Version = 1,
    Input = new Dictionary<string, object> { ["name"] = "Conductor" }
});
```

`CorrelationId` on the request is the field to use for tying an execution back to your
own domain identifier — it is searchable.

## Inspecting

```csharp
using Conductor.Api;

var workflowClient = configuration.GetClient<WorkflowResourceApi>();
var execution = workflowClient.GetExecutionStatus(workflowId);
Console.WriteLine(execution.Status);
```

## Intervening

`WorkflowResourceApi` covers the control operations:

| Operation | Effect |
|---|---|
| `PauseWorkflow` | Stops scheduling new tasks; in-flight tasks finish. |
| `ResumeWorkflow` | Resumes scheduling. |
| `Retry` | Retries the last failed task, keeping history. |
| `Restart` | Starts over from the first task. |
| `Rerun` | Re-executes from a specified task. |
| `Terminate` | Ends the execution immediately with `TERMINATED`. |
| `SkipTaskFromWorkflow` | Marks a task skipped and moves on. |

`WorkflowBulkResourceApi` applies pause, resume, restart, retry, and terminate across
many executions — the right tool after a bad deploy has left hundreds of executions
stuck.

## Sub-workflows

`SubWorkflowTask` runs another workflow as a task. The parent waits for the child; the
child is a first-class execution with its own id, visible and controllable
independently. A terminated child fails the parent task.

## Versioning during a lifecycle

Registering a new version does not migrate running executions. They complete on the
version they started with. This is why bumping `WithVersion` is the safe way to change a
definition — see [workflows.md](workflows.md#registration-and-versioning).

## Timeouts

Timeouts are properties of the task and workflow *definitions*, enforced server-side —
not client settings. A worker that stops responding does not stop the clock. See
[reliability.md](reliability.md).

## Agent executions

An agent run is a workflow execution underneath, so all of the above applies. The agent
layer wraps it in `AgentHandle` with `StopAsync` / `CancelAsync` — see
[agents/concepts/streaming-hitl.md](agents/concepts/streaming-hitl.md#stopping-a-run).
