# Testing workflows and workers

## Test projects

| Project / directory | Scope |
|---|---|
| `Tests/` (`conductor-csharp.test.csproj`) | Core SDK. |
| `Tests/ApiUnit/` | Resource-API units, no server. |
| `Tests/Definition/` | Workflow definition building. |
| `Tests/Worker/` | Worker behaviour. |
| `Tests/Executor/` | `WorkflowExecutor`. |
| `Tests/Telemetry/` | Metrics collection. |
| `Tests/Integration/` | End-to-end, against a live server. |
| `Tests/Api/` | Resource APIs against a live server. |
| `Tests/TestData/`, `Tests/Helper/` | Fixtures and helpers. |
| `Conductor.AI.Tests/` | Agent unit tests. |
| `Conductor.AI.E2eTests/` | Agent E2E suites, against a live server. |
| `Conductor.AI.{OpenAI,GoogleADK,SemanticKernel}.Tests/` | Framework adapters. |

The split that matters: **unit tests need no server**, integration and E2E tests do. CI
runs them as separate jobs for that reason.

## Unit-testing a worker

A worker is a plain class — construct a `Task`, call `Execute`, assert on the
`TaskResult`. No server, no host:

```csharp
var worker = new GreetWorker();
var task = new Task
{
    InputData = new Dictionary<string, object> { ["name"] = "Conductor" }
};

var result = worker.Execute(task);

Assert.Equal("Hello, Conductor!", result.OutputData["greeting"]);
```

This is the payoff of the stateless-worker design principle in
[workers.md](workers.md#design-principles) — a worker with no workflow-specific state is
trivially unit-testable.

## Unit-testing definitions

`ConductorWorkflow` builds a definition in memory, so assertions can be made without
registering anything:

```csharp
var workflow = new ConductorWorkflow().WithName("wf").WithVersion(1)
    .WithTask(new SimpleTask("greet", "greet_ref"));

Assert.Equal("wf", workflow.Name);
```

## Mocking the API surface

Every resource API has an interface (`IWorkflowResourceApi`, `ITaskResourceApi`, and so
on), so code that takes the interface can be tested against a mock rather than a server.
See [api-map.md](api-map.md).

## Integration tests

`Tests/Integration/` runs against a real server. CI runs this as `Run integration tests
(v5)` in `pull_request.yml`, with the server endpoint and auth key supplied as secrets.

Locally, point at a server and run:

```shell
export CONDUCTOR_SERVER_URL=http://localhost:8080/api
dotnet test Tests/conductor-csharp.test.csproj
```

### Running the OSS integration suite locally

`scripts/run-integration-oss.sh` mirrors the `integration_tests_oss` job in
`pull_request.yml`: it starts a local Conductor OSS + Postgres stack (defined in
`scripts/docker-compose-oss.yaml`), waits for `/health`, runs the integration suite with
Orkes-only tests filtered out (`ServerType!=Orkes`), and tears the stack down on exit.

```shell
scripts/run-integration-oss.sh                    # against `latest`
scripts/run-integration-oss.sh --version 3.32.0-rc18
scripts/run-integration-oss.sh --keep-up           # leave the stack running afterwards
```

The script always prints the resolved `conductoross/conductor` tag and pulls it before
starting the stack, since `latest` (the local default) is a mutable tag — without an
explicit pull, `docker compose up` would silently reuse a stale cached image instead of
fetching the current one.

Tests tagged `[Trait("ServerType", "Orkes")]` are excluded from this run because they
exercise features OSS does not implement — everything in `Tests/Integration/Orkes/`, plus
`EnvironmentVariableTests` and
`WorkflowLifecycleTests.UpdateWorkflowVariables_VariablesAreReflected`. Why each one is
gated, and the OSS endpoint it needs, is recorded in a comment next to the trait in the test
file itself. Read that before adding or removing the trait, and confirm the change against a
freshly-pulled image — a test that fails against a stale local image may pass against
current OSS.

## Agent E2E suites

`Conductor.AI.E2eTests/` is organised by feature, one numbered suite per area — basic
validation, tool calling, CLI tools, HTTP tools, stateful domains, PDF and media tools,
PLAN_EXECUTE refs, skills, agent client, auth headers, schedules, and SDK parity.

They are driven by `.github/workflows/agent-e2e.yml`, which downloads a server jar, starts
it, waits for `/health`, starts an MCP testkit, and runs the suites. That workflow also has
a **`Guard against vacuous run`** step asserting tests actually executed — worth knowing
about, because a suite that silently collects zero tests would otherwise look like a pass.

Extending the matching suite is usually the fastest way to reproduce a suspected SDK bug.
See [debugging.md](debugging.md#reproducing-in-a-test).

## Testing agents without a server

Agent construction and serialization are testable offline. `runtime.PlanAsync(agent)`
requires a server (it compiles server-side), but `AgentConfigSerializer` output and
builder validation — for instance that `Build()` throws `ConfigurationException` when
sub-agents lack a strategy — do not.

## Coverage

CI produces a Cobertura report and uploads to Codecov. Unit tests in a unit-test project
rather than among the integration suites keep that signal meaningful.
