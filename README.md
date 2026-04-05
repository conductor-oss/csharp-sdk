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

   
### Setup Conductor C# Package​

```shell
dotnet add package conductor-csharp
```

## Hello World

This example creates a worker, connects it to a local Conductor server, and runs it end-to-end.

### Step 1: Start a local Conductor server

```shell
docker run --init -p 8080:8080 conductoross/conductor-standalone:latest
```

### Step 2: Define and run a worker

```csharp
using Conductor.Client;
using Conductor.Client.Extensions;
using Conductor.Client.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Define a worker using the [WorkerTask] attribute
[WorkerTask]
public class GreetWorker
{
    [WorkerTask(taskType: "greet", batchSize: 1, pollIntervalMs: 200, workerId: "greeter")]
    public string Greet([InputParam("name")] string name)
    {
        return $"Hello, {name}!";
    }
}

// Connect to local OSS Conductor and start polling
var configuration = new Configuration
{
    BasePath = "http://localhost:8080/api"
};

var host = WorkflowTaskHost.CreateWorkerHost(configuration, LogLevel.Information, new GreetWorker());
await host.StartAsync();
Console.WriteLine("Worker started. Press Ctrl+C to stop.");
await Task.Delay(Timeout.Infinite);
```

### Step 3: Register a workflow and run it

```csharp
using Conductor.Api;
using Conductor.Client;
using Conductor.Definition;
using Conductor.Definition.TaskType;
using Conductor.Executor;

var configuration = new Configuration { BasePath = "http://localhost:8080/api" };

// Build and register the workflow
var workflow = new ConductorWorkflow()
    .WithName("greetings")
    .WithVersion(1)
    .WithTask(new SimpleTask("greet", "greet_ref").WithInput("name", "${workflow.input.name}"));

var executor = new WorkflowExecutor(configuration);
executor.RegisterWorkflow(workflow, overwrite: true);

// Start the workflow
var workflowClient = configuration.GetClient<WorkflowResourceApi>();
var workflowId = workflowClient.StartWorkflow(
    name: "greetings",
    body: new Dictionary<string, object> { ["name"] = "World" },
    version: 1
);

Console.WriteLine($"Started workflow: {workflowId}");
// Open http://localhost:8080 to see the execution in the UI
```

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
