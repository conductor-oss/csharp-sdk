# Authoring Workflows with the C# SDK

## Overview

The C# SDK provides a fluent builder API for defining workflows as code. Workflows are composed of tasks that execute in sequence, in parallel, or conditionally.

## A Simple Workflow

```csharp
using Conductor.Client;
using Conductor.Definition;
using Conductor.Definition.TaskType;
using Conductor.Executor;

var workflow = new ConductorWorkflow()
    .WithName("my_first_workflow")
    .WithVersion(1)
    .WithDescription("A simple two-step workflow")
    .WithTask(new SimpleTask("fetch_data", "fetch_ref")
        .WithInput("url", "${workflow.input.url}"))
    .WithTask(new SimpleTask("process_data", "process_ref")
        .WithInput("data", "${fetch_ref.output.result}"));

var configuration = new Configuration { BasePath = "http://localhost:8080/api" };
var executor = new WorkflowExecutor(configuration);

// Register the workflow
executor.RegisterWorkflow(workflow, overwrite: true);

// Start the workflow
var workflowId = executor.StartWorkflow(workflow);
Console.WriteLine($"Started workflow: {workflowId}");
```

## Task Types

### Simple Task (Worker)

```csharp
var task = new SimpleTask("task_name", "task_ref")
    .WithInput("key1", "value1")
    .WithInput("key2", "${workflow.input.param}");
```

### HTTP Task

```csharp
var httpTask = new HttpTask("http_ref", new HttpTaskSettings
{
    uri = "https://api.example.com/data",
    method = "GET"
});
```

### HTTP Poll Task

```csharp
var pollTask = new HttpPollTask("poll_ref", new HttpTaskSettings
{
    uri = "https://api.example.com/status/${workflow.input.jobId}"
});
```

### Inline Task (JavaScript)

```csharp
var inlineTask = new InlineTask("inline_ref",
    "function e() { return $.input_val * 2; } e();");
inlineTask.WithInput("input_val", "${workflow.input.number}");
```

### JSON JQ Transform

```csharp
var jqTask = new JQTask("jq_ref", ".data | map(select(.active)) | length")
    .WithInput("data", "${fetch_ref.output.response.body}");
```

### Switch (Decision)

```csharp
var switchTask = new SwitchTask("switch_ref", "${workflow.input.action}");

switchTask.WithDecisionCase("approve",
    new SimpleTask("approve_task", "approve_ref"));

switchTask.WithDecisionCase("reject",
    new SimpleTask("reject_task", "reject_ref"));

// Default case
switchTask.WithDefaultCase(
    new SimpleTask("default_task", "default_ref"));
```

### Fork/Join (Parallel)

```csharp
var sendEmail = new SimpleTask("send_email", "email_ref");
var sendSms = new SimpleTask("send_sms", "sms_ref");
var sendSlack = new SimpleTask("send_slack", "slack_ref");

var fork = new ForkJoinTask("fork_ref",
    new WorkflowTask[] { sendEmail },
    new WorkflowTask[] { sendSms },
    new WorkflowTask[] { sendSlack });
```

### Do-While Loop

```csharp
// Fixed iteration count
var loop = new LoopTask("loop_ref", 10,
    new SimpleTask("process_item", "item_ref"));

// Custom condition
var doWhile = new DoWhileTask("dowhile_ref",
    "if ($.dowhile_ref['iteration'] < $.maxIterations) { true; } else { false; }",
    new SimpleTask("process", "process_ref"));
```

### Sub Workflow

```csharp
// Reference by name
var sub = new SubWorkflowTask("sub_ref",
    new SubWorkflowParams(name: "child_workflow", version: 1));

// Inline definition
var childWorkflow = new ConductorWorkflow()
    .WithName("inline_child")
    .WithVersion(1)
    .WithTask(new SimpleTask("child_task", "child_ref"));

var subInline = new SubWorkflowTask("sub_ref", childWorkflow);
```

### Start Workflow (Async)

```csharp
var startWf = new StartWorkflowTask("start_ref", "async_workflow", 1,
    input: new Dictionary<string, object>
    {
        { "param", "${workflow.input.param}" }
    });
```

### Wait Task

