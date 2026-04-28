# Writing Workers with the C# SDK

## Overview

A worker is responsible for executing a task. Operator and system tasks are handled by the Conductor server, while user-defined tasks need a worker that polls the server for work.

The worker framework provides polling, metrics, error handling, health checks, and server communication.

## Design Principles

1. Workers are **stateless** and do not implement workflow-specific logic
2. Each worker executes a **specific task** and produces well-defined output given specific inputs
3. Workers should be **idempotent** (handle cases where a partially executed task gets rescheduled)
4. Workers do not implement retry logic — that is handled by the Conductor server

## Creating a Worker

Implement the `IWorkflowTask` interface:

```csharp
using Conductor.Client.Interfaces;
using Conductor.Client.Models;
using Conductor.Client.Worker;

public class GreetWorker : IWorkflowTask
{
    public string TaskType => "greet";
    public WorkflowTaskExecutorConfiguration WorkerSettings { get; } = new();

    public TaskResult Execute(Task task)
    {
        var name = task.InputData.GetValueOrDefault("name", "World");
        task.OutputData["greeting"] = $"Hello, {name}!";
        return task.Completed();
    }
}
```

### Task Result Status

Return the appropriate status from your worker:

```csharp
// Success
return task.Completed();

// Failure (will retry based on task definition)
return task.Failed("Something went wrong");

// Failure with terminal error (no retry)
return task.FailedWithTerminalError("Unrecoverable error");

// In progress (task will be polled again later)
return task.InProgress("Still processing...");
```

## Starting Workers

Use `WorkflowTaskHost` to manage worker lifecycle:

```csharp
using Conductor.Client;
using Conductor.Client.Worker;

var configuration = new Configuration
{
    BasePath = "http://localhost:8080/api"
};

var host = WorkflowTaskHost.CreateWorkerHost(
    configuration,
    new GreetWorker(),
    new ProcessWorker(),
    new NotifyWorker()
);

await host.StartAsync();
```

## Configuring Workers

Each worker can customize its behavior:

```csharp
public class BatchWorker : IWorkflowTask
{
    public string TaskType => "batch_task";
    public WorkflowTaskExecutorConfiguration WorkerSettings { get; }

    public BatchWorker()
    {
        WorkerSettings = new WorkflowTaskExecutorConfiguration
        {
            BatchSize = 10,                                          // Poll 10 tasks at once
            PollInterval = TimeSpan.FromMilliseconds(500),          // Poll every 500ms
            Domain = "production",                                   // Task domain
            MaxPollBackoffInterval = TimeSpan.FromSeconds(30),      // Max backoff on empty queue
            PollBackoffMultiplier = 2.0,                             // Exponential backoff multiplier
            MaxRestartAttempts = 5,                                  // Auto-restart on failure
            RestartDelay = TimeSpan.FromSeconds(10),                // Delay between restarts
            LeaseExtensionEnabled = true,                            // Extend lease for long tasks
            LeaseExtensionThreshold = TimeSpan.FromSeconds(60),     // Extend if > 60s
        };
    }

    public TaskResult Execute(Task task) => task.Completed();
}
```

See [Worker Configuration Guide](worker_configuration.md) for full details.

## Worker Auto-Discovery

Discover workers by scanning assemblies:

```csharp
using System.Reflection;

var assembly = Assembly.GetExecutingAssembly();
var workerTypes = assembly.GetTypes()
    .Where(t => typeof(IWorkflowTask).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
    .ToList();

foreach (var type in workerTypes)
{
    var worker = (IWorkflowTask)Activator.CreateInstance(type);
    Console.WriteLine($"Found worker: {worker.TaskType}");
}
```

## Annotated Workers

Use attributes for simpler worker definitions:

```csharp
public class AnnotatedWorkers
{
    [WorkerTask("send_email", 5, "default", 200)]
    public static TaskResult SendEmail(Task task)
    {
        // Send email logic
        return task.Completed();
    }
}
```

## Metrics

The worker framework collects metrics automatically:

| Metric | Description |
|--------|-------------|
| `conductor.task.poll.count` | Polls performed per task type |
| `conductor.task.poll.latency` | Time to poll for tasks |
| `conductor.task.execution.count` | Tasks executed (success/failure) |
| `conductor.task.execution.latency` | Task execution time |
| `conductor.task.update.count` | Task status updates sent |
| `conductor.task.update.latency` | Time to update task status |
| `conductor.task.payload.size` | Input/output payload sizes |

See [Metrics Guide](metrics.md) for configuration and export options.

## Health Checks

Monitor worker health at runtime:

```csharp
// Check if all workers are healthy
bool allHealthy = coordinator.IsHealthy();

// Get per-worker health details
var statuses = coordinator.GetHealthStatuses();
```

## Event Listeners

Monitor worker events for logging, alerting, or custom metrics:

```csharp
using Conductor.Client.Events;

public class MyTaskListener : ITaskRunnerEventListener
{
    public void OnPolling(string taskType, string workerId, string domain) { }
    public void OnPollSuccess(string taskType, string workerId, List<Task> tasks) { }
    public void OnPollEmpty(string taskType, string workerId) { }
    public void OnPollError(string taskType, string workerId, Exception ex) { }
    public void OnTaskExecutionStarted(string taskType, Task task) { }
    public void OnTaskExecutionCompleted(string taskType, Task task, TaskResult result) { }
    public void OnTaskExecutionFailed(string taskType, Task task, Exception ex) { }
    public void OnTaskUpdateSent(string taskType, TaskResult result) { }
    public void OnTaskUpdateFailed(string taskType, TaskResult result, Exception ex) { }
}

EventDispatcher.Instance.Register(new MyTaskListener());
```

## Next

- [Workflows Guide](workflow.md) — Define and execute workflows
- [Worker Configuration](worker_configuration.md) — Advanced configuration
- [Metrics Guide](metrics.md) — Telemetry and monitoring
