# Reference: agent configuration schema

An `Agent` is serialized to an `agentConfig` JSON object and sent to the server, which
compiles it into a Conductor `WorkflowDef`. This page documents that wire format;
[agent-schema.json](agent-schema.json) is the machine-readable version.

Inspect what your agent actually compiles to with a dry run:

```csharp
var workflowDef = await runtime.PlanAsync(agent);   // JsonNode?
```

## Top-level fields

| Field | Type | Source |
|---|---|---|
| `name` | string | `Agent.Name` |
| `model` | string | `Agent.Model` |
| `instructions` | string | `Agent.Instructions` / `InstructionsFn` |
| `instruction` | string | Google ADK adapter (singular) |
| `prompt` | object | `PromptTemplateInstructions` — `{name, variables}` |
| `tools` | array | `Agent.Tools` |
| `agents` | array | Sub-agents, recursively `agentConfig`-shaped |
| `strategy` | string | `handoff`, `sequential`, `parallel`, `router`, `round_robin`, `random`, `swarm`, `manual`, `plan_execute` |
| `router` | object | `Strategy.Router` classifier |
| `maxTurns` / `maxTokens` / `temperature` / `timeoutSeconds` | number | |
| `guardrails` | array | |
| `termination` | object | |
| `handoffs` | array | Swarm triggers |
| `gate` | object | `{text, caseSensitive}` |
| `allowedTransitions` | object | `name -> [names]` |
| `callbacks` | array | Positions: `before_agent`, `after_agent`, `before_model`, `after_model`, `before_tool`, `after_tool` |
| `outputType` / `outputSchema` | object | Structured output |
| `stateful` | bool | Domain-routed workers |
| `enablePlanning` | bool | Prompt preamble only |
| `planner` / `fallback` / `fallbackMaxTurns` | | `Strategy.PlanExecute` slots |
| `external` | bool | |
| `framework` / `config` | string / object | Set by framework adapters (`openai`, `google_adk`) |
| `metadata` | object | |
| `reasoningEffort` / `thinkingConfig` | | Per-run LLM controls |
| `includeContents` / `introduction` / `requiredTools` | | |
| `sessionId` / `media` / `input` | | Run-time inputs, not part of the definition |
| `version` / `rawConfig` | | |

## Tool objects

Each entry in `tools` carries `name`, `description`, `inputSchema`, `toolType`, and
the execution knobs `approvalRequired`, `external`, `timeoutSeconds`, `credentials`,
`stateful`, `retryCount`, `retryDelaySeconds`, `retryPolicy` (`fixed`,
`linear_backoff`, `exponential_backoff`), plus an optional nested `guardrails`.

`toolType` selects the server-side handler — `worker` (a local `[Tool]` method),
`agent_tool`, `human`, `skill`, and the built-in factories. Type-specific payloads
appear alongside: `cliConfig` (`allowedCommands`, `allowShell`, `workingDir`,
`timeout`), `codeExecution` (`allowedLanguages`, `language`, `code`), `taskName`,
`workerNames`, `className`, `arguments`, `optional`.

## Termination objects

`termination` is a tree. Leaves carry a `type`:

| `type` | Fields |
|---|---|
| `text_mention` | `text`, `caseSensitive` |
| `stop_message` | `stopMessage` |
| `max_message` | `maxMessages` |
| `token_usage` | `maxTotalTokens`, `maxPromptTokens`, `maxCompletionTokens` |

Composites use `and` / `or` with a `conditions` array.

## Handoff objects

| `type` | Fields |
|---|---|
| `on_text_mention` | `text`, `target` |
| `on_tool_result` | `toolName`, `target`, `resultContains` |
| `on_condition` | `target` (the predicate stays client-side) |

## Guardrail objects

`guardrailType` (`custom`, regex, LLM), `name`, `position` (`input` / `output`),
`onFail` (`retry`, `raise`, `fix`, `human`), `maxRetries`, plus type-specific fields
such as the regex pattern or LLM policy. Sensitive values may be listed in
`maskedFields`.

## Maintenance

> This schema is maintained by hand against `AgentConfigSerializer` and `AgentDef`.
> Unlike the Java SDK — which verifies its equivalent in CI with a schema generator —
> this repo has no automated check that the schema stays in step with the serializer.
> Treat a discrepancy as a documentation bug and prefer `PlanAsync` output as the
> ground truth.
