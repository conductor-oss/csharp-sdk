# Security

## Authentication

Key/secret pairs are exchanged for a short-lived JWT by `TokenHandler`, cached on the
`Configuration`. See
[connection-authentication.md](connection-authentication.md).

Never commit a key pair. Supply them through `CONDUCTOR_AUTH_KEY` /
`CONDUCTOR_AUTH_SECRET` or your platform's secret store, and reuse a single
`Configuration` so the token is fetched once.

## Secrets

Secrets live server-side, managed through `SecretResourceApi`. A workflow or tool
references a secret **by name**; the value is resolved at execution time and never
appears in a definition.

```csharp
var secretClient = configuration.GetClient<SecretResourceApi>();
```

> Secret values are stored as raw strings. A recent fix stopped the SDK
> JSON-encoding them on write — if you stored secrets with an older version, values may
> carry stray surrounding quotes. See
> [#152](https://github.com/conductor-oss/csharp-sdk/issues/152).

## Credential injection for tools

Tools declare the credential names they need. The `${NAME}` placeholder form is used in
server-side tool headers:

```csharp
var listRepos = HttpTools.Create(
    name:        "list_github_repos",
    description: "List public GitHub repositories for a user.",
    url:         "https://api.github.com/users/conductor-oss/repos?per_page=5",
    headers:     new()
    {
        ["Authorization"]        = "Bearer ${GITHUB_TOKEN}",
        ["X-GitHub-Api-Version"] = "2022-11-28",
    },
    credentials: ["GITHUB_TOKEN"]);
```

The server fills the placeholder when it makes the call. The same mechanism applies to
`McpTools`, `ApiTools`, `CliTool`, and PLAN_EXECUTE `Context.FromUrl(...)` headers.

### Local tool credentials are fail-closed

For local `[Tool]` workers, the names in `Credentials` are stamped onto the worker's
`TaskDef.RuntimeMetadata` at every registration. A capable server resolves and delivers
the values on the wire-only `Task.RuntimeMetadata` at poll time — they never appear in
the task's regular `inputData`, so they do not show up in execution history or the UI.

Dispatch is **fail-closed**: if a declared credential is missing from the delivered
metadata, the SDK raises `CredentialNotFoundException` and the tool task terminates. It
never falls back to reading the ambient process environment.

That last point is the security property worth understanding. A tool cannot silently
pick up a developer's local environment variable in production — a misconfigured
credential fails loudly instead of running with the wrong identity.

There is no client-side credential resolution call; delivery is entirely server-driven,
per execution.

## Access control

Role-based access control, applications, and API keys are server-side. The relevant
clients are `AuthorizationResourceApi`, `ApplicationResourceApi`, `UserResourceApi`, and
`GroupResourceApi` — see [api-map.md](api-map.md#access-control) and the
[Orkes access control documentation](https://orkes.io/content/docs/getting-started/concepts/access-control).

## Tool execution surface

Two tool types execute arbitrary instructions and deserve scrutiny before enabling:

- **`CliTool`** — runs local commands. Always pass an explicit `allowedCommands`
  whitelist; the config also carries `allowShell` and `workingDir`.
- **Code execution** — `allowedLanguages` constrains what can run.

Both are opt-in and neither is enabled by default.

## Human approval as a control

`[Tool(ApprovalRequired = true)]` requires a human to approve each invocation, which is
the appropriate control for irreversible or outward-facing actions. See
[agents/concepts/streaming-hitl.md](agents/concepts/streaming-hitl.md#human-in-the-loop).

## Guardrails

Guardrails can block PII, enforce content policy, or escalate to a human before output
is returned. `maskedFields` on a guardrail definition marks values to redact. See
[agents/concepts/guardrails.md](agents/concepts/guardrails.md).

## Dependency scanning

Dependency vulnerabilities are tracked with the `vulnerability` and `security` labels on
the issue tracker.
