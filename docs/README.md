# Conductor .NET SDK — Documentation

The .NET client SDK for Conductor/Orkes: workflow authoring, task workers, the REST
client surface, and the durable AI-agent layer.

| Package | Target | Covers |
|---|---|---|
| `conductor-csharp` | `netstandard2.0` | Workflows, workers, REST clients |
| `conductor-ai` | `net8.0` | Durable AI agents |
| `conductor-ai-openai` / `-google-adk` / `-semantic-kernel` | `net8.0` | Framework adapters |

## Start here

| Doc | Covers |
|---|---|
| [core-quickstart.md](core-quickstart.md) | Install, connect, and run a workflow with a worker. |
| [server-setup.md](server-setup.md) | Getting a Conductor server running locally or in the cloud. |
| [connection-authentication.md](connection-authentication.md) | Server URLs, auth keys, token handling. |

## Authoring

| Doc | Covers |
|---|---|
| [workflows.md](workflows.md) | `ConductorWorkflow`, task types, registration. |
| [workers.md](workers.md) | `IWorkflowTask`, the worker host, polling and tuning. |
| [workflow-lifecycle.md](workflow-lifecycle.md) | Start, pause, resume, terminate, retry, sub-workflows. |
| [schedules-events.md](schedules-events.md) | Cron schedules, event handlers, webhooks. |
| [schema-client.md](schema-client.md) | Schema registry — not available in the .NET SDK. |

## Operating

| Doc | Covers |
|---|---|
| [observability.md](observability.md) | Metrics, OpenTelemetry, Prometheus. |
| [reliability.md](reliability.md) | Retries, timeouts, idempotency, failure modes. |
| [deployment-scaling.md](deployment-scaling.md) | Running workers in production, sizing poll loops. |
| [security.md](security.md) | Auth, secrets, credential injection, access control. |
| [debugging.md](debugging.md) | Diagnosing stuck workflows, unpolled tasks, auth failures. |

## Reference

| Doc | Covers |
|---|---|
| [api-map.md](api-map.md) | Every resource API and what it maps to on the server. |
| [compatibility.md](compatibility.md) | Framework targets, server versions, support matrix. |
| [upgrading.md](upgrading.md) | Breaking changes, deprecations, the Agentspan rename. |
| [examples.md](examples.md) | Where the runnable examples live. |
| [workflow-testing.md](workflow-testing.md) | Unit and integration testing workflows and workers. |

## AI agents

The durable AI-agent layer has its own documentation set:
[agents/README.md](agents/README.md).

## Documentation conventions

| Doc | Covers |
|---|---|
| [documentation-standard.md](documentation-standard.md) | How these docs are structured and written. |
| [documentation-parity.md](documentation-parity.md) | Where this SDK's docs stand against the Java and Python SDKs. |

## Design decisions

Architectural decision records live in [adr/](adr/). The domain glossary is
[`CONTEXT.md`](../CONTEXT.md) at the repo root.

| ADR | Decision |
|---|---|
| [0001](adr/0001-conductor-agent-env-naming.md) | `CONDUCTOR_AGENT_*` env naming, retained `AGENTSPAN_*` aliases, and why the `Agentspan*` type names were not renamed. |
