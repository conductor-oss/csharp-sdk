# Conductor agent naming: remove Agentspan outright, no aliases

Status: accepted

The AI-agent layer arrived from Agentspan and brought its own naming. We renamed the eight
runtime environment variables to `CONDUCTOR_AGENT_*`, the connection variables to
`CONDUCTOR_*`, and the public types to `ConductorAgentException` / `ConductorAgentJson` —
and we removed the old names **outright**, with no environment-variable aliases and no
`[Obsolete]` type shims. This is a breaking change, chosen deliberately over a compatible
migration.

## Why two prefixes

Connection settings stay `CONDUCTOR_SERVER_URL` / `CONDUCTOR_AUTH_KEY` /
`CONDUCTOR_AUTH_SECRET` because they are shared with the core SDK — the same variables
configure a `Configuration` for workflows and workers. The agent runtime knobs take
`CONDUCTOR_AGENT_*` because they configure only the agent layer. The split is not cosmetic:
a single prefix would either imply the core SDK reads worker-liveness knobs, or that the
agent layer owns the connection.

This also matches Java and Python, which matters because agents and workflows are
server-side artifacts shared across SDKs.

## Why no aliases

A fallback chain costs one line per knob, so the argument for keeping the old names was
real and was in fact the initial decision here. It was reversed in review: a partial
rebrand leaves two names for one setting indefinitely, and every reader has to learn which
is canonical. Python removed its aliases too — despite its PR description claiming
otherwise, its config docstring reads *"Only `CONDUCTOR_AGENT_* settings are supported."*

The cost is borne by existing deployments, which must rename variables. The failure mode is
the unpleasant part and is worth stating plainly: an unrecognised `AGENTSPAN_*` variable is
indistinguishable from an unset one, so a missed rename surfaces as **unexpected default
behaviour, not an error**. `docs/upgrading.md` gives a grep to find stragglers.

## Why the types were renamed too

`ConductorAgentException` is the base of eight public exception types and
`ConductorAgentJson.Options` appears in user code. Renaming both is a source-breaking change.

We considered inserting `ConductorAgentException` as a new base with
`AgentspanException : ConductorAgentException` marked `[Obsolete]`, which would have kept
every existing `catch` compiling. Rejected for the same reason as the env aliases: it leaves
permanent obsolete surface in the public API to spare a one-line edit in consumer code.

A compile error is the right failure mode here — unlike the env vars, this one cannot fail
silently.

## Consequences

- **This must ship as a minor or major version bump, never a patch.**
- The OpenTelemetry `ActivitySource` name changed from `agentspan.agents` to
  `conductor.agents`. Code using `AgentTracing.SourceName` is unaffected, but collector
  configs and dashboards filtering the literal string will stop matching.
- Tests assert the removal *positively* — `LegacyAgentspanName_IsIgnored` and friends — so
  re-introducing a fallback fails the build instead of passing quietly.
- Names outside this SDK's control were left untouched: the `agentspan` CLI, the
  `agentspan-ai` GitHub org, server properties such as
  `agentspan.default-context-window`, and the `__agentspan_ctx__` task-input key, which is
  part of the server's wire contract for `ToolContext` injection. Renaming that last one
  would silently break tool context delivery.
- There is no automated check that env-var documentation stays in step with `AgentConfig`.
  See `docs/documentation-parity.md`.
