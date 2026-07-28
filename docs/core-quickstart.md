# Core quickstart

Define a worker, define a workflow, register it, run it.

## 1. Install

```shell
dotnet new console -n conductor-hello
cd conductor-hello
dotnet add package conductor-csharp
```

## 2. Start a server

```shell
docker run --init -p 8080:8080 conductoross/conductor:latest
```

The UI is at `http://localhost:8080` and the API at `http://localhost:8080/api`. See
[server-setup.md](server-setup.md) for other options.

## 3. Write the program

`Program.cs`:

```csharp
using Conductor.Client;
using Conductor.Client.Extensions;
using Conductor.Client.Worker;
using Conductor.Definition;
using Conductor.Definition.TaskType;
using Conductor.Executor;

// Reads CONDUCTOR_SERVER_URL from the environment, or falls back to a local server.
var configuration = new Configuration
{
    BasePath = Environment.GetEnvironmentVariable("CONDUCTOR_SERVER_URL")
               ?? "http://localhost:8080/api"
};

// Define the workflow: one SIMPLE task called "greet".
var workflow = new ConductorWorkflow()
    .WithName("greetings")
    .WithVersion(1);

var greetTask = new SimpleTask("greet", "greet_ref")
    .WithInput("name", workflow.Input("name"));
workflow.WithTask(greetTask);

// Register the workflow definition on the server.
var executor = new WorkflowExecutor(configuration);
executor.RegisterWorkflow(workflow, overwrite: true);

// Start the worker host — it discovers GreetWorker automatically.
var host = WorkflowTaskHost.CreateWorkerHost(
    Microsoft.Extensions.Logging.LogLevel.Information,
    new GreetWorker());
await host.StartAsync();

// Run the workflow and print the execution ID.
var workflowId = executor.StartWorkflow(new StartWorkflowRequest
{
    Name = "greetings",
    Version = 1,
    Input = new Dictionary<string, object> { ["name"] = "Conductor" }
});
Console.WriteLine($"Started workflow: {workflowId}");
Console.WriteLine($"View execution: http://localhost:8080/execution/{workflowId}");

await host.WaitForShutdownAsync();
```

`GreetWorker.cs`:

```csharp
using Conductor.Client.Extensions;
using Conductor.Client.Interfaces;
using Conductor.Client.Models;
using Conductor.Client.Worker;
using Task = Conductor.Client.Models.Task;

public class GreetWorker : IWorkflowTask
{
    public string TaskType => "greet";
    public WorkflowTaskExecutorConfiguration WorkerSettings { get; } = new();

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
}
```

## 4. Run

```shell
dotnet run
```

```
Started workflow: <workflow-id>
View execution: http://localhost:8080/execution/<workflow-id>
```

Open the UI link to see the completed execution and its output.

## What just happened

1. **`ConductorWorkflow`** built a workflow definition in code — see [workflows.md](workflows.md).
2. **`RegisterWorkflow`** pushed that definition to the server. Definitions are versioned; `overwrite: true` replaces version 1.
3. **`WorkflowTaskHost`** started poll loops that claim `greet` tasks — see [workers.md](workers.md).
4. **`StartWorkflow`** created an execution. The server scheduled `greet`, your worker polled it, executed, and reported back.

The server owns scheduling, retries, and state. The worker only executes.

## Next

- [workflows.md](workflows.md) — the full task-type catalog
- [workers.md](workers.md) — worker tuning and design principles
- [agents/README.md](agents/README.md) — the durable AI-agent layer
