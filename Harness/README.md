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

A self-feeding worker that runs indefinitely. On startup it registers `csharp_echo_task` and `csharp_echo_workflow`, then runs two background services:

- **WorkflowGovernor** -- polls for running `csharp_echo_workflow` instances and starts new ones to maintain a target concurrency.
- **EchoWorker** -- polls for `csharp_echo_task`, echoes input data to output, and completes the task.

```bash
docker build --target harness -t csharp-sdk-harness .

docker run -d \
  -e CONDUCTOR_SERVER_URL=https://your-cluster.example.com/api \
  -e CONDUCTOR_AUTH_KEY=$CONDUCTOR_AUTH_KEY \
  -e CONDUCTOR_AUTH_SECRET=$CONDUCTOR_AUTH_SECRET \
  -e HARNESS_TARGET_CONCURRENCY=10 \
  -e HARNESS_POLL_INTERVAL_SEC=15 \
  csharp-sdk-harness
```

All resource names use a `csharp_` prefix so multiple SDK harnesses (Python, Java, Go, etc.) can coexist on the same cluster.

### Environment Variables

| Variable | Required | Default | Description |
|---|---|---|---|
| `CONDUCTOR_SERVER_URL` | yes | -- | Conductor API base URL |
| `CONDUCTOR_AUTH_KEY` | no | -- | Orkes auth key |
| `CONDUCTOR_AUTH_SECRET` | no | -- | Orkes auth secret |
| `HARNESS_TARGET_CONCURRENCY` | no | 5 | Workflows to keep running |
| `HARNESS_POLL_INTERVAL_SEC` | no | 10 | Governor check interval (seconds) |
