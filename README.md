# Conductor OSS C# SDK

[![CI](https://github.com/conductor-oss/csharp-sdk/actions/workflows/pull_request.yml/badge.svg)](https://github.com/conductor-oss/csharp-sdk/actions)

The .NET SDK for [Conductor](https://www.conductor-oss.org/) lets you build durable Conductor agents, workflows, and workers. Conductor coordinates retries, state, and observability while your C# code runs wherever you deploy it.

**Get involved:** [⭐ Conductor OSS](https://github.com/conductor-oss/conductor) · [Choose a Conductor OSS contribution](https://github.com/conductor-oss/conductor/contribute) · [Contribution guide](https://github.com/conductor-oss/conductor/blob/main/CONTRIBUTING.md)

## Choose your path

| I want to… | Start here |
|---|---|
| Build a durable Conductor agent with tools and human approval | [AI agent quickstart](#ai-agent-quickstart) |
| Bring an existing OpenAI Agents, Google ADK, or Semantic Kernel agent | [Framework bridges](#framework-bridges) |
| Build a durable workflow and C# worker | [Workflow and worker quickstart](#workflow-and-worker-quickstart) |
| Browse all examples | [Agent examples](Conductor.AI.Examples/README.md) · [Core examples](docs/examples.md) |
| Navigate the SDK documentation | [Documentation hub](docs/README.md) |

## Choose your Conductor server

Connect to a server before following either quickstart. Use the hosted Developer Edition by default, or run Conductor locally when you need a self-managed development environment.

### Recommended: Orkes Developer Edition

[Orkes Developer Edition](https://developer.orkescloud.com/) is the default hosted option. Create an application and access key in the Developer Edition UI, then configure this SDK with its API endpoint. Keep the key and secret out of source control.

```shell
export CONDUCTOR_SERVER_URL=https://developer.orkescloud.com/api
export CONDUCTOR_AUTH_KEY=<your-key-id>
export CONDUCTOR_AUTH_SECRET=<your-key-secret>
```

For another hosted or self-managed remote cluster, use that cluster's `/api` URL and its application credentials instead. See [server setup](docs/server-setup.md) for details.

### Local alternative: Docker

```shell
docker run --init -p 8080:8080 conductoross/conductor:latest
export CONDUCTOR_SERVER_URL=http://localhost:8080/api
```

The UI is at [http://localhost:8080](http://localhost:8080) and the API at `http://localhost:8080/api`. See [server setup](docs/server-setup.md) for full local, remote, and authentication guidance — including the server features the agent layer depends on.

## Why Conductor?

- **Survive process failures:** execution state is durable, so Conductor agents and workflows resume from completed work.
- **Build dynamic agent graphs:** define graphs in C# or let an LLM plan them at runtime. Conductor executes plans as durable sub-workflows rather than transient in-process loops.
- **Run tools as distributed tasks:** scale C# workers independently while Conductor manages retries and delivery.
- **Orchestrate long-running work:** combine AI, schedules, events, and human approval without holding application threads open.
- **See every execution:** inspect inputs, outputs, tool calls, retries, and status through one execution model.

## Requirements and compatibility

- **.NET 8+** for the agent packages; the core SDK targets **`netstandard2.0`**, so it is also consumable from .NET Framework and Mono
- A running OSS or Orkes Conductor server, selected in [Choose your Conductor server](#choose-your-conductor-server)
- Docker when using the local-server option

The CI workflows are the source of truth for the server versions exercised by this SDK — see the [agent E2E workflow](.github/workflows/agent-e2e.yml) for its pinned server version. Full details in [compatibility](docs/compatibility.md).

## Install the SDK

### Workflows and workers

The base package includes the workflow, task, worker, metadata, scheduler, and metrics clients:

```shell
dotnet add package conductor-csharp
```

### AI agents

```shell
dotnet add package conductor-ai
```

### Modules

| Package | Use it for |
|---|---|
| `conductor-csharp` | Workflow, task, worker, metadata, scheduler, and metrics clients |
| `conductor-ai` | Durable Conductor agents, tools, guardrails, handoffs, strategies, plans, schedules |
| `conductor-ai-openai` | OpenAI Agents bridge |
| `conductor-ai-google-adk` | Google ADK bridge |
| `conductor-ai-semantic-kernel` | Semantic Kernel bridge |

## AI agent quickstart

Use this path when your Conductor agent needs LLM reasoning, tools, guardrails, handoffs, or human approval. Select a server above first. The **server**, not just your .NET process, needs a configured LLM provider — the [agent getting-started guide](docs/agents/getting-started.md) covers both hosted and local paths.

```shell
export CONDUCTOR_AGENT_LLM_MODEL=anthropic/claude-sonnet-4-6
dotnet run --project Conductor.AI.Examples/01_BasicAgent
```

Expected outcome: the example prints an `AgentResult` containing the model response. Continue with the [AI agent guide](docs/agents/README.md), [tools guide](docs/agents/concepts/tools.md), and [agent examples](Conductor.AI.Examples/README.md).

### Framework bridges

Keep using the agent framework your team already knows. The SDK bridges [OpenAI Agents](docs/agents/frameworks/openai.md), [Google ADK](docs/agents/frameworks/google-adk.md), and [Semantic Kernel](docs/agents/frameworks/semantic-kernel.md) agents into durable Conductor agents.

LangChain, LangGraph, and the Claude Agent SDK have no .NET adapter — [each page](docs/agents/frameworks/langchain.md) explains the nearest supported path.

## Workflow and worker quickstart

With a server selected above, define a workflow, register it, start a worker, and execute it. The [core quickstart](docs/core-quickstart.md) walks through the complete runnable program — worker class included — in four steps.

```csharp
var workflow = new ConductorWorkflow().WithName("greetings").WithVersion(1);
workflow.WithTask(new SimpleTask("greet", "greet_ref").WithInput("name", workflow.Input("name")));

var executor = new WorkflowExecutor(configuration);
executor.RegisterWorkflow(workflow, overwrite: true);
```

Expected outcome: the workflow finishes `COMPLETED` and prints its greeting output. For worker patterns, workflow definitions, and testing, continue with the [core examples catalog](docs/examples.md), [worker guide](docs/workers.md), and [workflow guide](docs/workflows.md).

## Common tasks

| Need | Start with |
|---|---|
| Build C# Conductor agents | [Agent concepts](docs/agents/concepts/agents.md) |
| Add tools and human approval | [Agent tools](docs/agents/concepts/tools.md) · [streaming and HITL](docs/agents/concepts/streaming-hitl.md) |
| Use another agent framework | [OpenAI Agents](docs/agents/frameworks/openai.md) · [Google ADK](docs/agents/frameworks/google-adk.md) · [Semantic Kernel](docs/agents/frameworks/semantic-kernel.md) |
| Deploy, serve, and run Conductor agents | [Agent runtime modes](docs/agents/concepts/deploy-serve-run.md) |
| Implement and scale C# workers | [Workers guide](docs/workers.md) · [reliability](docs/reliability.md) |
| Define and evolve workflows | [Workflows guide](docs/workflows.md) · [lifecycle and versioning](docs/workflow-lifecycle.md) |
| Test workflows and workers | [Workflow testing](docs/workflow-testing.md) |
| Expose worker metrics | [Observability](docs/observability.md) |
| Run workers in production | [Deployment and scaling](docs/deployment-scaling.md) |
| Manage schedules and events | [Schedules and events](docs/schedules-events.md) |
| Handle secrets and credentials | [Security](docs/security.md) |
| Find typed clients and API references | [Core API map](docs/api-map.md) |
| Upgrade across a breaking change | [Upgrading](docs/upgrading.md) |

## Troubleshooting

| Symptom | Check |
|---|---|
| Connection refused | The server is healthy at `http://localhost:8080/health`; `CONDUCTOR_SERVER_URL` ends in `/api`. |
| Task remains `SCHEDULED` | A worker is polling the exact task type, and the worker host was actually started. |
| Authentication failure | `CONDUCTOR_AUTH_KEY` **and** `CONDUCTOR_AUTH_SECRET` are both set — one alone means no-auth mode. |
| Conductor agent cannot call a model | The server, not only the .NET process, has a configured LLM provider and model. |
| Agent run pauses and never resumes | A human-approval step is waiting; under multi-agent strategies, respond to the **event's** execution, not the root. |

More in [debugging](docs/debugging.md).

## Support and project policies

**Contribute upstream:** [Choose a Conductor OSS contribution](https://github.com/conductor-oss/conductor/contribute) · [Read the Conductor OSS contribution guide](https://github.com/conductor-oss/conductor/blob/main/CONTRIBUTING.md)

- [SDK issues](https://github.com/conductor-oss/csharp-sdk/issues) for C# SDK bugs and feature requests
- [Conductor server issues](https://github.com/conductor-oss/conductor/issues) for OSS server behavior
- [Conductor Code of Conduct](https://github.com/conductor-oss/conductor/blob/main/CODE_OF_CONDUCT.md) for community expectations
- [Conductor security policy](https://github.com/conductor-oss/conductor/security/policy) for private vulnerability reporting
- [Conductor Slack](https://join.slack.com/t/orkes-conductor/shared_invite/zt-2vdbx239s-Eacdyqya9giNLHfrCavfaA) and the [Orkes Community Forum](https://community.orkes.io/) for questions

## License

Apache 2.0. See [LICENSE](LICENSE).
