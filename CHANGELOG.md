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
