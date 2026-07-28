# Server setup

The SDK is a client. You need a Conductor server for anything to run.

## Local, via Docker

```shell
docker run --init -p 8080:8080 conductoross/conductor:latest
```

- UI: `http://localhost:8080`
- API: `http://localhost:8080/api`

This is the default the SDK assumes when `CONDUCTOR_SERVER_URL` is unset. No
authentication is required, so leave `CONDUCTOR_AUTH_KEY` / `CONDUCTOR_AUTH_SECRET`
unset.

## Orkes Cloud

Point `CONDUCTOR_SERVER_URL` at your cluster's API endpoint and supply a key pair:

```shell
export CONDUCTOR_SERVER_URL=https://<your-cluster>.orkesconductor.io/api
export CONDUCTOR_AUTH_KEY=...
export CONDUCTOR_AUTH_SECRET=...
```

See [connection-authentication.md](connection-authentication.md) for how the token
exchange works.

## Server features the SDK depends on

Some SDK capabilities require server-side features to be enabled. These are
**server** properties, not SDK settings — set them on the Conductor deployment:

| Capability | Server requirement |
|---|---|
| `WaitForMessageTool`, `runtime.SendMessageAsync` | `conductor.workflow-message-queue.enabled=true` |
| Local `[Tool]` credential delivery | A server that resolves `runtimeMetadata` at poll time |
| Agent APIs (`/agent/*`) | A server build that exposes the agent control plane |

If a capability is missing, calls fail at the API rather than degrading silently — see
[debugging.md](debugging.md).

## Verifying the connection

```csharp
using Conductor.Api;
using Conductor.Client;

var configuration = new Configuration { BasePath = "http://localhost:8080/api" };
var metadataClient = configuration.GetClient<MetadataResourceApi>();
var workflowDefs = metadataClient.GetAll();
Console.WriteLine($"Server reachable; {workflowDefs.Count} workflow definitions registered.");
```

A connection or auth problem raises `ApiException` here rather than later inside a
worker poll loop, which makes this a useful startup check.

## Which server version

See [compatibility.md](compatibility.md).
