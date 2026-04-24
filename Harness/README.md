# C# SDK Docker Harness

Two Docker targets built from the root `Dockerfile`: an **integration test runner** and a **long-running worker harness**.

## Integration Test Runner

Runs the xUnit integration test suite against a Conductor cluster and exits with 0 (pass) or 1 (fail).

```bash
docker build --target integration-test-runner -t csharp-sdk-tests .

docker run --rm \
  -e CONDUCTOR_SERVER_URL=https://your-cluster.example.com/api \
  -e CONDUCTOR_AUTH_KEY=$CONDUCTOR_AUTH_KEY \
  -e CONDUCTOR_AUTH_SECRET=$CONDUCTOR_AUTH_SECRET \
  -e GITHUB_RUN_ID=$(uuidgen) \
  csharp-sdk-tests
```

`GITHUB_RUN_ID` (or any unique value) namespaces test resources to avoid collisions between concurrent runs.

## Worker Harness

A self-feeding worker that runs indefinitely. On startup it registers five simulated tasks (`csharp_worker_0` through `csharp_worker_4`) and the `csharp_sleep_workflow`, then runs two background services:

- **WorkflowGovernor** -- starts a configurable number of `csharp_sleep_workflow` instances per second (default 2), indefinitely.
- **SimulatedTaskWorkers** -- five task handlers, each with a codename and a default sleep duration. Each worker supports configurable delay types, failure simulation, and output generation via task input parameters. The workflow chains them in sequence: quickpulse (1s) → whisperlink (2s) → shadowfetch (3s) → ironforge (4s) → deepcrawl (5s).

```bash
docker build --target harness -t csharp-sdk-harness .

docker run -d \
  -e CONDUCTOR_SERVER_URL=https://your-cluster.example.com/api \
  -e CONDUCTOR_AUTH_KEY=$CONDUCTOR_AUTH_KEY \
  -e CONDUCTOR_AUTH_SECRET=$CONDUCTOR_AUTH_SECRET \
  -e HARNESS_WORKFLOWS_PER_SEC=4 \
  csharp-sdk-harness
```

You can also run the harness locally without Docker:

```bash
export CONDUCTOR_SERVER_URL=https://your-cluster.example.com/api
export CONDUCTOR_AUTH_KEY=$CONDUCTOR_AUTH_KEY
export CONDUCTOR_AUTH_SECRET=$CONDUCTOR_AUTH_SECRET

dotnet run --project Harness/Harness.csproj
```

Override defaults with environment variables as needed:

```bash
HARNESS_WORKFLOWS_PER_SEC=4 HARNESS_BATCH_SIZE=10 dotnet run --project Harness/Harness.csproj
```

All resource names use a `csharp_` prefix so multiple SDK harnesses (Python, Java, Go, etc.) can coexist on the same cluster.

### Metrics

The harness exposes Prometheus metrics at `http://localhost:9991/metrics` via the OpenTelemetry
Prometheus exporter. See [metrics.md](../docs/metrics.md) for the full list of metrics, labels, and
configuration options.

### Environment Variables

| Variable | Required | Default | Description |
|---|---|---|---|
| `CONDUCTOR_SERVER_URL` | yes | -- | Conductor API base URL |
| `CONDUCTOR_AUTH_KEY` | no | -- | Orkes auth key |
| `CONDUCTOR_AUTH_SECRET` | no | -- | Orkes auth secret |
| `HARNESS_WORKFLOWS_PER_SEC` | no | 2 | Workflows to start per second |
| `HARNESS_BATCH_SIZE` | no | 20 | Number of tasks each worker polls per batch |
| `HARNESS_POLL_INTERVAL_MS` | no | 100 | Milliseconds between poll cycles |
| `HARNESS_METRICS_PORT` | no | 9991 | Port for the Prometheus metrics HTTP endpoint |
