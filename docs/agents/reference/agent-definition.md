# Reference: `[AgentDef]`

Declarative agent definition on a host object. See
[../concepts/agents.md](../concepts/agents.md#agents-from-methods-agentdef) for usage.

## Attribute properties

| Property | Type | Notes |
|---|---|---|
| `Name` | `string` | Agent name. Must match `^[a-zA-Z_][a-zA-Z0-9_-]*$`. |
| `Model` | `string` | `"provider/model"`. May be left unset and assigned after resolution. |
| `Instructions` | `string` | Static system prompt. |
| `Tools` | `string[]` | `["*"]` = all `[Tool]` methods on the host, `[]` = none, or explicit tool names. |
| `Guardrails` | `string[]` | Same filtering semantics as `Tools`. |
| `Agents` | `string[]` | Sub-agent names, resolved from other `[AgentDef]` methods on the same host. |
| `Strategy` | `Strategy` | Required when `Agents` is non-empty. |
| `MaxTurns` | `int` | |
| `MaxTokens` | `int` | |
| `Temperature` | `double` | |

## Return types

The method's return type determines how much the attribute has to specify:

| Return type | Meaning |
|---|---|
| `void` | Defined entirely by the attribute. |
| `string` | A no-arg method becomes `InstructionsFn` — dynamic instructions re-evaluated on every submit. |
| `Agent` | A full factory; the returned agent is used as-is. |

## Resolution

- `Agent.FromInstance(object host)` → `List<Agent>` — every `[AgentDef]` method.
- `Agent.FromInstance(object host, string name)` → `Agent` — a single one by name.

Sub-agent references in `Agents` are resolved against the same host, so a coordinator
and its children can be declared side by side in one class.

## Interaction with `[Tool]` and `[Guardrail]`

`[Tool]` and `[Guardrail]` methods on the host are attached automatically unless the
`Tools` / `Guardrails` filters narrow them. This is why an `[AgentDef]` with
`Tools = new string[0]` is meaningful — it opts *out* of the host's tools rather than
merely leaving them unset.

## Related

- [api.md](api.md) — `Agent`, `AgentBuilder`, `Strategy`
- [agent-schema.md](agent-schema.md) — the serialized wire format
