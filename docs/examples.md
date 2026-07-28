# Examples

Where the runnable code lives in this repository.

## Agent examples

`Conductor.AI.Examples/` — 176 self-contained example projects, numbered roughly by
topic and increasing in complexity:

| Range | Topic |
|---|---|
| `01_*` | Basic agent |
| `02_*` | Tools — simple, multi-step, registries |
| `03_*` | Structured output |
| `04_*` | HTTP and MCP tools |
| `Sk*_*` | Semantic Kernel adapter |

Each directory is its own `Program.cs` with a header comment listing the environment
variables it needs. Run one directly:

```shell
dotnet run --project Conductor.AI.Examples/01_BasicAgent
```

The CI job `Build agent examples` in `pull_request.yml` compiles all of them, so they
stay buildable.

## Core SDK examples

`csharp-examples/` — workflow and worker examples:

| File | Covers |
|---|---|
| `WorkFlowExamples.cs` | Workflow authoring and execution. |
| `HumanTaskExamples.cs` | Human task flows. |
| `TestWorker.cs` | A minimal worker. |
| `Runner.cs` / `Program.cs` | Entry points. |

There is a `Dockerfile` (and `DockerfileMacArm`) for running these in a container.

## Tests as examples

The test suites are often the most complete and most current reference, because CI keeps
them honest:

| Location | Covers |
|---|---|
| `Tests/Worker/` | Worker behaviour. |
| `Tests/Integration/` | End-to-end workflow runs against a live server. |
| `Tests/Definition/` | Workflow definition building. |
| `Tests/Telemetry/` | Metrics collection. |
| `Conductor.AI.Tests/` | Agent unit tests. |
| `Conductor.AI.E2eTests/` | Numbered agent E2E suites — tool calling, CLI tools, HTTP tools, stateful domains, media, PLAN_EXECUTE, skills, agent client, auth headers, schedules. |

`Conductor.AI.E2eTests/Suite17_SdkParity.cs` is worth knowing about specifically: it
checks behaviour parity with the sibling SDKs.

## Running the E2E suites

They need a live server and provider credentials, and are driven by the `Agent E2E`
workflow (`.github/workflows/agent-e2e.yml`) rather than the ordinary CI build. That
workflow downloads a server jar, starts it, waits for health, and runs the suites — read
it for the exact environment expected.

## Elsewhere

Additional examples outside this repo:
[conductor-sdk/conductor-examples](https://github.com/conductor-sdk/conductor-examples).
