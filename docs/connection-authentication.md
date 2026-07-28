# Connection and authentication

## Configuration

`Conductor.Client.Configuration` is the single connection object. Every client and the
agent runtime are built from one:

```csharp
using Conductor.Client;

var configuration = new Configuration
{
    BasePath = "http://localhost:8080/api"
};
```

`BasePath` includes the `/api` suffix. The default is `http://localhost:8080/api`.

## Environment variables

| Variable | Used by | Notes |
|---|---|---|
| `CONDUCTOR_SERVER_URL` | Core SDK, agent runtime | Server URL including `/api`. |
| `CONDUCTOR_AUTH_KEY` | Core SDK, agent runtime | Key id. Unset means no-auth mode. |
| `CONDUCTOR_AUTH_SECRET` | Core SDK, agent runtime | Key secret. Set together with the key. |

The legacy `AGENTSPAN_SERVER_URL` / `AGENTSPAN_AUTH_KEY` / `AGENTSPAN_AUTH_SECRET`
names are still honored as fallbacks when the `CONDUCTOR_*` equivalents are unset, and the
`CONDUCTOR_*` names win when both are present. See [upgrading.md](upgrading.md).

Blank and whitespace-only values are treated as unset at every step, so
`export CONDUCTOR_SERVER_URL=` falls through to the legacy name and then to the default
rather than yielding an empty `BasePath`. The same applies to a `ServerUrl` passed
explicitly via `AgentRuntimeOptions`.

## Authenticated connections

For a server that requires authentication — Orkes Cloud, or any deployment with access
control enabled — supply `OrkesAuthenticationSettings`:

```csharp
using Conductor.Api;
using Conductor.Client;
using Conductor.Client.Authentication;

var configuration = new Configuration
{
    BasePath = basePath,
    AuthenticationSettings = new OrkesAuthenticationSettings("keyId", "keySecret")
};

var workflowClient = configuration.GetClient<WorkflowResourceApi>();

workflowClient.StartWorkflow(
    name: "test-sdk-csharp-workflow",
    body: new Dictionary<string, object>(),
    version: 1);
```

`TokenHandler` exchanges the key/secret pair for a JWT and refreshes it as needed. The
token cache lives on the `Configuration`, so **reuse one `Configuration`** across
clients rather than constructing one per call — otherwise each gets its own cache and
performs its own token exchange.

## Sharing a Configuration with the agent runtime

```csharp
await using var runtime = new AgentRuntime(myConfiguration, AgentConfig.FromEnv());
```

This shares the token cache with every other client built from `myConfiguration`.
`Configuration.GetAgentClient()` does the same for the control-plane client alone. See
[agents/concepts/deploy-serve-run.md](agents/concepts/deploy-serve-run.md#runtime-initialization).

## Agent runtime options

The agent runtime can also take connection settings directly, bypassing the
environment:

```csharp
await using var runtime = new AgentRuntime(new AgentRuntimeOptions
{
    ServerUrl  = "https://my-server.example.com/api",
    AuthKey    = "...",
    AuthSecret = "...",
});
```

When both `AuthKey` and `AuthSecret` are set, worker polling is configured for Orkes
authentication automatically. With neither set, it runs in no-auth mode.

## Access control

Role-based access control and API key generation are server-side concerns. See the
[Orkes access control documentation](https://orkes.io/content/docs/getting-started/concepts/access-control).

## Troubleshooting

Auth failures surface as `ApiException` with a 401 or 403. See
[debugging.md](debugging.md#authentication-failures).
