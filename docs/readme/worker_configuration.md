# Worker Configuration Guide

## Overview

The C# SDK provides a comprehensive worker framework with configurable polling, error handling, health checks, and metrics. This guide covers all configuration options.

## Basic Worker Configuration

Every worker implements `IWorkflowTask` and can configure its behavior via `WorkflowTaskExecutorConfiguration`:

```csharp
public class MyWorker : IWorkflowTask
{
    public string TaskType => "my_task";
    public WorkflowTaskExecutorConfiguration WorkerSettings { get; }

    public MyWorker()
    {
        WorkerSettings = new WorkflowTaskExecutorConfiguration
        {
            BatchSize = 5,
            PollInterval = TimeSpan.FromMilliseconds(100),
            Domain = "my-domain"
        };
    }

    public TaskResult Execute(Task task)
    {
        // Worker logic
        return task.Completed();
    }
}
```

## Configuration Properties

| Property | Default | Description |
|----------|---------|-------------|
| `BatchSize` | 1 | Number of tasks to poll in each batch |
| `PollInterval` | 100ms | Interval between poll requests |
| `Domain` | null | Task domain for routing |
| `MaxPollBackoffInterval` | 10s | Maximum backoff when queue is empty |
| `PollBackoffMultiplier` | 2.0 | Multiplier for exponential backoff |
| `MaxConsecutiveErrors` | 10 | Max errors before marking unhealthy |
| `MaxRestartAttempts` | 3 | Max auto-restart attempts on failure |
| `RestartDelay` | 5s | Delay between restart attempts |
| `LeaseExtensionEnabled` | false | Enable lease extension for long tasks |
| `LeaseExtensionThreshold` | 30s | Time threshold to trigger extension |
| `PauseEnvironmentVariable` | null | Env var name to pause worker |
| `PauseCheckInterval` | 5s | How often to check pause state |

## Exponential Backoff

When a poll returns no tasks, the worker automatically backs off to reduce server load:

```csharp
WorkerSettings = new WorkflowTaskExecutorConfiguration
{
    PollInterval = TimeSpan.FromMilliseconds(100),         // Initial interval
    MaxPollBackoffInterval = TimeSpan.FromSeconds(10),     // Max backoff
    PollBackoffMultiplier = 2.0                            // Double each time
};
```

The backoff sequence: 100ms → 200ms → 400ms → 800ms → 1.6s → 3.2s → 6.4s → 10s (capped).
On successful poll, the interval resets to the base value.

## Auto-Restart

Workers automatically restart on unhandled exceptions:

```csharp
WorkerSettings = new WorkflowTaskExecutorConfiguration
{
    MaxRestartAttempts = 5,
    RestartDelay = TimeSpan.FromSeconds(10)
};
```

After `MaxRestartAttempts` consecutive failures, the worker stops and is marked unhealthy.

## Lease Extension

For long-running tasks, enable lease extension to prevent timeouts:

```csharp
WorkerSettings = new WorkflowTaskExecutorConfiguration
{
    LeaseExtensionEnabled = true,
    LeaseExtensionThreshold = TimeSpan.FromSeconds(60) // Extend if task runs > 60s
};
```

## Pause/Resume via Environment Variables

Workers can be paused at runtime without restarting:

```csharp
WorkerSettings = new WorkflowTaskExecutorConfiguration
{
    PauseEnvironmentVariable = "CONDUCTOR_WORKER_PAUSED",
    PauseCheckInterval = TimeSpan.FromSeconds(5)
};
```

Set the environment variable to `"true"` to pause, remove or set to `"false"` to resume.

## 3-Tier Configuration

Configuration is applied in layers (later layers override earlier ones):

1. **Code** — Values set in `WorkflowTaskExecutorConfiguration`
2. **Global environment** — `CONDUCTOR_WORKER_*` variables
3. **Worker-specific environment** — `CONDUCTOR_WORKER_{taskType}_*` variables

```csharp
var config = new WorkflowTaskExecutorConfiguration();

// Apply global overrides from environment
config.ApplyEnvironmentOverrides();

// Apply task-specific overrides
config.ApplyEnvironmentOverrides("my_task_type");
```

### Environment Variables

| Variable | Config Property |
|----------|----------------|
| `CONDUCTOR_WORKER_POLL_INTERVAL` | PollInterval (ms) |
| `CONDUCTOR_WORKER_BATCH_SIZE` | BatchSize |
| `CONDUCTOR_WORKER_DOMAIN` | Domain |

Prefix with task type for worker-specific overrides:
- `CONDUCTOR_WORKER_my_task_POLL_INTERVAL=5000`
- `CONDUCTOR_WORKER_my_task_BATCH_SIZE=10`

## Health Checks

Monitor worker health status:

```csharp
var coordinator = /* get from WorkflowTaskHost */;

// Check overall health
bool healthy = coordinator.IsHealthy();

// Get per-worker status
var statuses = coordinator.GetHealthStatuses();
foreach (var status in statuses)
{
    Console.WriteLine($"Worker: {status.Key}");
    Console.WriteLine($"  Healthy: {status.Value.IsHealthy}");
    Console.WriteLine($"  Running Workers: {status.Value.RunningWorkers}");
    Console.WriteLine($"  Total Tasks: {status.Value.TotalTasksProcessed}");
    Console.WriteLine($"  Errors: {status.Value.TotalTaskErrors}");
    Console.WriteLine($"  Last Poll: {status.Value.LastPollTime}");
}
```

## Worker Host

The `WorkflowTaskHost` manages the lifecycle of all workers:

```csharp
var host = WorkflowTaskHost.CreateWorkerHost(
    configuration,
    new GreetWorker(),
    new ProcessWorker(),
    new NotifyWorker()
);

await host.StartAsync();

// Graceful shutdown
await host.StopAsync();
```

## ASP.NET Core Integration

Register workers as a background service:

```csharp
// In Program.cs
builder.Services.AddSingleton<Configuration>(sp => new Configuration
{
    BasePath = "http://conductor:8080/api"
});

builder.Services.AddHostedService<ConductorWorkerService>();

// BackgroundService implementation
public class ConductorWorkerService : BackgroundService
{
    private readonly WorkflowTaskHost _host;

    public ConductorWorkerService(Configuration config)
    {
        _host = WorkflowTaskHost.CreateWorkerHost(config, new MyWorker());
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _host.StartAsync(stoppingToken);
    }
}
```
