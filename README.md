# Conductor OSS C# SDK

[![CI](https://github.com/conductor-oss/csharp-sdk/actions/workflows/pull_request.yml/badge.svg)](https://github.com/conductor-oss/csharp-sdk/actions)

The conductor-csharp repository provides the client SDKs to build task workers in C#.

Building the task workers in C# mainly consists of the following steps:

1. Setup `conductor-csharp` package
1. Create and run task workers
1. Create workflows using code
1. API Documentation

## ⭐ Conductor OSS
Show support for the Conductor OSS.  Please help spread the awareness by starring Conductor repo.

[![GitHub stars](https://img.shields.io/github/stars/conductor-oss/conductor.svg?style=social&label=Star&maxAge=)](https://GitHub.com/conductor-oss/conductor/)

## Start Conductor Server

If you don't already have a Conductor server running, start one with Docker:

```shell
docker run --init -p 8080:8080 conductoross/conductor:latest
```

The UI will be available at `http://localhost:8080` and the API at `http://localhost:8080/api`.

## Setup Conductor C# Package

```shell
dotnet add package conductor-csharp
```

## Hello World

This quickstart shows the full flow: define a worker, define a workflow, register it, run it.

**Step 1: Create a new console project**

```shell
dotnet new console -n conductor-hello
cd conductor-hello
dotnet add package conductor-csharp
```

**Step 2: Replace `Program.cs` with the following**

```csharp
using Conductor.Client;
using Conductor.Client.Worker;
using Conductor.Definition;
using Conductor.Definition.TaskType;
using Conductor.Executor;

// Configure the SDK — reads CONDUCTOR_SERVER_URL from the environment,
// or falls back to a local server.
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

// Keep the worker running until Ctrl-C.
await host.WaitForShutdownAsync();
```

**Step 3: Add the worker class — create `GreetWorker.cs`**

```csharp
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

**Step 4: Run it**

```shell
dotnet run
```

Expected output:
```
Started workflow: <workflow-id>
View execution: http://localhost:8080/execution/<workflow-id>
```

Open the UI link to see the completed execution and its output.

## Configurations

### Authentication Settings (Optional)
Configure the authentication settings if your Conductor server requires authentication.
* keyId: Key for authentication.
* keySecret: Secret for the key.

```csharp
authenticationSettings: new OrkesAuthenticationSettings(
    KeyId: "key",
    KeySecret: "secret"
)
```

### Access Control Setup
See [Access Control](https://orkes.io/content/docs/getting-started/concepts/access-control) for more details on role-based access control with Conductor and generating API keys for your environment.

### Configure API Client
```csharp
using Conductor.Api;
using Conductor.Client;
using Conductor.Client.Authentication;

var configuration = new Configuration() {
    BasePath = basePath,
    AuthenticationSettings = new OrkesAuthenticationSettings("keyId", "keySecret")
};

var workflowClient = configuration.GetClient<WorkflowResourceApi>();

workflowClient.StartWorkflow(
    name: "test-sdk-csharp-workflow",
    body: new Dictionary<string, object>(),
    version: 1
)
```

### Next: [Create and run task workers](https://github.com/conductor-oss/csharp-sdk/blob/main/docs/readme/workers.md)
