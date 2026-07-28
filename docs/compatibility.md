# Compatibility

## Framework targets

| Package | Target framework |
|---|---|
| `conductor-csharp` | `netstandard2.0` |
| `conductor-ai` | `net8.0` |
| `conductor-ai-openai` | `net8.0` |
| `conductor-ai-google-adk` | `net8.0` |
| `conductor-ai-semantic-kernel` | `net8.0` |

The core SDK targets `netstandard2.0`, so it is consumable from .NET Framework, .NET
Core, and modern .NET, as well as Xamarin/Mono. The agent layer requires `net8.0` — the
two packages therefore have different minimum runtimes, and a `netstandard2.0`-only host
cannot use `conductor-ai`.

## Server compatibility

The core SDK speaks the standard Conductor REST API and works against Conductor OSS and
Orkes Conductor.

The agent layer additionally requires a server exposing the `/agent/*` control plane. It
is not part of every Conductor build — see [server-setup.md](server-setup.md#server-features-the-sdk-depends-on).

Feature-gated capabilities:

| Capability | Requirement |
|---|---|
| `WaitForMessageTool` / `SendMessageAsync` | `conductor.workflow-message-queue.enabled=true` on the server |
| Local `[Tool]` credential delivery | A server that resolves and delivers `runtimeMetadata` at poll time |

Where a required server feature is absent, the call fails at the API rather than
degrading silently.

## Cross-SDK compatibility

Agents and workflows are server-side artifacts, so they are not SDK-specific. A workflow
registered from Python can be executed from .NET, and an agent deployed from .NET can be
started by name from Java. This is what makes the "not available in .NET" framework
adapters a routing question rather than a hard blocker — see
[agents/frameworks/langchain.md](agents/frameworks/langchain.md).

Behaviour parity with the sibling SDKs is checked by
`Conductor.AI.E2eTests/Suite17_SdkParity.cs`.

## Deprecations

Deprecated-but-honored surfaces:

| Surface | Status |
|---|---|
| `AGENTSPAN_*` environment variables | Honored as fallbacks; `CONDUCTOR_*` / `CONDUCTOR_AGENT_*` win. |
| `AgentspanException`, `AgentspanJson` | Retained type names; not renamed. |

See [upgrading.md](upgrading.md).

## Known gaps

Task-type builders and enum values that the server supports but this SDK does not yet
expose are tracked in [workflows.md](workflows.md#known-gaps).
