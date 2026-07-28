# Conductor agent env naming, and the Agentspan names we kept

Status: accepted

The AI-agent layer arrived from Agentspan and brought its own naming. Aligning it
with the Java and Python SDKs meant renaming eight runtime environment variables to
`CONDUCTOR_AGENT_*`, but we deliberately stopped short of a complete rebrand: legacy
`AGENTSPAN_*` variables still resolve as fallbacks, and the public type names
`AgentspanException` and `AgentspanJson` keep their names. The end state therefore
looks half-finished, which is why it is written down.

## Why two prefixes

Connection settings stay `CONDUCTOR_SERVER_URL` / `CONDUCTOR_AUTH_KEY` /
`CONDUCTOR_AUTH_SECRET` because they are shared with the core SDK — the same variables
configure a `Configuration` for workflows and workers. The agent runtime knobs take
`CONDUCTOR_AGENT_*` because they configure only the agent layer. The split is not
cosmetic: a single prefix would either imply the core SDK reads worker-liveness knobs,
or that the agent layer owns the connection.

This also matches Java and Python, which matters because agents and workflows are
server-side artifacts shared across SDKs.

## Why the type names stayed

`AgentspanException` is the base of eight public exception types, so renaming it breaks
every `catch (AgentspanException)` in consumer code. `AgentspanJson.Options` appears in
user code that deserializes agent output.

A clean rename is possible — insert `ConductorAgentException` as a new base with
`AgentspanException : ConductorAgentException` marked `[Obsolete]`, so both catch
clauses keep working — but it buys IntelliSense tidiness at the cost of permanent
obsolete surface. We chose API stability and a visibly incomplete rebrand over
churn in the public surface.

## Why the aliases stayed

The Python SDK deleted every `AGENTSPAN_*` read; its config docstring now reads *"Only
`CONDUCTOR_AGENT_*` settings are supported."* We did not follow, because the cost of
keeping a fallback chain is one line per knob and the cost of dropping it is every
existing deployment's configuration.

One subtlety worth preserving: a **blank** current value falls through to the legacy
name, but a **malformed** one does not — it falls back to the built-in default. Without
that asymmetry, a typo in `CONDUCTOR_AGENT_WORKER_THREADS` would silently resurrect a
stale `AGENTSPAN_WORKER_THREADS` value, which is worse than using the default.

## Consequences

- The repo will read as a partially-completed migration for as long as the aliases and
  type names remain. That is intended; this ADR is the answer to "why didn't they
  finish?"
- Examples and docs use only the current names, so new users never learn the legacy
  ones. `docs/upgrading.md` carries the mapping for existing users.
- Externally-owned names containing "agentspan" — the `agentspan` CLI, the
  `agentspan-ai` GitHub org, server properties such as
  `agentspan.default-context-window` — are outside this SDK's control and were left
  untouched. Renaming them in docs would point users at things that may not exist.
- There is no automated check that env-var documentation stays in step with
  `AgentConfig`. See `docs/documentation-parity.md`.
