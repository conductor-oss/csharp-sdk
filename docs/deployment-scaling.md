# Deployment and scaling

## Deployment shapes

| Shape | Use |
|---|---|
| Worker service | A long-lived process hosting `WorkflowTaskHost` for one or more task types. The standard production shape. |
| Agent worker service | `runtime.ServeAsync(ct, agents)` — hosts local `[Tool]` workers for already-deployed agents. |
| Definition push | `RegisterWorkflow` / `runtime.DeployAsync(agents)` in CI. No execution, no workers. |
| Embedded | Workers alongside application code. Fine for low volume; couples worker lifetime to your app. |

Separating **definition push** from **worker hosting** is the important split: CI pushes
definitions, and worker services scale independently.

```csharp
// CI: push definitions, exit.
await runtime.DeployAsync(docAssistant, opsBot);

// Worker service: host tool workers, block until shut down.
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };
await runtime.ServeAsync(cts.Token, docAssistant, opsBot);
```

`ServeAsync` deploys each agent idempotently before starting its workers, so it is safe
to run without a preceding deploy. Pass `blocking: false` to return once workers are
polling — useful when hosting inside an existing application lifetime.

## Scaling workers

Scale horizontally: run more worker processes polling the same task type. The server
distributes tasks; no coordination is needed and no worker owns a partition.

The exception is **stateful agent runs**, which pin tasks to the process that started
them via a per-run domain. Those do not distribute, and adding processes does not help a
stalled run. See [reliability.md](reliability.md).

## Poll-loop tuning

Core SDK workers configure themselves through
`WorkflowTaskExecutorConfiguration`, per worker. Agent tool workers use `AgentConfig`:

| Setting | Env var | Default |
|---|---|---|
| Worker threads per task type | `CONDUCTOR_AGENT_WORKER_THREADS` | `1` |
| Poll interval (ms) | `CONDUCTOR_AGENT_WORKER_POLL_INTERVAL` | `100` |
| Auto-start workers | `CONDUCTOR_AGENT_AUTO_START_WORKERS` | `true` |
| Daemon worker threads | `CONDUCTOR_AGENT_DAEMON_WORKERS` | `true` |

Tuning guidance:

- **Thread count** should track task duration, not task volume. Long-running I/O-bound
  tasks want more threads; fast CPU-bound tasks want roughly one per core across the fleet.
- **Poll interval** trades latency against server load. Lowering it across many workers
  multiplies request volume — prefer more threads on fewer processes over aggressive
  polling on many.
- **Set these per worker.** A slow task and a fast task in one process should not share a
  configuration.

`DaemonWorkers = false` makes worker threads foreground, so the process will not exit
while they run — appropriate for a dedicated worker service, wrong for a CLI that should
terminate.

## Graceful shutdown

Cancel the token and let in-flight tasks finish, rather than killing the process — an
abandoned in-progress task waits for its server-side timeout before being rescheduled.

```csharp
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };
```

`await using` on the runtime shuts down any local tool workers it started.

## Containers

The repo ships a `Dockerfile` at the root and a `Harness/` project with its own image,
built by `.github/workflows/harness-image.yml`. `csharp-examples/` has a `Dockerfile` and
a `DockerfileMacArm` variant for Apple Silicon.

Containerised workers need `CONDUCTOR_SERVER_URL` reachable from inside the container —
`localhost` refers to the container, not the host.

## Capacity signals

Watch queue depth and poll/execution metrics to decide when to scale. See
[observability.md](observability.md).
