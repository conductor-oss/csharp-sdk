# Upgrading

## The Agentspan → Conductor rename

The agent layer was originally released under the Agentspan name. It is now part of the
Conductor SDK, and the naming has been brought in line — but deliberately not all at
once, and not everywhere.

### Environment variables: renamed, aliases kept

Connection settings:

| Current | Legacy fallback |
|---|---|
| `CONDUCTOR_SERVER_URL` | `AGENTSPAN_SERVER_URL` |
| `CONDUCTOR_AUTH_KEY` | `AGENTSPAN_AUTH_KEY` |
| `CONDUCTOR_AUTH_SECRET` | `AGENTSPAN_AUTH_SECRET` |

Agent runtime knobs:

| Current | Legacy fallback |
|---|---|
| `CONDUCTOR_AGENT_WORKER_THREADS` | `AGENTSPAN_WORKER_THREADS` |
| `CONDUCTOR_AGENT_WORKER_POLL_INTERVAL` | `AGENTSPAN_WORKER_POLL_INTERVAL` |
| `CONDUCTOR_AGENT_AUTO_START_WORKERS` | `AGENTSPAN_AUTO_START_WORKERS` |
| `CONDUCTOR_AGENT_DAEMON_WORKERS` | `AGENTSPAN_DAEMON_WORKERS` |
| `CONDUCTOR_AGENT_STREAMING_ENABLED` | `AGENTSPAN_STREAMING_ENABLED` |
| `CONDUCTOR_AGENT_LIVENESS_ENABLED` | `AGENTSPAN_LIVENESS_ENABLED` |
| `CONDUCTOR_AGENT_LIVENESS_STALL_SECONDS` | `AGENTSPAN_LIVENESS_STALL_SECONDS` |
| `CONDUCTOR_AGENT_LIVENESS_CHECK_INTERVAL_SECONDS` | `AGENTSPAN_LIVENESS_CHECK_INTERVAL_SECONDS` |

**Precedence:** the current name wins when both are set.

For the **agent runtime knobs**, a blank or whitespace-only value is treated as unset, so
`CONDUCTOR_AGENT_WORKER_THREADS=""` with `AGENTSPAN_WORKER_THREADS=2` resolves to `2`. A
*malformed* value (say `not-a-number`) is not treated as unset — it falls back to the
built-in default rather than to the legacy value, so a typo cannot silently pick up stale
configuration.

For the **connection settings**, resolution currently falls back only on an unset
variable, not on a blank one. On Unix, `export CONDUCTOR_SERVER_URL=` yields an empty
`BasePath` rather than falling through to the legacy name or the default. Set connection
variables to a real value or leave them unset.

**No action required.** Existing `AGENTSPAN_*` configuration keeps working. Migrate at
your convenience.

### Two prefixes, on purpose

Connection settings use `CONDUCTOR_*` because they are shared with the core SDK — the
same variables configure a `Configuration` for workflows and workers. Agent runtime knobs
use `CONDUCTOR_AGENT_*` because they configure only the agent layer, and this matches the
Java and Python SDKs.

### Type names: not renamed

`AgentspanException` and `AgentspanJson` keep their names.

`AgentspanException` is the base of every agent exception, so renaming it would break
`catch (AgentspanException)` in consumer code, and `AgentspanJson.Options` appears in
user code that deserializes agent output. Both were left alone in favour of source and
binary compatibility.

```csharp
// still correct
try { await runtime.RunAsync(agent, prompt); }
catch (AgentspanException ex) { /* ... */ }

// still correct
var report = JsonSerializer.Deserialize<WeatherReport>(json, AgentspanJson.Options);
```

The `AgentspanE2eTests` namespace in the E2E test project is likewise unchanged; it is
test-only and not part of the public surface.

## Documentation restructure

The agent documentation moved from a handful of large files into
`docs/agents/{concepts,frameworks,reference}/`, matching the Java and Python SDKs. The old
paths remain as redirect stubs:

| Old | Now |
|---|---|
| `docs/agents/writing-agents.md` | [agents/concepts/](agents/concepts/agents.md) |
| `docs/agents/advanced.md` | [agents/concepts/deploy-serve-run.md](agents/concepts/deploy-serve-run.md) |
| `docs/agents/api-reference.md` | [agents/reference/api.md](agents/reference/api.md) |
| `docs/agents/framework-agents.md` | [agents/frameworks/](agents/frameworks/openai.md) |
| `docs/metrics.md` | [observability.md](observability.md) |
| `docs/readme/workers.md` | [workers.md](workers.md) |
| `docs/readme/workflow.md` | [workflows.md](workflows.md) |

## Earlier changes

The default agent server URL changed from `:6767` to `http://localhost:8080/api`. If you
relied on the old default without setting `CONDUCTOR_SERVER_URL`, set it explicitly.

See [CHANGELOG.md](../CHANGELOG.md) for the full history.