```csharp
// Wait for duration
var waitDuration = new WaitTask("wait_ref", TimeSpan.FromMinutes(5));

// Wait until specific time
var waitUntil = new WaitTask("wait_ref", "2024-12-31T23:59:59Z");
```

### Human Task

```csharp
var humanTask = new HumanTask("review_ref",
    displayName: "Review Document",
    formTemplate: "doc_review_form",
    formVersion: 1);
```

### Kafka Publish

```csharp
var kafka = new KafkaPublishTask("kafka_ref",
    "localhost:9092", "my-topic", "${workflow.input.message}");
```

### Terminate

```csharp
var terminate = new TerminateTask("term_ref",
    WorkflowStatus.StatusEnum.COMPLETED,
    "Workflow completed successfully");
```

## Workflow Operations

Using the high-level client:

```csharp
using Conductor.Client.Orkes;

var clients = new OrkesClients(configuration);
var workflowClient = clients.GetWorkflowClient();

// Start
var req = new StartWorkflowRequest(name: "my_workflow", version: 1);
var workflowId = workflowClient.StartWorkflow(req);

// Get status
var wf = workflowClient.GetWorkflow(workflowId, includeTasks: true);
Console.WriteLine($"Status: {wf.Status}");

// Pause / Resume
workflowClient.PauseWorkflow(workflowId);
workflowClient.ResumeWorkflow(workflowId);

// Terminate
workflowClient.Terminate(workflowId, "Reason for termination");

// Restart
workflowClient.Restart(workflowId, useLatestDefinitions: true);

// Retry
workflowClient.Retry(workflowId);

// Search
var results = workflowClient.Search(query: "workflowType='my_workflow'", start: 0, size: 10);
```

## Workflow Testing

Test workflows with mock task outputs:

```csharp
var testRequest = new WorkflowTestRequest(
    name: "my_workflow",
    version: 1,
    workflowDef: workflow,
    taskRefToMockOutput: new Dictionary<string, List<TaskMock>>
    {
        { "fetch_ref", new List<TaskMock>
            {
                new TaskMock(
                    status: TaskMock.StatusEnum.COMPLETED,
                    output: new Dictionary<string, object>
                    {
                        { "result", new { data = "test data" } }
                    })
            }
        }
    }
);

var result = workflowClient.TestWorkflow(testRequest);
Assert.Equal(Workflow.StatusEnum.COMPLETED, result.Status);
```

## AI/LLM Workflows

### Chat Completion

```csharp
var chatTask = new LlmChatComplete("chat_ref", "openai", "gpt-4",
    new List<ChatMessage>
    {
        new ChatMessage("system", "You are a helpful assistant."),
        new ChatMessage("user", "${workflow.input.question}")
    },
    maxTokens: 500,
    temperature: 0);
```

### RAG Pipeline

```csharp
// Ingest: fetch document and store embeddings
var getDoc = new GetDocumentTask("get_doc", "${workflow.input.url}", "application/pdf");
var storeEmb = new LlmStoreEmbeddings("store_emb", "pinecone", "my-index", "docs",
    new EmbeddingModel("openai", "text-embedding-ada-002"),
    "${get_doc.output.result}", "doc-1");

// Query: search embeddings and generate response
var searchEmb = new LlmSearchEmbeddings("search_emb", "pinecone", "my-index", "docs",
    new EmbeddingModel("openai", "text-embedding-ada-002"),
    "${workflow.input.query}");

var generate = new LlmChatComplete("generate", "openai", "gpt-4",
    new List<ChatMessage>
    {
        new ChatMessage("system", "Answer using this context: ${search_emb.output.result}"),
        new ChatMessage("user", "${workflow.input.query}")
    });
```

### MCP Agent

```csharp
var listTools = new ListMcpToolsTask("list_tools", "weather-server");
var callTool = new CallMcpToolTask("call_tool", "weather-server", "get_weather",
    new Dictionary<string, object> { { "city", "San Francisco" } });
```

## Next

- [Workers Guide](workers.md) — Creating and running task workers
- [Worker Configuration](worker_configuration.md) — Advanced worker settings
- [Metrics Guide](metrics.md) — Telemetry and monitoring
