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

A self-feeding worker that runs indefinitely. On startup it registers five sleep tasks (`csharp_worker_0` through `csharp_worker_4`) and the `csharp_sleep_workflow`, then runs two background services:

- **WorkflowGovernor** -- starts a configurable number of `csharp_sleep_workflow` instances per second (default 2), indefinitely.
- **SleepWorkers** -- five task handlers, each with a codename and a distinct sleep duration. The workflow chains them in sequence: whisperlink (2s) → quickpulse (1s) → shadowfetch (3s) → deepcrawl (9s) → ironforge (7s).

```bash
docker build --target harness -t csharp-sdk-harness .

docker run -d \
  -e CONDUCTOR_SERVER_URL=https://your-cluster.example.com/api \
  -e CONDUCTOR_AUTH_KEY=$CONDUCTOR_AUTH_KEY \
  -e CONDUCTOR_AUTH_SECRET=$CONDUCTOR_AUTH_SECRET \
  -e HARNESS_WORKFLOWS_PER_SEC=4 \
  csharp-sdk-harness
```

All resource names use a `csharp_` prefix so multiple SDK harnesses (Python, Java, Go, etc.) can coexist on the same cluster.

### Environment Variables

| Variable | Required | Default | Description |
|---|---|---|---|
| `CONDUCTOR_SERVER_URL` | yes | -- | Conductor API base URL |
| `CONDUCTOR_AUTH_KEY` | no | -- | Orkes auth key |
| `CONDUCTOR_AUTH_SECRET` | no | -- | Orkes auth secret |
| `HARNESS_WORKFLOWS_PER_SEC` | no | 2 | Workflows to start per second |
