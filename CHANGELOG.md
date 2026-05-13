# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased — async executor / thread-starvation fix]

### Changed

- `WorkflowTaskExecutor`: converted `async void` methods (`WorkOnce`, `ProcessTasks`, `ProcessTask`) to `async Task` so the poll loop properly awaits each batch before re-entering. Previously, `async void` caused untracked continuations — the `RunningWorkerDone()` monitor count drifted, and any exception after the first `await` was unobserved on the thread pool.
- `WorkflowTaskExecutor`: replaced all `Thread.Sleep` calls (poll interval, error backoff, retry backoff) with `await Task.Delay`, releasing thread-pool threads during waits instead of blocking them.
- `ApiClient`: added `CallApiAsync` overload that accepts `Configuration` for async token-refresh retry (mirrors the sync `CallApi` + `RetryRestClientCallApi` path but uses `RestClient.ExecuteAsync`).
- `TaskResourceApi.BatchPollAsync` / `UpdateTaskAsync(TaskResult)`: now truly async — previously wrapped the synchronous `*WithHttpInfo` call in `Task.FromResult(...)`, providing zero async benefit.
- `IWorkflowTaskClient`: added `PollTaskAsync` and `UpdateTaskAsync` to the interface; `WorkflowTaskHttpClient` implements them via the now-truly-async `TaskResourceApi` methods.

### Fixed

- `WorkflowTaskExecutor`: `task_update_time_seconds` metric now records per-attempt HTTP latency. Previously a single `Stopwatch` spanned the entire retry loop including `Thread.Sleep` backoff (2–8s per retry), inflating the metric 6–15× beyond actual network time.
- `WorkflowTaskExecutor`: cancellation check in `ProcessTask`'s `finally` block was inverted (`== CancellationToken.None` instead of `!=`), so it never fired when a real token was provided.

## [Unreleased — metrics]

### Added

- Canonical metrics aligned with the cross-SDK catalog -- see [docs/metrics.md](docs/metrics.md) for the full metric reference, configuration examples, and technical details

### Changed

- `Microsoft.Extensions.Logging` 6.0.0 → 10.0.0, `System.Diagnostics.DiagnosticSource` 8.0.1 → 10.0.0 (required by OpenTelemetry 1.15.x which is now bundled for metrics support)
- RestSharp `MaxTimeout` replaced with `Timeout` (TimeSpan) per deprecation warning

### Fixed

- `WorkflowTaskExecutor`: `OperationCanceledException` in the worker loop previously slept 10ms and re-entered `while(true)`, immediately re-throwing -- creating an infinite hot loop on shutdown. Now cleanly exits the loop.
- `WorkflowResourceApi.UpdateWorkflowVariables`: used C# string interpolation (`$"/workflow/{workflowId}/variables"`) instead of the path-template pattern used by every other API method, and was missing the `localVarPathParams.Add("workflowId", ...)` call. The HTTP request was functionally equivalent but the method is now consistent with the rest of the generated API surface.

### Removed

- Top-level `METRICS.md` replaced by `docs/metrics.md`
