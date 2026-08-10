# Schema client

> **Not available in the .NET SDK.** There is no schema registry client in
> `conductor-csharp`. This page exists to keep the documentation structure aligned with
> the sibling SDKs; see the [Python SDK schema client guide](https://github.com/conductor-oss/python-sdk/blob/main/docs/schema-client.md)
> if you need it there.

## What to use instead

### Workflow and task definitions

Definition management is available — it is the *schema registry* specifically that is
absent. Use `MetadataResourceApi`:

```csharp
using Conductor.Api;
using Conductor.Client;

var configuration = new Configuration { BasePath = "http://localhost:8080/api" };
var metadataClient = configuration.GetClient<MetadataResourceApi>();

var workflowDefs = metadataClient.GetAll();
```

See [api-map.md](api-map.md#workflow-and-task-execution).

### Validating payload shapes

For enforcing the shape of data flowing through a workflow:

- **Agent structured output** — set `Agent.OutputType` to a C# type and the server
  enforces the derived JSON schema. See
  [agents/concepts/structured-output.md](agents/concepts/structured-output.md).
- **Tool input schemas** — `ToolDef.InputSchema`, derived automatically from `[Tool]`
  method parameters. See [agents/concepts/tools.md](agents/concepts/tools.md).
- **PLAN_EXECUTE generate steps** — `Generate.OutputSchema` is required and enforced. See
  [agents/concepts/deploy-serve-run.md](agents/concepts/deploy-serve-run.md#plans-and-plan_execute).

### The agent config schema

The wire format for an agent definition is documented, and available as a JSON Schema
document, at [agents/reference/agent-schema.md](agents/reference/agent-schema.md) and
[agents/reference/agent-schema.json](agents/reference/agent-schema.json). That is a
descriptive schema for the SDK's own payload, not a registry client.

## If you need the schema registry

Schemas are stored server-side, so a schema registered from another SDK is visible to the
server-side machinery that consumes it — a .NET workflow can rely on a schema registered
from Python or Java. Only the *client* for managing them is missing here. Manage them from
a sibling SDK, or call the endpoint directly through `ApiClient`.
