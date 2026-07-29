# Workflows

A workflow is a definition registered on the server. `ConductorWorkflow` builds one in
code.

## Authoring

```csharp
using Conductor.Client;
using Conductor.Definition;
using Conductor.Definition.TaskType;
using Conductor.Executor;

var workflow = new ConductorWorkflow()
    .WithName("my_first_workflow")
    .WithVersion(1)
    .WithOwner("developers@orkes.io")
    .WithTask(new SimpleTask("simple_task_1", "ref_1"))
    .WithTask(new SimpleTask("simple_task_2", "ref_2"));

var configuration = new Configuration();
var workflowExecutor = new WorkflowExecutor(configuration);

workflowExecutor.RegisterWorkflow(workflow, overwrite: true);
var workflowId = workflowExecutor.StartWorkflow(workflow);
```

`SimpleTask(taskType, referenceName)` — the first argument is the task *type* a worker
registers for, the second is the *reference name* unique within this workflow. Two
tasks of the same type in one workflow need distinct reference names.

## Wiring inputs

Reference the workflow's own input, or a prior task's output:

```csharp
var task = new SimpleTask("greet", "greet_ref")
    .WithInput("name", workflow.Input("name"))          // from workflow input
    .WithInput("upstream", "${ref_1.output.result}");   // from another task
```

## Task types

`namespace Conductor.Definition.TaskType`:

| Builder | Purpose |
|---|---|
| `SimpleTask` | A task executed by your worker. |
| `HttpTask` | Server makes an HTTP call. |
| `SwitchTask` | Branch on a value. |
| `ForkJoinTask` / `JoinTask` | Static parallel branches and their join. |
| `DynamicFork` (with `DynamicForkInput`) | Fan out over a runtime-computed list. |
| `DoWhileTask` | Loop while a condition holds. |
| `SubWorkflowTask` | Run another workflow as a task. |
| `DynamicTask` | Task type resolved at run time. |
| `EventTask` | Publish an event. |
| `WaitTask` | Pause for a duration or until a timestamp. |
| `WaitForWebhookTask` | Pause until a webhook arrives. |
| `HumanTask` | Pause for human input. |
| `JavascriptTask` | Inline JS evaluated server-side. |
| `JQTask` | JQ expression over the workflow state. |
| `SetVariableTask` | Set a workflow variable. |
| `TerminateTask` | End the workflow early with a status. |

### LLM tasks

`Conductor.Definition.TaskType.LlmTasks`: `LlmTextComplete`, `LlmChatComplete`,
`LlmGenerateEmbeddings`, `LlmQueryEmbeddings`, `LlmIndexText`, `LlmIndexDocuments`,
`LlmSearchIndex`.

For agentic orchestration rather than individual LLM calls, use the agent layer —
[agents/README.md](agents/README.md).

## Known gaps

Some server task types have no builder in this SDK yet. Track:

- [#160](https://github.com/conductor-oss/csharp-sdk/issues/160) — `NOOP`, `EXCLUSIVE_JOIN`, `START_WORKFLOW` builders
- [#161](https://github.com/conductor-oss/csharp-sdk/issues/161) — `AGENT`, `GET_AGENT_CARD`, `CANCEL_AGENT`, `PULL_WORKFLOW_MESSAGES` enum values
- [#158](https://github.com/conductor-oss/csharp-sdk/issues/158), [#159](https://github.com/conductor-oss/csharp-sdk/issues/159) — `DynamicFork` join and field-name defects

Where a builder is missing you can still register the raw definition through
`MetadataResourceApi`.

## Registration and versioning

`RegisterWorkflow(workflow, overwrite: true)` replaces the definition at that version.
Definitions are versioned, so bumping `WithVersion` leaves running executions on the old
version untouched — this is the safe way to change a workflow that has executions in
flight.

## Starting executions

```csharp
var workflowId = workflowExecutor.StartWorkflow(new StartWorkflowRequest
{
    Name = "greetings",
    Version = 1,
    Input = new Dictionary<string, object> { ["name"] = "Conductor" }
});
```

See [workflow-lifecycle.md](workflow-lifecycle.md) for pause, resume, retry, and
terminate.

## Next

- [workers.md](workers.md) — implementing the tasks
- [workflow-testing.md](workflow-testing.md) — testing definitions
- [api-map.md](api-map.md) — the underlying resource APIs
