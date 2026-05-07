# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- **Metrics harmonization** - canonical metric surface aligned with the cross-SDK catalog (no `WORKER_CANONICAL_METRICS` env var; the C# metrics surface was unreleased so consumers move directly to canonical without a gate)
  - `Conductor.Client.Telemetry.MetricsCollector` now emits the harmonized cross-SDK catalog under meter `Conductor.Client`: new instruments `task_execution_started_total`, `task_ack_error_total`, `task_ack_failed_total`, `http_api_client_request_seconds{method,uri,status}`. Time histograms (`task_poll_time_seconds`, `task_execute_time_seconds`, `task_update_time_seconds`) gain a `status` label (`SUCCESS`/`FAILURE`). Time buckets `0.001…10s`; size buckets `100…10_000_000` bytes.
  - `MetricsCollector.StartServer(port)` bundles the OpenTelemetry Prometheus HTTP listener with canonical bucket views into the SDK so consumers no longer have to wire OTel manually.
  - `ApiClient` records `http_api_client_request_seconds` for every call (method, interpolated URI, status code, elapsed seconds) via a new public `ApiClient.Metrics` property.
  - DI registration (`AddConductorWorker()`) now also assigns the singleton `MetricsCollector` to `ApiClient.Metrics` so HTTP-client metrics flow without further wiring.
  - `WorkflowExecutor` accepts an optional `MetricsCollector` and records `workflow_input_size_bytes` and `workflow_start_error_total` on `StartWorkflow`.

### Changed

- **Metrics harmonization** - label/API renames; no legacy mode (other Conductor SDKs that did release metrics — Python, Go, Java, JavaScript, Ruby — ship a gated switch via `WORKER_CANONICAL_METRICS`)
  - **Metrics labels are now camelCase** to match the canonical cross-SDK catalog: `task_type → taskType`, `error_type → exception`, `workflow_type → workflowType`, `payload_type → payloadType`, `entity_name → entityName`.
  - `MetricsCollector` is now `IDisposable`.
  - `RecordUncaughtException`, `RecordTaskUpdateError`, and `RecordWorkflowStartError` now take an `exception` label argument.
  - OpenTelemetry dependencies (`OpenTelemetry 1.15.1`, `OpenTelemetry.Exporter.Prometheus.HttpListener 1.15.1-beta.1`) moved into the main SDK package; `Microsoft.Extensions.Logging` bumped 6.0.0 → 10.0.0; `System.Diagnostics.DiagnosticSource` bumped 8.0.1 → 10.0.0.
  - Harness uses `MetricsCollector.StartServer(port)` instead of inline OTel bootstrap; `WorkflowGovernor` switches to `WorkflowExecutor.StartWorkflow` so workflow metrics get recorded automatically.
  - RestSharp `MaxTimeout` calls replaced with `Timeout = TimeSpan.FromMilliseconds(...)` to fix a deprecation warning.
  - New `docs/metrics.md` (335 lines) documenting the canonical catalog, configuration via `StartServer(port)` / DI / custom OTel `MeterProvider` / console exporter, the metrics that are registered-but-N/A for the C# internal runner (`task_ack_*`, `external_payload_used_total`, `worker_restart_total`), and known caveats (`otel_scope_name="Conductor.Client"` always present; `uri` interpolated rather than templated).
  - `docs/readme/workers.md` and `Harness/README.md` updated to point at `docs/metrics.md`.

### Removed

- Top-level `METRICS.md` (252 lines, snake_case catalog) replaced by `docs/metrics.md`.
