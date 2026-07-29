# API map

Every resource API in `Conductor.Api` and what it covers on the server. Obtain one from
a `Configuration`:

```csharp
using Conductor.Api;
using Conductor.Client;

var configuration = new Configuration { BasePath = "http://localhost:8080/api" };
var workflowClient = configuration.GetClient<WorkflowResourceApi>();
```

Each concrete class has a matching interface (`IWorkflowResourceApi` and so on) for
mocking — see [workflow-testing.md](workflow-testing.md).

## Workflow and task execution

| API | Covers |
|---|---|
| `WorkflowResourceApi` | Start, get, search, pause, resume, restart, retry, terminate, rerun executions. |
| `WorkflowBulkResourceApi` | The same operations applied to many executions at once. |
| `TaskResourceApi` | Poll, update, ack, log, and search tasks; queue sizes. |
| `MetadataResourceApi` | Register and fetch workflow and task definitions. |

Prefer `WorkflowExecutor` and `WorkflowTaskHost` over these for ordinary authoring and
worker hosting — see [workflows.md](workflows.md) and [workers.md](workers.md). Reach for
the resource APIs directly for operations the higher-level wrappers don't expose.

## Scheduling and events

| API | Covers |
|---|---|
| `SchedulerResourceApi` | Cron schedules for workflows. |
| `EventResourceApi` | Event handlers and queues. |

See [schedules-events.md](schedules-events.md).

## Human tasks

| API | Covers |
|---|---|
| `HumanTaskResourceApi` | Human task templates, claims, and completion. |

## Secrets, integrations, prompts

| API | Covers |
|---|---|
| `SecretResourceApi` | Named secrets stored server-side. |
| `IntegrationResourceApi` | LLM and vector-DB provider integrations. |
| `PromptResourceApi` | Server-side prompt templates. |
| `EnvironmentResourceApi` | Environment variables available to tasks. |

See [security.md](security.md) for how secrets reach a running task.

## Access control

| API | Covers |
|---|---|
| `AuthorizationResourceApi` | Grants and permissions. |
| `UserResourceApi` | Users. |
| `GroupResourceApi` | Groups. |
| `ApplicationResourceApi` | Applications and their access keys. |
| `TokenResourceApi` | Token exchange. |

`TokenResourceApi` is used internally by `TokenHandler`; you rarely call it directly. See
[connection-authentication.md](connection-authentication.md).

## Metadata and misc

| API | Covers |
|---|---|
| `TagsApi` | Tags on definitions. |
| `MetaResourceApi` | Server metadata. |

## Agent control plane

The `/agent/*` API is not a `Conductor.Api` resource class. It is reached through
`IAgentClient` — see [agents/reference/client.md](agents/reference/client.md).

## Not present in this SDK

| Surface | Status |
|---|---|
| Schema registry client | Not implemented — see [schema-client.md](schema-client.md). |
| File / storage client | Not implemented. The Java SDK has `file-client.md`; there is no .NET equivalent. |
