# Conductor AI Agent Examples (.NET)

179 self-contained example projects for the `conductor-ai` agent layer. Each directory is
its own `.csproj` with a single `Program.cs`, and a header comment listing the environment
variables it needs.

## Running one

```shell
export CONDUCTOR_SERVER_URL=http://localhost:8080/api
export CONDUCTOR_AGENT_LLM_MODEL=anthropic/claude-sonnet-4-6

dotnet run --project Conductor.AI.Examples/01_BasicAgent
```

`CONDUCTOR_AGENT_LLM_MODEL` is a convention of these examples (see `Shared/Settings.cs`),
not something the SDK itself reads — an `Agent` takes its model from the `Model` property.

Most examples need a server with a configured **LLM provider**; the provider is configured
on the server, not in your .NET process. See
[../docs/agents/getting-started.md](../docs/agents/getting-started.md) and
[../docs/server-setup.md](../docs/server-setup.md).

The `Build agent examples` CI job compiles every project in this directory, so they stay
buildable. CI does **not** execute them.

## Layout

| Prefix | Count | Covers |
|---|---|---|
| `01`–`118` | 113 | The native agent API |
| `Adk*` | 36 | Google ADK adapter |
| `Sk*` | 20 | Semantic Kernel adapter |
| `OpenAi*` | 10 | OpenAI Agents adapter |
| `Shared/` | — | `Settings.cs`, linked into every project |

Numbering is roughly thematic and increases in complexity. Suffixed variants (`02a`, `16b`,
`63c`) are narrower cuts of the base example.

## Where to start

| Goal | Example |
|---|---|
| Simplest possible agent | `01_BasicAgent` |
| Tools via `[Tool]` methods | `02a_SimpleTools`, `02b_MultiStepTools` |
| Typed results | `03_StructuredOutput` |
| Server-side HTTP / MCP tools | `04_HttpTools`, `04_McpWeather` |
| Multi-agent delegation | `05_Handoffs`, `06_SequentialPipeline`, `07_Parallel*` |
| Guardrails | `21_RegexGuardrails` (server regex), `36_SimpleGuardrails` (custom worker), `22_LlmGuardrails` (server LLM) |
| Human approval | `09_HumanInTheLoop`, `78_ApprovalWorkflow` |
| Streaming | `11_Streaming`, `76_WaitForMessageStreaming` |
| Credentials | `16_Credentials` and its `16b`–`16h` variants |
| Deploy / serve / run-by-name | `63b_Serve`, `63c_RunByName`, `63d_ServeFromAssembly` |
| Stateful agents and message queues | `51b_StatefulAgentWithWaitForMessage`, `75_WaitForMessage`, `83_StatefulResume` |
| PLAN_EXECUTE | `48_Planner`, `108_PlanExecuteRefs`, `115_PlanExecutePlannerContext` |
| Observability | `26_OpenTelemetryTracing`, `80_LiveDashboard` |
| Skills | `91_Skills` |
| Schedules | `92_ScheduledAgent` |
| Scheduler pause/resume verb fallback | `93_SchedulerVerbFallback` |
| Decision SWITCH with INLINE branches | [116_DecisionRouting](116_DecisionRouting/Program.cs) |
| Decision selector routing to chat agents | [117_DecisionRouter](117_DecisionRouter/Program.cs) |
| Chat agent using a decision tool | [118_DecisionTool](118_DecisionTool/Program.cs) |

## Decision examples

These mirror Python SDK PR #511. They require a server with decision inference enabled;
provider credentials belong on the server. No local decision worker is needed.

```shell
export CONDUCTOR_SERVER_URL=http://localhost:8080/api
export CONDUCTOR_DECISION_PROVIDER=openrouter  # optional when the server has a default
dotnet run --project Conductor.AI.Examples/116_DecisionRouting
dotnet run --project Conductor.AI.Examples/116_DecisionRouting -- "The product crashes on startup with error 500. Help troubleshoot it."
dotnet run --project Conductor.AI.Examples/117_DecisionRouter -- --run
dotnet run --project Conductor.AI.Examples/118_DecisionTool
```

The router prints its compiled plan without `--run`. Use `Agent.DecisionRouter` or
`AgentBuilder.WithRouter(ToolDef)` with `ToolType = "decision"` and a config containing
`model`, `questions`, and optional `provider`. The existing `Agent.Router` property
continues to accept an Agent; assigning both router properties is rejected. A decision
selector is emitted as `{name, kind: "decision", model, questions, provider?}` and runs
as one synchronous SWITCH on the server. The standalone tool also executes as a
SWITCH, with no branches.

## Examples with extra prerequisites

Some need more than a server and a model:

| Example | Also needs |
|---|---|
| `04_McpWeather`, `04_HttpAndMcpTools` | An MCP server (CI uses `mcp-testkit` on port 3001) |
| `16*_Credentials*` | A stored secret, and a server that delivers `runtimeMetadata` |
| `24_CodeExecution` | Code execution enabled server-side |
| `77_KafkaConsumerAgent` | A Kafka broker with the `conductor_topic` topic |
| `60*`/`61_GithubCodingAgent*` | A GitHub token |
| `09*`, `32_HumanGuardrail`, `78_ApprovalWorkflow` | Interactive input — they read from stdin |

The HITL examples read stdin, so piping works for non-interactive runs:

```shell
printf 'y\n' | dotnet run --project Conductor.AI.Examples/09_HumanInTheLoop
```

## Related

- [../docs/agents/README.md](../docs/agents/README.md) — the agent documentation index
- [../docs/examples.md](../docs/examples.md) — where every example and test suite lives
- `../Conductor.AI.E2eTests/` — the same features as assertions, run in CI against a live server
