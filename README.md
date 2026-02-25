# Conductor C# SDK

[![CI](https://github.com/conductor-oss/csharp-sdk/actions/workflows/pull_request.yml/badge.svg)](https://github.com/conductor-oss/csharp-sdk/actions)
[![NuGet](https://img.shields.io/nuget/v/conductor-csharp.svg)](https://www.nuget.org/packages/conductor-csharp/)

The official C# SDK for [Conductor](https://github.com/conductor-oss/conductor) — build task workers, define workflows as code, and orchestrate microservices in C#/.NET.

## Quick Start

### 1. Install the package

```shell
dotnet add package conductor-csharp
```

### 2. Configure the client

```csharp
using Conductor.Client;
using Conductor.Client.Orkes;

var configuration = new Configuration
{
    BasePath = "http://localhost:8080/api",
    AuthenticationSettings = new OrkesAuthenticationSettings("keyId", "keySecret")
};

var clients = new OrkesClients(configuration);
```

### 3. Create a worker

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

### 4. Define a workflow

```csharp
using Conductor.Definition;
using Conductor.Definition.TaskType;
using Conductor.Executor;

var workflow = new ConductorWorkflow()
    .WithName("greet_workflow")
    .WithVersion(1)
    .WithTask(new SimpleTask("greet", "greet_ref")
        .WithInput("name", "${workflow.input.name}"));

var executor = new WorkflowExecutor(configuration);
executor.RegisterWorkflow(workflow, overwrite: true);
```

### 5. Start workers and run

```csharp
using Conductor.Client.Worker;

var host = WorkflowTaskHost.CreateWorkerHost(configuration, new GreetWorker());
await host.StartAsync();

var workflowId = executor.StartWorkflow(workflow);
Console.WriteLine($"Started workflow: {workflowId}");
```

## Features

### High-Level Client Layer

The SDK provides high-level client interfaces for all Conductor APIs:

```csharp
var clients = new OrkesClients(configuration);

IWorkflowClient workflowClient = clients.GetWorkflowClient();
ITaskClient taskClient = clients.GetTaskClient();
IMetadataClient metadataClient = clients.GetMetadataClient();
ISchedulerClient schedulerClient = clients.GetSchedulerClient();
ISecretClient secretClient = clients.GetSecretClient();
IAuthorizationClient authClient = clients.GetAuthorizationClient();
IPromptClient promptClient = clients.GetPromptClient();
IIntegrationClient integrationClient = clients.GetIntegrationClient();
IEventClient eventClient = clients.GetEventClient();
```

### Task Types

The SDK supports all Conductor task types with a fluent builder API:

| Task Type | Class | Description |
|-----------|-------|-------------|
| Simple | `SimpleTask` | Execute a worker |
| HTTP | `HttpTask` | Make HTTP calls |
| HTTP Poll | `HttpPollTask` | Poll HTTP endpoints |
| Inline | `InlineTask` | Execute inline scripts |
| JSON JQ | `JQTask` | JQ transformations |
| Switch | `SwitchTask` | Conditional branching |
| Fork/Join | `ForkJoinTask` | Parallel execution |
| Do-While | `DoWhileTask` / `LoopTask` | Loop execution |
| Sub Workflow | `SubWorkflowTask` | Start child workflows |
| Start Workflow | `StartWorkflowTask` | Start async workflows |
| Wait | `WaitTask` | Wait for duration/signal |
| Human | `HumanTask` | Human-in-the-loop |
| Event | `EventTask` | Publish events |
| Terminate | `TerminateTask` | Terminate workflow |
| Set Variable | `SetVariableTask` | Set workflow variables |
| Kafka Publish | `KafkaPublishTask` | Publish to Kafka |
| Dynamic | `DynamicTask` | Dynamic task routing |

### AI/LLM Orchestration

Build AI-powered workflows with native LLM task types:

```csharp
using Conductor.Definition.TaskType.LlmTasks;

var chatTask = new LlmChatComplete("chat_ref", "openai", "gpt-4",
    new List<ChatMessage>
    {
        new ChatMessage("system", "You are a helpful assistant."),
        new ChatMessage("user", "${workflow.input.question}")
    });
```

Available AI task types:
- `LlmChatComplete` - Chat completion
- `LlmGenerateEmbeddings` - Generate embeddings
- `LlmStoreEmbeddings` - Store embeddings in vector DB
- `LlmSearchEmbeddings` - Search vector DB
- `LlmIndexText` - Index text for retrieval
- `GetDocumentTask` - Fetch documents
- `GenerateImageTask` - Generate images
- `GenerateAudioTask` - Generate audio
- `ListMcpToolsTask` - List MCP tools
- `CallMcpToolTask` - Call MCP tools

Supported providers: OpenAI, Azure OpenAI, GCP Vertex AI, HuggingFace, Anthropic, AWS Bedrock, Cohere, Grok, Mistral, Ollama, Perplexity.

### Worker Framework

Advanced worker features:

- **Exponential backoff** on empty poll queues
- **Auto-restart** on worker failure (configurable retries)
- **Health checks** per worker type
- **Lease extension** for long-running tasks
- **Pause/resume** via environment variables
- **3-tier configuration** (code < global env < worker-specific env)
- **Metrics** collection via `System.Diagnostics.Metrics`

See [Worker Configuration Guide](docs/readme/worker_configuration.md) for details.

### Metrics & Telemetry

The SDK collects metrics using `System.Diagnostics.Metrics`:

```csharp
using Conductor.Client.Telemetry;

// Per-worker metrics
var workerMetrics = new WorkerMetrics("my_task", "worker-1");
workerMetrics.RecordPoll(success: true, taskCount: 3);
workerMetrics.RecordExecution(success: true);

// Listen for metrics
using var listener = new MeterListener();
listener.InstrumentPublished = (instrument, meterListener) =>
{
    if (instrument.Meter.Name == ConductorMetrics.MeterName)
        meterListener.EnableMeasurementEvents(instrument);
};
```

See [Metrics Guide](docs/readme/metrics.md) for the full reference.

### Event System

Monitor worker and workflow lifecycle events:

```csharp
using Conductor.Client.Events;

var dispatcher = EventDispatcher.Instance;
dispatcher.Register(new MyTaskRunnerListener());
dispatcher.Register(new MyWorkflowListener());
```

### Workflow Testing

Test workflows without executing real tasks using `TaskMock`:

```csharp
var testRequest = new WorkflowTestRequest(
    name: "my_workflow",
    version: 1,
    taskRefToMockOutput: new Dictionary<string, List<TaskMock>>
    {
        { "task_ref", new List<TaskMock>
            {
                new TaskMock(status: TaskMock.StatusEnum.COMPLETED,
                    output: new Dictionary<string, object> { { "result", "mocked" } })
            }
        }
    },
    workflowDef: workflow
);

var result = workflowClient.TestWorkflow(testRequest);
```

## Examples

See the [`csharp-examples`](csharp-examples/) directory for comprehensive examples:

| Example | Description |
|---------|-------------|
| [KitchenSink](csharp-examples/Examples/KitchenSink.cs) | All task types in one workflow |
| [WorkflowOps](csharp-examples/Examples/Orkes/WorkflowOps.cs) | Full workflow lifecycle |
| [MetadataJourney](csharp-examples/Examples/Orkes/MetadataJourney.cs) | Metadata CRUD operations |
| [ScheduleJourney](csharp-examples/Examples/Orkes/ScheduleJourney.cs) | Scheduler operations |
| [PromptJourney](csharp-examples/Examples/Orkes/PromptJourney.cs) | Prompt template management |
| [AuthorizationJourney](csharp-examples/Examples/Orkes/AuthorizationJourney.cs) | Authorization APIs |
| [McpAgentExample](csharp-examples/Examples/Orkes/McpAgentExample.cs) | MCP agent workflow |
| [RagPipelineExample](csharp-examples/Examples/Orkes/RagPipelineExample.cs) | RAG pipeline |
| [HumanInLoopChat](csharp-examples/Examples/HumanInLoopChat.cs) | Human-in-the-loop |
| [MultiAgentChat](csharp-examples/Examples/MultiAgentChat.cs) | Multi-agent collaboration |
| [WorkflowTestExample](csharp-examples/Examples/WorkflowTestExample.cs) | Workflow unit testing |
| [WorkerConfiguration](csharp-examples/Examples/WorkerConfigurationExample.cs) | Worker configuration |
| [EventListener](csharp-examples/Examples/EventListenerExample.cs) | Event system |
| [Metrics](csharp-examples/Examples/MetricsExample.cs) | Metrics collection |
| [WorkerDiscovery](csharp-examples/Examples/WorkerDiscoveryExample.cs) | Auto-discover workers |
| [ASP.NET Core](csharp-examples/Examples/AspNetCoreIntegration.cs) | DI and controller patterns |

## Documentation

- [Workers Guide](docs/readme/workers.md) - Creating and running task workers
- [Workflows Guide](docs/readme/workflow.md) - Defining and executing workflows
- [Worker Configuration](docs/readme/worker_configuration.md) - Advanced worker settings
- [Metrics Guide](docs/readme/metrics.md) - Telemetry and monitoring

## Contributing

1. Fork and clone the repository
2. Build: `dotnet build conductor-csharp.sln`
3. Test: `dotnet test Tests/conductor-csharp.test.csproj`
4. Submit a pull request

## License

Apache License 2.0 - see [LICENSE](LICENSE) for details.
