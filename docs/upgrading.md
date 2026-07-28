# Upgrading

## The Agentspan → Conductor rename

> **Breaking change.** The agent layer was originally released under the Agentspan name.
> All Agentspan naming this SDK owns has now been **removed outright** — there are no
> environment-variable aliases and no `[Obsolete]` type shims. Action is required if you
> set `AGENTSPAN_*` variables or reference the old type names.

### Environment variables

Rename these in every environment, deployment manifest, and CI configuration. The old
names are no longer read and **fail silently** — an unset variable falls back to the
built-in default, so a missed rename shows up as unexpected default behaviour rather than
an error.

Connection settings:

| Old (removed) | New |
|---|---|
| `AGENTSPAN_SERVER_URL` | `CONDUCTOR_SERVER_URL` |
| `AGENTSPAN_AUTH_KEY` | `CONDUCTOR_AUTH_KEY` |
| `AGENTSPAN_AUTH_SECRET` | `CONDUCTOR_AUTH_SECRET` |

Agent runtime knobs:

| Old (removed) | New |
|---|---|
| `AGENTSPAN_WORKER_THREADS` | `CONDUCTOR_AGENT_WORKER_THREADS` |
| `AGENTSPAN_WORKER_POLL_INTERVAL` | `CONDUCTOR_AGENT_WORKER_POLL_INTERVAL` |
| `AGENTSPAN_AUTO_START_WORKERS` | `CONDUCTOR_AGENT_AUTO_START_WORKERS` |
| `AGENTSPAN_DAEMON_WORKERS` | `CONDUCTOR_AGENT_DAEMON_WORKERS` |
| `AGENTSPAN_STREAMING_ENABLED` | `CONDUCTOR_AGENT_STREAMING_ENABLED` |
| `AGENTSPAN_LIVENESS_ENABLED` | `CONDUCTOR_AGENT_LIVENESS_ENABLED` |
| `AGENTSPAN_LIVENESS_STALL_SECONDS` | `CONDUCTOR_AGENT_LIVENESS_STALL_SECONDS` |
| `AGENTSPAN_LIVENESS_CHECK_INTERVAL_SECONDS` | `CONDUCTOR_AGENT_LIVENESS_CHECK_INTERVAL_SECONDS` |

To find stragglers:

```shell
env | grep '^AGENTSPAN_'
grep -rn 'AGENTSPAN_' . --include='*.yml' --include='*.yaml' --include='*.env' --include='Dockerfile*'
```

Blank and whitespace-only values are treated as unset and fall back to the default, rather
than yielding an empty `BasePath` or a failed parse. That applies to a `ServerUrl` passed
explicitly via `AgentRuntimeOptions` too.

### Two prefixes, on purpose

Connection settings use `CONDUCTOR_*` because they are shared with the core SDK — the same
variables configure a `Configuration` for workflows and workers. Agent runtime knobs use
`CONDUCTOR_AGENT_*` because they configure only the agent layer, and this matches the Java
and Python SDKs.

### Type names

| Old (removed) | New |
|---|---|
| `AgentspanException` | `ConductorAgentException` |
| `AgentspanJson` | `ConductorAgentJson` |

Code referencing the old names **will not compile**, which is the intended failure mode —
it is preferable to a silent behaviour change. `ConductorAgentException` remains the base
of every agent exception, so a single `catch` still covers them all.

```csharp
// before
try { await runtime.RunAsync(agent, prompt); }
catch (AgentspanException ex) { /* ... */ }

var report = JsonSerializer.Deserialize<WeatherReport>(json, AgentspanJson.Options);

// after
try { await runtime.RunAsync(agent, prompt); }
catch (ConductorAgentException ex) { /* ... */ }

var report = JsonSerializer.Deserialize<WeatherReport>(json, ConductorAgentJson.Options);
```

### OpenTelemetry source name

The `ActivitySource` name changed from `agentspan.agents` to `conductor.agents`. Code
using the constant is unaffected:

```csharp
.AddSource(AgentTracing.SourceName)   // still correct
```

But **collector configs, dashboards, or alerts that filter on the literal string
`agentspan.agents` will stop matching** and need updating.

### What was deliberately left alone

Names outside this SDK's control still contain "agentspan", and renaming them in docs would
point you at things that may not exist:

- the `agentspan` CLI (`agentspan credentials set …`)
- the `agentspan-ai` GitHub organisation, referenced by credential examples
- server-side properties such as `agentspan.default-context-window`
- the `__agentspan_ctx__` task-input key, which is part of the server's wire contract for
  `ToolContext` injection

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
