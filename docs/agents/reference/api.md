# Reference: public API

The public surface of the `conductor-ai` package, one section per type. The
[concepts](../concepts/agents.md) pages show usage; this is the lookup table.

`AgentRuntime` and `AgentConfig` are in [runtime.md](runtime.md); `IAgentClient` is in
[client.md](client.md).

- [Agent](#agent)
- [AgentBuilder](#agentbuilder)
- [Strategy](#strategy)
- [Tools: ToolDef and built-ins](#tools-tooldef-and-built-ins)
- [Guardrails](#guardrails)
- [TerminationCondition](#terminationcondition)
- [Handoff](#handoff)
- [TextGate](#textgate)
- [CallbackHandler](#callbackhandler)
- [Schedule / Schedules](#schedule--schedules)
- [Plans](#plans)
- [Results](#results)
- [Exceptions](#exceptions)

## Agent

`sealed partial class Agent`. Constructor: `Agent(string name)` — name must match
`^[a-zA-Z_][a-zA-Z0-9_-]*$`.

| Member | Type | Notes |
|---|---|---|
| `Name` | `string` (get-only) | |
| `Model` | `string?` | `"provider/model"`. |
| `Instructions` | `string?` | Static system prompt. |
| `InstructionsFn` | `Func<string>?` | Dynamic; takes precedence over `Instructions`. |
| `PromptTemplateInstructions` | `PromptTemplate?` | Server-side template. |
| `Tools` | `List<ToolDef>` | |
| `Agents` | `List<Agent>` | Sub-agents. |
| `Strategy` | `Strategy?` | Required when `Agents` is non-empty. |
| `Router` | `Agent?` | For `Strategy.Router`. |
| `MaxTurns` / `MaxTokens` / `Temperature` / `TimeoutSeconds` | nullable | |
| `Guardrails` | `List<GuardrailDef>` | |
| `Termination` | `TerminationCondition?` | |
| `Handoffs` | `List<Handoff>` | Swarm triggers. |
| `Gate` | `TextGate?` | Sequential-pipeline stop gate. |
| `AllowedTransitions` | `Dictionary<string, List<string>>?` | Constrained transitions. |
| `Callbacks` | `List<CallbackHandler>` | Composable lifecycle handlers. |
| `BeforeAgentCallback` / `AfterAgentCallback` / `BeforeModelCallback` / `AfterModelCallback` / `BeforeToolCallback` / `AfterToolCallback` | `Func<...>?` | Inline delegate hooks. |
| `OutputType` | `Type?` | Structured output. |
| `Stateful` | `bool` | Domain-routed workers. |
| `EnablePlanning` | `bool` | "Plan first" prompt preamble. |
| `Planner` / `Fallback` / `FallbackMaxTurns` / `PlannerContext` | | `Strategy.PlanExecute` slots. |
| `External` | `bool` | |
| `Framework` / `FrameworkConfig` | `string?` / `Dictionary<string,object>?` | Set by framework adapters. |

Operators and statics:

- `operator >>` — `Agent a >> Agent b` builds a `Strategy.Sequential` pipeline.
- `Agent.ScatterGather(name, worker, ...)` — coordinator that fans a worker agent out in parallel.
- `Agent.FromInstance(object)` / `Agent.FromInstance(object, string name)` — resolve `[AgentDef]` methods. See [agent-definition.md](agent-definition.md).

## AgentBuilder

`sealed class AgentBuilder`. Start with `AgentBuilder.Create(string name)`, chain
`With*`, finish with `Build()` — which throws `ConfigurationException` if sub-agents
have no strategy.

`WithModel`, `WithInstructions(string)`, `WithInstructions(Func<string>)`,
`WithInstructions(PromptTemplate)`, `WithTools(params ToolDef[])`,
`WithAgents(params Agent[])`, `WithStrategy`, `WithRouter`, `WithOutputType<T>()`,
`WithMaxTurns`, `WithMaxTokens`, `WithTemperature`, `WithTimeout`, `WithExternal`,
`WithEnablePlanning`, `WithPlanner`, `WithFallback`, `WithFallbackMaxTurns`,
`WithPlannerContext(params Context[])` / `(params string[])`, `WithIncludeContents`,
`WithThinkingBudget`, `WithRequiredTools`, `WithIntroduction`, `WithMetadata`,
`WithHandoffs(params Handoff[])`, `WithGate(TextGate)`,
`WithCallbacks(params CallbackHandler[])`, and the four
`WithBefore/AfterAgent/ToolCallback(...)` delegate setters.

## Strategy

`enum Strategy`: `Handoff`, `Sequential`, `Parallel`, `Router`, `RoundRobin`,
`Random`, `Swarm`, `Manual`, `PlanExecute`.

## Tools: ToolDef and built-ins

**`ToolAttribute`** (`[Tool]`) on a method: `Name`, `Description`,
`ApprovalRequired`, `External`, `TimeoutSeconds`, `Credentials` (`string[]`),
`Stateful`, `RetryCount` (2), `RetryDelaySeconds` (2), `RetryPolicy`. Constructors:
`ToolAttribute()`, `ToolAttribute(string description)`.

**`ToolDef`** — `Name`, `Description`, `InputSchema` (`JsonObject`),
`ApprovalRequired`, `External`, `TimeoutSeconds`, `Credentials`, `Stateful`,
`RetryCount`, `RetryDelaySeconds`, `RetryPolicy`, `Guardrails`. Method
`WithGuardrails(params GuardrailDef[])` returns a copy with guardrails appended.

**`ToolContext`** (record) — injected into tool methods: `SessionId`, `ExecutionId`,
`AgentName`, `Metadata`, `Dependencies`, `State`, `ExecutionToken`.

**`ToolRegistry.FromInstance(object)`** → `List<ToolDef>` (scans `[Tool]` methods).

**`ToolDefFactory.Create(name, description, handler, inputSchema = null, credentials = null)`**
— sync or async `handler` of shape
`(Dictionary<string, JsonElement> args, ToolContext? ctx) -> object?`.

Built-in factories (all return `ToolDef`):

| Factory | Signature highlights |
|---|---|
| `AgentTool.Create` | `(Agent agent, string? name = null, string? description = null, int? retryCount = null, int? retryDelaySeconds = null, bool? optional = null)` |
| `HttpTools.Create` | `(string name, string description, string url, string method = "GET", Dictionary<string,string>? headers = null, JsonObject? inputSchema = null, string[]? credentials = null)` |
| `McpTools.Create` | `(string serverUrl, string? name = null, string? description = null, Dictionary<string,string>? headers = null, List<string>? toolNames = null, int maxTools = 64, string[]? credentials = null)` |
| `RagTools.Index` / `RagTools.Search` | `(string name, string description, string vectorDb, string index, string embeddingModelProvider, string embeddingModel, string namespace = "default_ns", ...)` |
| `MediaTools.Image` / `.Audio` / `.Video` | `(string name, string description, string llmProvider, string model, JsonObject? inputSchema = null, Dictionary<string,object>? extra = null)` |
| `MediaTools.Pdf` | `(string name = "generate_pdf", string description = "...", JsonObject? inputSchema = null, Dictionary<string,object>? extra = null)` |
| `HumanTool.Create` | `(string name = "ask_user", string description = "...", JsonObject? inputSchema = null)` |
| `WaitForMessageTool.Create` | `(string name = "wait_for_message", string description = "...", int batchSize = 1, bool blocking = true)` |
| `ApiTools.Create` | `(string url, string? name = null, string? description = null, Dictionary<string,string>? headers = null, List<string>? toolNames = null, int maxTools = 64, string[]? credentials = null)` |
| `CliTool.Create` | `(IEnumerable<string>? allowedCommands = null, string name = "run_command", int timeoutSeconds = 30, string[]? credentials = null)` |

**`Skill`** (static) — `Skill.Load(path, model?, agentModels?, parameters?, searchPath?)`
loads an agentskills.io skill directory as an `Agent` (requires `SKILL.md`);
`Skill.LoadSkills(path, model?, searchPath?)` → `Dictionary<string, Agent>`;
`Skill.CreateSkillWorkers(agent)` → `IReadOnlyList<SkillWorker>`. Throws
`SkillLoadException`.

## Guardrails

**`GuardrailAttribute`** (`[Guardrail]`): `Name`, `Position` (default `Output`),
`OnFail` (default `Raise`), `MaxRetries` (3).

**`GuardrailDef`** — `Name`, `Position`, `OnFail`, `MaxRetries`.

**`GuardrailResult`** (record) — `(bool Passed, string? Message = null, string? FixedOutput = null)`.

**`GuardrailRegistry.FromInstance(object)`** → `List<GuardrailDef>`.

**`RegexGuardrail.Create`** — `(string|IEnumerable<string> pattern(s), string mode = "block", string? name = null, string? message = null, Position position = Output, OnFail onFail = Raise, int maxRetries = 3)`. `mode`: `"block"` fails on match, `"allow"` fails when nothing matches. Evaluated server-side.

**`LLMGuardrail.Create`** — `(string model, string policy, string? name = null, int? maxTokens = null, Position position = Output, OnFail onFail = Raise, int maxRetries = 3)`. Evaluated server-side — no API key needed in-process.

Enums: `Position` { `Input`, `Output` }; `OnFail` { `Retry`, `Raise`, `Fix`, `Human` }.

## TerminationCondition

`abstract class TerminationCondition` with `operator &` (AND) and `operator |` (OR).

- `TextMentionTermination(string text, bool caseSensitive = false)`
- `StopMessageTermination(string stopMessage)`
- `MaxMessageTermination(int maxMessages)`
- `TokenUsageTermination(int? maxTotalTokens = null, int? maxPromptTokens = null, int? maxCompletionTokens = null)`
- `AndTermination` / `OrTermination` — produced by the operators.

## Handoff

`abstract class Handoff` — `Target` (get-only),
`abstract bool ShouldHandoff(IReadOnlyDictionary<string, object?> context)`.

- `OnTextMention(string text, string target)`; static `OnTextMention.Of(text, target)`.
- `OnToolResult(string toolName, string target, string? resultContains = null)`; statics `.Of(toolName, target)` and `.Of(toolName, target, resultContains)`.
- `OnCondition(string target, Func<IReadOnlyDictionary<string, object?>, bool> condition)`.

Context keys: `result`, `messages`, `tool_name`, `tool_result`.

## TextGate

`sealed class TextGate` — `TextGate(string text, bool caseSensitive = true)`;
properties `Text`, `CaseSensitive`. Stops a sequential pipeline after the agent if its
output contains `Text`.

## CallbackHandler

`abstract class CallbackHandler`. Override any of `OnAgentStart`, `OnAgentEnd`,
`OnModelStart`, `OnModelEnd`, `OnToolStart`, `OnToolEnd` — each
`Dictionary<string, object>? On...(Dictionary<string, JsonElement> kwargs)`. A
non-empty return overrides or short-circuits. Register a list via `Agent.Callbacks`;
handlers run in order and the first non-empty return wins.

Positions map to server task names: `before_agent`, `after_agent`, `before_model`,
`after_model`, `before_tool`, `after_tool`.

## Schedule / Schedules

`namespace Conductor.AI.Scheduling`.

**`Schedule`** (init-only): `Name` (required), `Cron` (required, 6-field Quartz),
`Timezone` (`"UTC"`), `Input` (`IReadOnlyDictionary<string, object?>`), `Catchup`,
`Paused`, `StartAt`, `EndAt`, `Description`. `Validate()` throws on bad input.

**`ScheduleInfo`** (record) — server view: `Name` (wire), `ShortName`, `Agent`, `Cron`,
`Timezone`, `Input`, `Paused`, `PausedReason`, `Catchup`, `StartAt`, `EndAt`,
`Description`, `NextRun`, `CreateTime`, `UpdateTime`, `CreatedBy`, `UpdatedBy`.

**`Schedules`** — `SaveAsync(Schedule, agentName)`, `GetAsync(wireName)`,
`ListAsync(agentName)`, `PauseAsync(wireName, reason?)`, `ResumeAsync(wireName)`,
`DeleteAsync(wireName)`, `RunNowAsync(ScheduleInfo)`,
`PreviewNextAsync(cron, n = 5, startAt?, endAt?)`,
`ReconcileAsync(agentName, IEnumerable<Schedule>?)`. Statics: `Prefix`, `Unprefix`,
`CheckUniqueNames`.

## Plans

`namespace Conductor.AI.Plans`. For `Strategy.PlanExecute`.

- **`Plan`** — `Steps` (`List<Step>`), `Validation`, `OnSuccess`, `OnFailure`. `ToJson()`.
- **`Step(string id)`** — `Operations` (`List<Op>`), `DependsOn` (`List<string>`), `Parallel`.
- **`Op(string tool, Dictionary<string, object?> args)`** (literal); `Op.WithGenerate(string tool, Generate)` (LLM-driven). Exactly one of args/generate.
- **`Generate`** — `Instructions` (required), `OutputSchema` (required), `MaxTokens?`, `Context` (string or `Ref`).
- **`Ref(string stepId)`** — wires a prior step's output (`{"$ref": "stepId"}`); the step must be in `DependsOn`.
- **`Context.FromText(string)`** / **`Context.FromUrl(url, headers? = null, required = true, maxBytes = 16384)`** — planner reference context.
- **`Validation(string tool)`** — `Args`, `SuccessCondition`. **`Action(string tool)`** — `Args`.

## Results

**`AgentResult`** (record): `ExecutionId`, `CorrelationId`, `Output`
(`Dictionary<string, object>?`; final text usually `Output["result"]`), `Messages`,
`ToolCalls`, `Status`, `FinishReason`, `Error`, `TokenUsage`, `Metadata`, `Events`,
`SubResults`. Convenience: `IsSuccess`, `IsFailed`, `IsRejected`, `PrintResult()`.

**`AgentHandle`** — `ExecutionId`, `RunId`, `WaitAsync(ct)`, `StreamAsync(ct)`,
`GetStatusAsync(ct)`, `RespondAsync(object)`, `ApproveAsync()` /
`ApproveAsync(string comment)`, `RejectAsync(string? reason)`, the event-targeted
overloads `ApproveAsync(AgentEvent, ...)` / `RejectAsync(AgentEvent, reason)` /
`RespondAsync(AgentEvent, object)`, `IsWaitingAsync(ct)`,
`WaitUntilWaitingAsync(timeout, pollInterval? = null, ct)`, `StopAsync()` / `Stop()`,
`CancelAsync(reason)` / `Cancel(reason)`, `PauseAsync()` / `Pause()`,
`UnpauseAsync()` / `Unpause()`.

`PauseAsync` stops tasks being scheduled until `UnpauseAsync`; it is distinct from
`AgentRuntime.ResumeAsync`, which re-attaches workers to an existing execution by id.

**`AgentEvent`** (record): `Type` (`EventType`), `Content`, `ToolName`, `Args`,
`Result`, `Target`, `Output`, `ExecutionId`, `GuardrailName`, `Timestamp`, `Status`.

**`AgentStatus`** (record): `ExecutionId`, `IsComplete`, `IsRunning`, `IsWaiting`,
`Output`, `StatusValue`, `Reason`, `CurrentTask`, `PendingTool`, `TokenUsage`.

Enums: `EventType` { `Thinking`, `ToolCall`, `ToolResult`, `GuardrailPass`,
`GuardrailFail`, `Waiting`, `Handoff`, `Message`, `Error`, `Done` }; `Status`
{ `Completed`, `Failed`, `Terminated`, `TimedOut` }; `FinishReason` { `Stop`, `Length`,
`ToolCalls`, `Error`, `Cancelled`, `Timeout`, `Guardrail`, `Rejected` }.

Other records: `TokenUsage(PromptTokens, CompletionTokens, TotalTokens)`,
`DeploymentInfo(RegisteredName, AgentName)`.

## Exceptions

All agent exceptions derive from `AgentspanException`. The name predates the Conductor
rebrand and is retained for source and binary compatibility — see
[../../upgrading.md](../../upgrading.md).

| Exception | Raised when |
|---|---|
| `ConfigurationException` | Invalid agent config, e.g. sub-agents without a strategy. |
| `AgentApiException` | HTTP error from the agent API; carries the status code and body. |
| `AgentNotFoundException` | 404 from a control-plane call. |
| `CredentialNotFoundException` | A tool's declared credential wasn't present in the server-delivered `runtimeMetadata` at poll time. The SDK never falls back to ambient process env. |
| `WorkerStallException` | A stateful run's liveness monitor flagged an unpolled tool task. Carries `TaskReferenceName` and `ExecutionId`. |
| `SSEUnavailableException` | The server rejected an SSE stream connection. |
| `TerminalToolException` | A tool failed unrecoverably. |
| `SkillLoadException` | A skill directory could not be loaded. |

Scheduling throws `ScheduleException` and its subtypes `ScheduleNotFound`,
`ScheduleNameConflict`, `InvalidCronExpression`.
