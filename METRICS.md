# Metrics Documentation

The C# SDK exposes worker metrics via the standard `System.Diagnostics.Metrics` API, making them
compatible with any .NET metrics listener -- most notably the
[OpenTelemetry .NET SDK](https://github.com/open-telemetry/opentelemetry-dotnet).

## Table of Contents

- [Quick Reference](#quick-reference)
- [Configuration](#configuration)
  - [DI-Based Workers](#di-based-workers)
  - [Built-in Prometheus Server (Recommended)](#built-in-prometheus-server-recommended)
  - [Custom MeterProvider](#custom-meterprovider)
  - [Console Exporter (Development)](#console-exporter-development)
- [Metric Types](#metric-types)
  - [Counters](#counters)
  - [Histograms](#histograms)
  - [Gauges](#gauges)
- [Labels](#labels)
  - [Dual-Emit Strategy (Phase 1)](#dual-emit-strategy-phase-1)
  - [Deprecated Labels](#deprecated-labels)
- [Histogram Bucket Boundaries](#histogram-bucket-boundaries)
- [Example Metrics Output](#example-metrics-output)
- [OTel Scope Label](#otel-scope-label)
- [Best Practices](#best-practices)

## Quick Reference

All metrics are registered under the meter named `Conductor.Client`.

| Name | Type | Canonical Labels | Deprecated Labels | Description |
|---|---|---|---|---|
| `task_poll_total` | Counter | `taskType` | `task_type` | Total task poll attempts |
| `task_execution_started_total` | Counter | `taskType` | `task_type` | Tasks dispatched to the worker function |
| `task_poll_error_total` | Counter | `taskType`, `exception` | `task_type`, `error_type` | Total task poll errors |
| `task_execute_error_total` | Counter | `taskType`, `exception` | `task_type`, `error_type` | Total task execution errors |
| `task_update_error_total` | Counter | `taskType`, `exception` | `task_type`, `error_type` | Total task update errors (after all retries) |
| `task_ack_error_total` | Counter | `taskType`, `exception` | `task_type` | Task ack client-side errors (surface-only) |
| `task_ack_failed_total` | Counter | `taskType` | `task_type` | Task ack declined by server (surface-only) |
| `task_paused_total` | Counter | `taskType` | `task_type` | Polls skipped because the worker is paused |
| `task_execution_queue_full_total` | Counter | `taskType` | `task_type` | Polls returning zero capacity (all workers busy) |
| `thread_uncaught_exceptions_total` | Counter | `exception` | -- | Uncaught exceptions in worker threads |
| `workflow_start_error_total` | Counter | `workflowType`, `exception` | `workflow_type` | Errors starting workflows |
| `external_payload_used_total` | Counter | `entityName`, `operation`, `payload_type` | `entity_name` | External payload storage usage |
| `task_poll_time_seconds` | Histogram | `taskType`, `status` | `task_type` | Task poll round-trip duration (seconds) |
| `task_execute_time_seconds` | Histogram | `taskType`, `status` | `task_type` | Task execution duration (seconds) |
| `task_update_time_seconds` | Histogram | `taskType`, `status` | `task_type` | Task result update duration (seconds) |
| `http_api_client_request_seconds` | Histogram | `method`, `uri`, `status` | -- | HTTP API client request duration (seconds) |
| `task_result_size_bytes` | **Gauge** | `taskType` | `task_type` | Task result payload size (bytes) — last value |
| `task_result_size_bytes_histogram` | Histogram | `taskType` | `task_type` | **[DEPRECATED]** Task result payload size (bytes) |
| `workflow_input_size_bytes` | **Gauge** | `workflowType`, `version` | `workflow_type` | Workflow input payload size (bytes) — last value |
| `workflow_input_size_bytes_histogram` | Histogram | `workflowType`, `version` | `workflow_type` | **[DEPRECATED]** Workflow input payload size (bytes) |
| `active_workers` | Gauge | `taskType` | `task_type` | Workers currently executing tasks |

## Configuration

### DI-Based Workers

`MetricsCollector` is automatically registered as a singleton when you call `AddConductorWorker()`.
No additional setup is needed for the SDK to start recording -- metrics are written to
`System.Diagnostics.Metrics` instruments immediately.

```csharp
var host = new HostBuilder()
    .ConfigureServices(services =>
    {
        // MetricsCollector is registered automatically here.
        services.AddConductorWorker(config);
        services.AddConductorWorkflowTask(new MyWorker());
        services.WithHostedService();
    })
    .Build();
```

To **expose** the metrics externally (e.g. Prometheus scraping), use the built-in server
or attach your own exporter as shown below.

### Built-in Prometheus Server (Recommended)

The simplest way to expose a `/metrics` endpoint is to call `MetricsCollector.StartServer(port)`.
This configures the OpenTelemetry `MeterProvider` with canonical histogram bucket boundaries
and starts a Prometheus HTTP listener on the given port:

```csharp
var metrics = serviceProvider.GetRequiredService<MetricsCollector>();
metrics.StartServer(9991);
// Prometheus scrape endpoint now at http://localhost:9991/metrics
```

This is equivalent to what every other Conductor SDK provides (Python, Go, Java, Ruby, Rust)
and is the recommended setup for production use.

### Custom MeterProvider

If you need full control (e.g. adding additional exporters, custom Views, or extra meters),
you can configure your own `MeterProvider`. Use the SDK's canonical bucket constants
to stay aligned with the fleet:

```csharp
using OpenTelemetry;
using OpenTelemetry.Metrics;
using Conductor.Client.Telemetry;

var meterProvider = Sdk.CreateMeterProviderBuilder()
    .AddMeter(MetricsCollector.MeterName)
    .AddView("task_poll_time_seconds",
        new ExplicitBucketHistogramConfiguration
        { Boundaries = MetricsCollector.CanonicalTimeBuckets })
    .AddView("task_execute_time_seconds",
        new ExplicitBucketHistogramConfiguration
        { Boundaries = MetricsCollector.CanonicalTimeBuckets })
    .AddView("task_update_time_seconds",
        new ExplicitBucketHistogramConfiguration
        { Boundaries = MetricsCollector.CanonicalTimeBuckets })
    .AddView("http_api_client_request_seconds",
        new ExplicitBucketHistogramConfiguration
        { Boundaries = MetricsCollector.CanonicalTimeBuckets })
    .AddPrometheusHttpListener(options =>
    {
        options.UriPrefixes = new[] { "http://*:9090/" };
    })
    .Build();
```

### Console Exporter (Development)

For quick debugging, the OpenTelemetry console exporter prints metrics to stdout:

```
dotnet add package OpenTelemetry.Exporter.Console
```

```csharp
var meterProvider = Sdk.CreateMeterProviderBuilder()
    .AddMeter(MetricsCollector.MeterName)
    .AddConsoleExporter()
    .Build();
```

## Metric Types

### Counters

Monotonically increasing values. Prometheus exposes them with a `_total` suffix.

| Name | Labels | Description |
|---|---|---|
| `task_poll_total` | `taskType` | Incremented once per poll round (regardless of how many tasks are returned). |
| `task_execution_started_total` | `taskType` | Incremented when a polled task is dispatched to the worker's `Execute()` method. |
| `task_poll_error_total` | `taskType`, `exception` | Incremented when a poll HTTP call fails. `exception` is the exception class name. |
| `task_execute_error_total` | `taskType`, `exception` | Incremented when `Execute()` throws. `exception` is the exception class name. |
| `task_update_error_total` | `taskType`, `exception` | Incremented when all update retries are exhausted. `exception` is the exception class name. |
| `task_ack_error_total` | `taskType`, `exception` | Surface-only: C# has no separate ack call (batch-poll response is the ack). Available for user code. |
| `task_ack_failed_total` | `taskType` | Surface-only: same rationale as `task_ack_error_total`. |
| `task_paused_total` | `taskType` | Incremented when a poll is skipped because the worker is paused. |
| `task_execution_queue_full_total` | `taskType` | Incremented when a poll is skipped because all workers are busy (batch size reached). |
| `thread_uncaught_exceptions_total` | `exception` | Incremented on any exception in the top-level poll loop that is not an `OperationCanceledException`. |
| `workflow_start_error_total` | `workflowType`, `exception` | Incremented when a `WorkflowExecutor.StartWorkflow()` call fails. |
| `external_payload_used_total` | `entityName`, `operation`, `payload_type` | Incremented when external payload storage is used. |

### Histograms

Distribution metrics with sum, count, and bucket breakdowns. All time values are in **seconds**.

| Name | Labels | Unit | Description |
|---|---|---|---|
| `task_poll_time_seconds` | `taskType`, `status` | seconds | Wall-clock time for the poll HTTP call. `status` is `SUCCESS` or `FAILURE`. |
| `task_execute_time_seconds` | `taskType`, `status` | seconds | Wall-clock time inside `worker.Execute()`. `status` is `SUCCESS` or `FAILURE`. |
| `task_update_time_seconds` | `taskType`, `status` | seconds | Wall-clock time for the update call (including retries). `status` is `SUCCESS` or `FAILURE`. |
| `http_api_client_request_seconds` | `method`, `uri`, `status` | seconds | Every HTTP request made by the API client. `method` is the HTTP verb, `uri` is the request path, `status` is the HTTP status code (or `"0"` on transport failure). |
| `task_result_size_bytes_histogram` | `taskType` | bytes | **[DEPRECATED]** JSON-serialized size of `TaskResult.OutputData`. Use the `task_result_size_bytes` Gauge instead. |
| `workflow_input_size_bytes_histogram` | `workflowType`, `version` | bytes | **[DEPRECATED]** Workflow input payload size. Use the `workflow_input_size_bytes` Gauge instead. |

### Gauges

Point-in-time values sampled by the metrics listener.

| Name | Labels | Description |
|---|---|---|
| `task_result_size_bytes` | `taskType` | Serialized byte size of the most recent task result output. Last-value gauge. |
| `workflow_input_size_bytes` | `workflowType`, `version` | Serialized byte size of the most recent workflow input. Last-value gauge. |
| `active_workers` | `taskType` | Number of concurrent task executions in progress. Updated on every poll cycle. |

## Labels

### Dual-Emit Strategy (Phase 1)

As part of the cross-SDK metrics harmonization, every metric now emits **both** the canonical
camelCase label and the legacy snake_case label with the same value. This ensures existing
dashboards and alerts continue to work during migration.

| Canonical (new) | Legacy (deprecated) | Used By |
|---|---|---|
| `taskType` | `task_type` | Most task metrics |
| `exception` | `error_type` | Error counters |
| `workflowType` | `workflow_type` | Workflow metrics |
| `entityName` | `entity_name` | `external_payload_used_total` |

Labels that are new (no legacy equivalent): `status` on time histograms, `method`/`uri`/`status`
on `http_api_client_request_seconds`, `version` on `workflow_input_size_bytes`.

### Deprecated Labels

The following labels are deprecated and will be removed in a future major release:

- `task_type` — use `taskType`
- `error_type` — use `exception`
- `workflow_type` — use `workflowType`
- `entity_name` — use `entityName`

## Histogram Bucket Boundaries

The SDK defines canonical bucket boundaries that match all other Conductor SDKs:

**Time histograms** (`*_seconds`):

```
0.001, 0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10
```

**Size histograms** (deprecated `*_histogram`):

```
100, 1000, 10000, 100000, 1000000, 10000000
```

These are applied automatically when using `MetricsCollector.StartServer(port)`. If you build
your own `MeterProvider`, reference `MetricsCollector.CanonicalTimeBuckets` and
`MetricsCollector.CanonicalSizeBuckets`.

## Example Metrics Output

When scraped by Prometheus (via the built-in server or OpenTelemetry exporter):

```prometheus
# HELP task_poll_total Total task poll attempts
# TYPE task_poll_total counter
task_poll_total{taskType="my_worker",task_type="my_worker"} 142

# HELP task_execution_started_total Tasks dispatched to the worker function
# TYPE task_execution_started_total counter
task_execution_started_total{taskType="my_worker",task_type="my_worker"} 140

# HELP task_poll_time_seconds Task poll round-trip duration (seconds)
# TYPE task_poll_time_seconds histogram
task_poll_time_seconds_bucket{taskType="my_worker",task_type="my_worker",status="SUCCESS",le="0.001"} 2
task_poll_time_seconds_bucket{taskType="my_worker",task_type="my_worker",status="SUCCESS",le="0.005"} 12
task_poll_time_seconds_bucket{taskType="my_worker",task_type="my_worker",status="SUCCESS",le="0.01"} 45
task_poll_time_seconds_bucket{taskType="my_worker",task_type="my_worker",status="SUCCESS",le="0.025"} 98
task_poll_time_seconds_bucket{taskType="my_worker",task_type="my_worker",status="SUCCESS",le="0.05"} 120
task_poll_time_seconds_bucket{taskType="my_worker",task_type="my_worker",status="SUCCESS",le="0.1"} 135
task_poll_time_seconds_bucket{taskType="my_worker",task_type="my_worker",status="SUCCESS",le="0.25"} 140
task_poll_time_seconds_bucket{taskType="my_worker",task_type="my_worker",status="SUCCESS",le="+Inf"} 140
task_poll_time_seconds_sum{taskType="my_worker",task_type="my_worker",status="SUCCESS"} 3.842
task_poll_time_seconds_count{taskType="my_worker",task_type="my_worker",status="SUCCESS"} 140

# HELP http_api_client_request_seconds HTTP API client request duration (seconds)
# TYPE http_api_client_request_seconds histogram
http_api_client_request_seconds_bucket{method="GET",uri="/api/tasks/poll/batch/my_worker",status="200",le="0.01"} 30
http_api_client_request_seconds_bucket{method="GET",uri="/api/tasks/poll/batch/my_worker",status="200",le="+Inf"} 142

# HELP task_result_size_bytes Task result payload size (bytes)
# TYPE task_result_size_bytes gauge
task_result_size_bytes{taskType="my_worker",task_type="my_worker"} 2048

# HELP active_workers Workers currently executing tasks
# TYPE active_workers gauge
active_workers{taskType="my_worker",task_type="my_worker"} 5

# HELP task_execution_queue_full_total Polls returning zero capacity
# TYPE task_execution_queue_full_total counter
task_execution_queue_full_total{taskType="my_worker",task_type="my_worker"} 3
```

## OTel Scope Label

The C# metrics output includes an `otel_scope_name="Conductor.Client"` label on every metric.
This is injected by the OpenTelemetry .NET exporter (it identifies the `System.Diagnostics.Metrics.Meter`
that owns each instrument). Other SDKs using native Prometheus client libraries do not have this.
This is harmless — it is a standard OTel convention and does not interfere with canonical label queries.

## Best Practices

1. **Use `MetricsCollector.StartServer(port)` for production.** It serves a standard `/metrics`
   endpoint that Prometheus can scrape directly, with canonical bucket boundaries pre-configured.

2. **Override histogram buckets via OpenTelemetry Views** if the defaults don't match your
   workload. For example, if your workers are consistently fast (< 100ms), add more fine-grained
   lower buckets:
   ```csharp
   builder.AddView("task_execute_time_seconds",
       new ExplicitBucketHistogramConfiguration
       {
           Boundaries = new double[] { 0.001, 0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5 }
       });
   ```

3. **Alert on `task_update_error_total`.** A non-zero rate means task results are being lost
   after all retries are exhausted -- this is a critical failure.

4. **Monitor `task_execution_queue_full_total`.** A sustained rate indicates the worker needs
   more capacity (increase `BatchSize` or add replicas).

5. **Use `rate()` on counters, not raw values.** For example:
   ```promql
   rate(task_poll_total{taskType="my_worker"}[5m])
   ```

6. **Track p99 execution latency** using histogram quantiles:
   ```promql
   histogram_quantile(0.99, rate(task_execute_time_seconds_bucket[5m]))
   ```

7. **Migrate dashboards to canonical labels.** Both `taskType` and `task_type` resolve to the
   same value during Phase 1. Update your queries to use `taskType` before the deprecated labels
   are removed in a future major release.

8. **Use `WorkflowExecutor` for starting workflows.** It automatically records
   `workflow_input_size_bytes` and `workflow_start_error_total` when a `MetricsCollector`
   is injected.
