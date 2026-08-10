# Tools

A tool is a named capability an agent can invoke to act beyond generating text.

## `[Tool]` methods + `ToolRegistry.FromInstance`

Decorate public methods with `[Tool]` and scan an instance. Method names become
`snake_case` tool names (`GetWeather` → `get_weather`). Parameters become the
input schema; a `ToolContext` parameter (if present) is injected, not exposed to
the LLM.

```csharp
internal sealed class WeatherTools
{
    [Tool("Get the current weather for a city.")]
    public Dictionary<string, object> GetWeather(string city)
        => new() { ["city"] = city, ["temp_f"] = 72, ["condition"] = "Sunny" };

    [Tool("Send an email.", ApprovalRequired = true, TimeoutSeconds = 60)]
    public Dictionary<string, object> SendEmail(string to, string subject, string body)
        => new() { ["sent"] = true };
}

var tools = ToolRegistry.FromInstance(new WeatherTools());
var agent = new Agent("assistant") { Tools = tools };
```

`[Tool]` attribute knobs: `Name`, `Description`, `ApprovalRequired`, `External`,
`TimeoutSeconds`, `Credentials` (`string[]`), `Stateful`, `RetryCount` (default 2),
`RetryDelaySeconds` (default 2), `RetryPolicy` (`"fixed"` / `"linear_backoff"` /
`"exponential_backoff"`).

Local `[Tool]` methods run in a worker the runtime hosts for you — so agents with
local tools must run via `AgentRuntime`, not the bare `IAgentClient`. See
[deploy-serve-run.md](deploy-serve-run.md).

Mix scanned tools with built-ins via list spreads:

```csharp
var agent = new Agent("a") { Tools = [.. tools, httpTool, askUser] };
```

## Custom tool defs without attributes

```csharp
var t = ToolDefFactory.Create(
    name:        "submit_answer",
    description: "Submit the final answer.",
    handler:     (args, ctx) => new { ok = true });   // sync or async
```

## Built-in tool factories

All of the following are server-side (no local worker process) unless noted.

**HTTP** — the Conductor server makes the call:

```csharp
var reverse = HttpTools.Create(
    name:        "reverse_string",
    description: "Reverse a string via the HTTP API.",
    url:         "http://localhost:3001/api/string/reverse",
    method:      "POST",
    headers:     new() { ["Authorization"] = "Bearer ${HTTP_TEST_API_KEY}" },
    credentials: ["HTTP_TEST_API_KEY"]);
```

**MCP** — tools discovered from an MCP server:

```csharp
var mcp = McpTools.Create(
    serverUrl:   "http://localhost:3001/mcp",
    name:        "weather_mcp",
    description: "Weather tools via MCP.",
    headers:     new() { ["Authorization"] = "Bearer ${MCP_TEST_API_KEY}" },
    credentials: ["MCP_TEST_API_KEY"]);
```

**HumanTool** — pauses the workflow for human input when the LLM calls it:

```csharp
var askUser = HumanTool.Create(
    name:        "ask_user",
    description: "Ask the user a question when you need clarification.");
```

**MediaTools** — image / audio / video / PDF generation:

```csharp
var image = MediaTools.Image("generate_image", "Generate an image.", llmProvider: "openai", model: "dall-e-3");
var audio = MediaTools.Audio("text_to_speech", "Convert text to speech.", llmProvider: "openai", model: "tts-1");
var video = MediaTools.Video("generate_video", "Generate a video.", llmProvider: "...", model: "...");
var pdf   = MediaTools.Pdf();   // generate_pdf from markdown; sensible defaults
```

> `PdfTool` is `MediaTools.Pdf(...)`.

**WaitForMessageTool** — dequeues messages from the Workflow Message Queue
(server-side). Pair with `runtime.SendMessageAsync(...)` and `Stateful = true`
(see [stateful.md](stateful.md)):

```csharp
var receive = WaitForMessageTool.Create(
    name: "wait_for_message",
    description: "Wait for the next external message, then return its content.");
```

**AgentTool** — wrap an `Agent` as a callable tool (runs as a sub-workflow, called
inline like a function — distinct from handoff delegation):

```csharp
var manager = new Agent("manager")
{
    Tools = [ AgentTool.Create(researcher), .. ToolRegistry.FromInstance(new CalculatorTools()) ],
};
```

**RagTools** — vector-DB index and search (server-side embedding + storage):

```csharp
var index  = RagTools.Index("index_docs", "Index documents.",
                vectorDb: "pinecone", index: "kb",
                embeddingModelProvider: "openai", embeddingModel: "text-embedding-3-small");
var search = RagTools.Search("search_docs", "Search the knowledge base.",
                vectorDb: "pinecone", index: "kb",
                embeddingModelProvider: "openai", embeddingModel: "text-embedding-3-small",
                maxResults: 5);
```

Other built-ins: `ApiTools.Create(...)` (tools from an OpenAPI/Swagger/Postman
spec) and `CliTool.Create(...)` (a local `run_command` worker tool with a command
whitelist).

## Credentials

Tools declare the credential names they need; the server resolves them at run time
and injects them so the value never lives in your agent definition. Reference a
secret with the `${NAME}` placeholder in HTTP/MCP/API headers, or list names on a
`[Tool]`.

```csharp
[Tool("List public repositories for a GitHub user.", Credentials = ["GITHUB_TOKEN"])]
public async Task<Dictionary<string, object>> ListGithubRepos(string username, ToolContext? ctx = null)
{
    var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN") ?? "";
    // ...
}
```

Local `[Tool]` credentials ride the `runtimeMetadata` wire contract: the names
listed in a tool's `Credentials` are stamped onto that worker's
`TaskDef.RuntimeMetadata` at every registration, and a capable server resolves and
delivers the values on the wire-only `Task.RuntimeMetadata` at poll time — they
never live in the task's regular `inputData`. Dispatch is **fail-closed**: if a
declared credential is missing from the delivered metadata, the SDK raises
`CredentialNotFoundException` and the tool task terminates. It never falls back to
reading the ambient process environment.

See [../../security.md](../../security.md) for the full credential model.

## Tool-scoped guardrails

Scope a guardrail to a single tool (input or output of that tool):

```csharp
var t = someToolDef.WithGuardrails(noEmails);
```

See [guardrails.md](guardrails.md).

## Reference

[reference/api.md](../reference/api.md) has the full signature table for every
built-in factory.
