# Workers

A worker executes a task. Operator and system tasks are handled by the Conductor
server; user-defined tasks need a worker that awaits work scheduled by the server.

The worker framework provides polling threads, metrics, and server communication.

## Design principles

1. Workers are **stateless** and do not implement workflow-specific logic.
2. Each worker executes one specific task and produces well-defined output for given inputs.
3. Workers are **idempotent**, or handle being rescheduled after a partial execution or timeout.
4. Workers do **not** implement retry logic — the server owns that.

These are not style preferences. The server may reschedule a task whose worker died
mid-execution, so a non-idempotent worker produces duplicate side effects.

## Implementing a worker

```csharp
using Conductor.Client.Interfaces;
using Conductor.Client.Models;
using Conductor.Client.Worker;
using Task = Conductor.Client.Models.Task;

public class SimpleWorker : IWorkflowTask
{
    public string TaskType { get; }
    public WorkflowTaskExecutorConfiguration WorkerSettings { get; }

    public SimpleWorker(string taskType = "test-sdk-csharp-task")
    {
        TaskType = taskType;
        WorkerSettings = new WorkflowTaskExecutorConfiguration();
    }

    public TaskResult Execute(Task task)
    {
        return task.Completed();
    }
}
```

`TaskType` must match the task type used in the workflow definition.

## Reading input and writing output

```csharp
public TaskResult Execute(Task task)
{
    var name = task.InputData.GetValueOrDefault("name")?.ToString() ?? "World";

    var result = task.Completed();
    result.OutputData = new Dictionary<string, object>
    {
        ["greeting"] = $"Hello, {name}!"
    };
    return result;
}
```

`ConductorTaskExtensions` provides `task.Completed()`, and the failure equivalents for
reporting a task as failed rather than throwing.

## Starting workers

`WorkflowTaskHost` creates a host and runs the poll loops:

```csharp
using Conductor.Client.Worker;

var host = WorkflowTaskHost.CreateWorkerHost(configuration, new SimpleWorker());
await host.StartAsync();
await host.WaitForShutdownAsync();
```

There is also an overload taking a `LogLevel` instead of a `Configuration`:

```csharp
var host = WorkflowTaskHost.CreateWorkerHost(
    Microsoft.Extensions.Logging.LogLevel.Information,
    new GreetWorker());
```

## Tuning

`WorkflowTaskExecutorConfiguration` on each worker controls its own poll behaviour —
set it per worker rather than globally, since a slow task and a fast task want different
settings. See [deployment-scaling.md](deployment-scaling.md) for sizing guidance.

## Dependency injection

`DependencyInjectionExtensions` registers workers with an
`IServiceCollection`, so workers can take constructor dependencies and participate in
the host's lifetime. This is the preferred shape for anything beyond a sample.

## Metrics

The worker framework records polling, execution, update, and error metrics via
`MetricsCollector`. See [observability.md](observability.md).

## Examples

Integration tests are the most complete worker examples:
[Tests/Worker/WorkerTests.cs](https://github.com/conductor-oss/csharp-sdk/blob/main/Tests/Worker/WorkerTests.cs).
See also [examples.md](examples.md).

## Next

- [workflows.md](workflows.md) — defining the workflows workers serve
- [reliability.md](reliability.md) — timeouts, retries, and failure handling
- [agents/concepts/tools.md](agents/concepts/tools.md) — `[Tool]` methods, the agent-layer equivalent of a worker
