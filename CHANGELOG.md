# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased — async executor / thread-starvation fix]

> **Breaking (source + binary):** ~26 `XxxAsync` methods on the `*ResourceApi`
> classes and `I*ResourceApi` interfaces changed return type from `void` to
> `Task` (they were previously `async void`). Recompilation is required, and any
> fire-and-forget caller inside an `async` method will now emit compiler warning
> **CS4014** ("this call is not awaited"). To resolve, either `await` the call or
> explicitly discard it: `_ = api.DecideAsync(workflowId);`. This must ship as a
> **minor or major** version bump, never a patch.

### Added

- **AI agent SDK merged in** (from Agentspan `conductor-agent-sdk` 0.1.0): new
  packages `conductor-ai`, `conductor-ai-openai`, `conductor-ai-google-adk`,
  and `conductor-ai-semantic-kernel` — durable AI agents (`Agent`,
  `AgentRuntime`, tools, guardrails, handoffs, strategies, plans, schedules,
  code executors, semantic memory) targeting `net8.0`, released on the same
  tag-driven train as `conductor-csharp`. Docs live in `docs/agents/`;
  examples in `Conductor.AI.Examples/`. Migration note: users of the old
  `conductor-agent-sdk*` NuGet packages should switch to the `conductor-ai*`
  package ids — namespaces (`Conductor.AI.*`) and APIs are unchanged.
- Client access: `IAgentClient` (interface) + `OrkesAgentClient` ride the same
  `ApiClient`/`Configuration` as the rest of the SDK — obtain one via
  `OrkesApiClient.GetAgentClient()` or `Configuration.GetAgentClient()`, both
  sharing that `Configuration`'s token cache. The old bespoke
  `AgentAuthHandler` and its separate token client are deleted; 404s map to
  `AgentNotFoundException`, other non-2xx to `AgentApiException`.
- SSE streaming hardened: sources its auth header from
  `Configuration.AccessToken` on every (re)connect, tracks `Last-Event-ID`
  across drops with bounded backoff, and throws `SSEUnavailableException`
  when the server rejects streaming instead of silently returning nothing —
  `AgentHandle.StreamAsync` degrades to status-polling (one terminal `Done`
  event) rather than hanging.
- Verb contract: `AgentRuntime.ServeAsync` = deploy + serve (each served agent
  is compiled + registered first, idempotently) with an explicit `blocking`
  flag — `ServeAsync(blocking: false, ...)` returns once workers are polling
  in the background instead of blocking forever. `AgentHandle` gains
  `PauseAsync`/`UnpauseAsync` (distinct from `AgentRuntime`'s resume-by-
  execution-id, which re-attaches workers) and `SignalAsync` (now actually
  delivers `{"message": ...}` to the execution).
- `RunSettings` — per-run LLM overrides (`Model`, `Temperature`, `MaxTokens`,
  `ReasoningEffort`, `ThinkingBudgetTokens`; no `TopP` — it isn't part of the
  wire contract) accepted by `RunAsync`/`StartAsync`/`StreamAsync` and their
  `IAgentClient` conveniences. Only non-null fields override the agent;
  everything else keeps the agent's own settings.
- Connection environment: `CONDUCTOR_SERVER_URL` / `CONDUCTOR_AUTH_KEY` /
  `CONDUCTOR_AUTH_SECRET` now win over the legacy `AGENTSPAN_*` names (still
  honored as fallbacks); the default server URL is now
  `http://localhost:8080/api` (was `:6767`); blank env vars no longer clobber
  the fallback chain.
- `AgentConfig` — construction-time knobs with lenient env parsing
  (invalid/empty values fall back to the default): `AutoStartWorkers`,
  `DaemonWorkers`, `StreamingEnabled`, `LivenessEnabled`,
  `LivenessStallSeconds`, `LivenessCheckIntervalSeconds`.
- Tool workers now ride the Worker SDK (`IWorkflowTask` +
  `WorkflowTaskHost.CreateWorkerHost`) instead of a hand-rolled poll loop —
  polling, batching, and update-retry/backoff are the Worker SDK's.
