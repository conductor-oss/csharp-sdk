# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Canonical metrics: harmonized cross-SDK metric surface (no `WORKER_CANONICAL_METRICS` gate; C# metrics were unreleased so consumers move directly to canonical) -- [details](docs/metrics.md#detailed-technical-notes--unreleased)
- `MetricsCollector.StartServer(port)` bundles Prometheus into the SDK with canonical bucket views
- `WorkflowExecutor` records `workflow_input_size_bytes` and `workflow_start_error_total`

### Changed

- Metrics labels are now camelCase to match the cross-SDK catalog
- `MetricsCollector` implements `IDisposable`
- OpenTelemetry dependencies moved into the main SDK package
- `http_api_client_request_seconds` `uri` label now uses the path template (e.g. `/workflow/{workflowId}`) for bounded cardinality

### Removed

- Top-level `METRICS.md` replaced by `docs/metrics.md`
