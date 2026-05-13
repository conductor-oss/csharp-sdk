# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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