- Worker credentials ride the `runtimeMetadata` wire contract: declared
  secret names are stamped on `TaskDef.RuntimeMetadata` at every
  registration, and a capable server (agentspan > 0.4.2 / conductor-oss ≥
  `3.32.0-rc.8`, PR #1255) delivers the resolved values on the wire-only
  `Task.RuntimeMetadata` at poll time. Dispatch is fail-closed — a declared
  credential missing from the delivered metadata raises
  `CredentialNotFoundException` and the tool task terminates; ambient
  process env is never read as a fallback. The old `/workers/secrets` pull
  path and its dedicated exceptions (`CredentialAuthException`,
  `RateLimitException`, `ServiceException`) are deleted.
- Liveness for stateful runs: a `SCHEDULED`/`IN_PROGRESS` tool task that goes
  unpolled past `AgentConfig.LivenessStallSeconds` surfaces as
  `WorkerStallException` from `AgentHandle.WaitAsync` instead of hanging out
  the full timeout. `WaitAsync` is itself now bounded (a 10-minute overall
  deadline) and tolerates up to 3 consecutive transient status-poll errors
  before giving up.
- Swarm hand-offs: transfer tools echo the hand-off `message`;
  `{name}_check_transfer` is first-wins on `transfer_message` and surfaces
  any non-winning transfers in `dropped_transfers` (with a warning) instead
  of silently discarding them.
- `agent-e2e` GitHub workflow runs the e2e suites as a two-server matrix: the
  released `agentspan-server-0.4.4.jar` and the Conductor OSS boot JAR
  `3.32.0-rc.8` (Maven Central).
- Core (`conductor-csharp`): new `OrkesApiClient.Configuration` property
  exposing the client's underlying `Configuration` (so `GetAgentClient()`
  and other domain-client factories can share it), and additive
  `RuntimeMetadata` fields on `TaskDef` and `Task` (used by the worker
  credentials wire contract above).

### Changed

- `WorkflowTaskExecutor`: converted `async void` methods (`WorkOnce`, `ProcessTasks`, `ProcessTask`) to `async Task` so the poll loop properly awaits each batch before re-entering. Previously, `async void` caused untracked continuations — the `RunningWorkerDone()` monitor count drifted, and any exception after the first `await` was unobserved on the thread pool.
- `WorkflowTaskExecutor`: replaced all `Thread.Sleep` calls (poll interval, error backoff, retry backoff) with `await Task.Delay`, releasing thread-pool threads during waits instead of blocking them.
- `ApiClient`: added `CallApiAsync` overload that accepts `Configuration` for async token-refresh retry (mirrors the sync `CallApi` + `RetryRestClientCallApi` path but uses `RestClient.ExecuteAsync`).
- `TaskResourceApi.BatchPollAsync` / `UpdateTaskAsync(TaskResult)`: now truly async — previously wrapped the synchronous `*WithHttpInfo` call in `Task.FromResult(...)`, providing zero async benefit.
- `IWorkflowTaskClient`: added `PollTaskAsync` and `UpdateTaskAsync` to the interface; `WorkflowTaskHttpClient` implements them via the now-truly-async `TaskResourceApi` methods.

### Fixed

- `WorkflowTaskExecutor`: `task_update_time_seconds` metric now records per-attempt HTTP latency. Previously a single `Stopwatch` spanned the entire retry loop including `Thread.Sleep` backoff (2–8s per retry), inflating the metric 6–15× beyond actual network time.
- `WorkflowTaskExecutor`: removed the `ThrowIfCancellationRequested()` from `ProcessTask`'s `finally` block. It previously threw before `RunningWorkerDone()`, leaking the running-worker count on cancellation, and throwing from `finally` could mask the in-flight exception. Cancellation is still observed at the loop level in `Work4Ever`.
- `ApiClient`: the async `CallApiAsync(..., Configuration)` overload now wraps its metrics recording in try/catch (so metrics can never break the HTTP path) and records the route `path` only — matching the sync path and the canonical `uri` label used by the Python/JS SDKs (e.g. `/workflow/{workflowId}`) instead of prepending the base path.

## [Unreleased — metrics]

> **Note for reviewers:** No version of this SDK has been published with metrics
> support. The `MetricsCollector` class and all metrics instrumentation exist only
> on development branches; no consumers are affected by the changes below.
> Metrics-related entries in this changelog describe changes to unreleased code.

### Added

- Canonical metrics aligned with the cross-SDK catalog -- see [docs/metrics.md](docs/metrics.md) for the full metric reference, configuration examples, and technical details

### Changed

- `Microsoft.Extensions.Logging` 6.0.0 → 10.0.0, `System.Diagnostics.DiagnosticSource` 8.0.1 → 10.0.0 -- these are transitive requirements of OpenTelemetry 1.15.x, which is now bundled for metrics support. The Prometheus HTTP listener exporter (`1.15.1-beta.1`) is a pre-release package because the [OTel Prometheus exporter specification](https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/metrics/sdk_exporters/prometheus.md) has never been finalized; no stable release exists or is expected ([tracking issue](https://github.com/open-telemetry/opentelemetry-dotnet/issues/2622)). This is the standard approach used across the .NET ecosystem.
- RestSharp `MaxTimeout` replaced with `Timeout` (TimeSpan) per deprecation warning

### Fixed

- `WorkflowTaskExecutor`: `OperationCanceledException` in the worker loop previously slept 10ms and re-entered `while(true)`, immediately re-throwing -- creating an infinite hot loop on shutdown. Now cleanly exits the loop.
- `WorkflowResourceApi.UpdateWorkflowVariables`: used C# string interpolation (`$"/workflow/{workflowId}/variables"`) instead of the path-template pattern used by every other API method, and was missing the `localVarPathParams.Add("workflowId", ...)` call. The HTTP request was functionally equivalent but the method is now consistent with the rest of the generated API surface.

### Removed

- Top-level `METRICS.md` replaced by `docs/metrics.md`
