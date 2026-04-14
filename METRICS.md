# Metrics Documentation

The C# SDK exposes worker metrics via the standard `System.Diagnostics.Metrics` API, making them
compatible with any .NET metrics listener -- most notably the
[OpenTelemetry .NET SDK](https://github.com/open-telemetry/opentelemetry-dotnet).

## Table of Contents

- [Quick Reference](#quick-reference)
- [Configuration](#configuration)
  - [DI-Based Workers](#di-based-workers)
  - [WorkflowTaskHost Convenience API](#workflowtaskhost-convenience-api)
  - [Prometheus via OpenTelemetry](#prometheus-via-opentelemetry)
  - [Console Exporter (Development)](#console-exporter-development)
- [Metric Types](#metric-types)
  - [Counters](#counters)
  - [Histograms](#histograms)
  - [Gauges](#gauges)
- [Labels](#labels)
- [Example Metrics Output](#example-metrics-output)
- [Best Practices](#best-practices)

## Quick Reference

All metrics are registered under the meter named `Conductor.Client`.

| Name | Type | Labels | Description |
|---|---|---|---|
| `task_poll_total` | Counter | `task_type` | Total task poll attempts |
| `task_poll_error_total` | Counter | `task_type`, `error_type` | Total task poll errors |
| `task_execute_error_total` | Counter | `task_type`, `error_type` | Total task execution errors |
| `task_update_error_total` | Counter | `task_type` | Total task update errors (after all retries) |
| `task_paused_total` | Counter | `task_type` | Polls skipped because the worker is paused |
| `task_execution_queue_full_total` | Counter | `task_type` | Polls returning zero capacity (all workers busy) |
| `thread_uncaught_exceptions_total` | Counter | -- | Uncaught exceptions in worker threads |
| `workflow_start_error_total` | Counter | `workflow_type` | Errors starting workflows |
| `external_payload_used_total` | Counter | `entity_name`, `operation`, `payload_type` | External payload storage usage |
| `task_poll_time_seconds` | Histogram | `task_type` | Task poll round-trip duration (seconds) |
| `task_execute_time_seconds` | Histogram | `task_type` | Task execution duration (seconds) |
| `task_update_time_seconds` | Histogram | `task_type` | Task result update duration (seconds) |
| `task_result_size_bytes` | Histogram | `task_type` | Task result payload size (bytes) |
| `workflow_input_size_bytes` | Histogram | `workflow_type`, `version` | Workflow input payload size (bytes) |
| `active_workers` | Gauge | `task_type` | Workers currently executing tasks |

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

To **expose** the metrics externally (e.g. Prometheus scraping), attach a metrics listener
or exporter as shown below.

### WorkflowTaskHost Convenience API

If you use the one-liner `WorkflowTaskHost.CreateWorkerHost(...)`, metrics are registered
automatically via the same `AddConductorWorker()` call:

```csharp
var host = WorkflowTaskHost.CreateWorkerHost(config, workers: new MyWorker());
await host.RunAsync();
```

### Prometheus via OpenTelemetry

Add the following NuGet packages to your project:

```
dotnet add package OpenTelemetry
dotnet add package OpenTelemetry.Exporter.Prometheus.HttpListener --prerelease
```

Then configure a `MeterProvider` before starting the host:

```csharp
using OpenTelemetry;
using OpenTelemetry.Metrics;
using Conductor.Client.Telemetry;

var meterProvider = Sdk.CreateMeterProviderBuilder()
    .AddMeter(MetricsCollector.MeterName)   // "Conductor.Client"
    .AddPrometheusHttpListener(options =>
    {
        options.UriPrefixes = new[] { "http://*:9090/" };
    })
    .Build();

// ... start the host ...

// Dispose when shutting down.
meterProvider?.Dispose();
```

Metrics are now available at `http://localhost:9090/metrics` in Prometheus text format.

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
| `task_poll_total` | `task_type` | Incremented once per poll round (regardless of how many tasks are returned). |
| `task_poll_error_total` | `task_type`, `error_type` | Incremented when a poll HTTP call fails. `error_type` is the exception class name. |
| `task_execute_error_total` | `task_type`, `error_type` | Incremented when `Execute()` throws. `error_type` is the exception class name. |
| `task_update_error_total` | `task_type` | Incremented when all update retries are exhausted. |
| `task_paused_total` | `task_type` | Incremented when a poll is skipped because the worker is paused. |
| `task_execution_queue_full_total` | `task_type` | Incremented when a poll is skipped because all workers are busy (batch size reached). |
| `thread_uncaught_exceptions_total` | -- | Incremented on any exception in the top-level poll loop that is not an `OperationCanceledException`. |
| `workflow_start_error_total` | `workflow_type` | Incremented when a workflow start call fails. |
| `external_payload_used_total` | `entity_name`, `operation`, `payload_type` | Incremented when external payload storage is used. |

### Histograms

Distribution metrics with sum, count, and bucket breakdowns. All time values are in **seconds**.
All size values are in **bytes**.

| Name | Labels | Unit | Description |
|---|---|---|---|
| `task_poll_time_seconds` | `task_type` | seconds | Wall-clock time for the poll HTTP call. |
| `task_execute_time_seconds` | `task_type` | seconds | Wall-clock time inside `worker.Execute()`. |
| `task_update_time_seconds` | `task_type` | seconds | Wall-clock time for the update call (including retries). |
| `task_result_size_bytes` | `task_type` | bytes | JSON-serialized size of `TaskResult.OutputData`. |
| `workflow_input_size_bytes` | `workflow_type`, `version` | bytes | Workflow input payload size. |

### Gauges

Point-in-time values sampled by the metrics listener.

| Name | Labels | Description |
|---|---|---|
| `active_workers` | `task_type` | Number of concurrent task executions in progress. Updated on every poll cycle. |

## Labels

| Label | Used By | Values |
|---|---|---|
| `task_type` | Most metrics | Task definition name (e.g. `"my_worker"`) |
| `error_type` | `task_poll_error_total`, `task_execute_error_total` | Exception class name (e.g. `"HttpRequestException"`) |
| `workflow_type` | `workflow_start_error_total`, `workflow_input_size_bytes` | Workflow definition name |
| `version` | `workflow_input_size_bytes` | Workflow version string |
| `entity_name` | `external_payload_used_total` | Entity name |
| `operation` | `external_payload_used_total` | Operation name |
| `payload_type` | `external_payload_used_total` | Payload type (e.g. `"TASK_INPUT"`, `"TASK_OUTPUT"`) |

## Example Metrics Output

When scraped by Prometheus (via the OpenTelemetry exporter), the output looks like:

```prometheus
# HELP task_poll_total Total task poll attempts
# TYPE task_poll_total counter
task_poll_total{task_type="my_worker"} 142

# HELP task_poll_time_seconds Task poll round-trip duration (seconds)
# TYPE task_poll_time_seconds histogram
task_poll_time_seconds_bucket{task_type="my_worker",le="0.005"} 12
task_poll_time_seconds_bucket{task_type="my_worker",le="0.01"} 45
task_poll_time_seconds_bucket{task_type="my_worker",le="0.025"} 98
task_poll_time_seconds_bucket{task_type="my_worker",le="0.05"} 120
task_poll_time_seconds_bucket{task_type="my_worker",le="0.1"} 135
task_poll_time_seconds_bucket{task_type="my_worker",le="0.25"} 140
task_poll_time_seconds_bucket{task_type="my_worker",le="0.5"} 142
task_poll_time_seconds_bucket{task_type="my_worker",le="1"} 142
task_poll_time_seconds_bucket{task_type="my_worker",le="+Inf"} 142
task_poll_time_seconds_sum{task_type="my_worker"} 3.842
task_poll_time_seconds_count{task_type="my_worker"} 142

# HELP task_execute_time_seconds Task execution duration (seconds)
# TYPE task_execute_time_seconds histogram
task_execute_time_seconds_bucket{task_type="my_worker",le="0.25"} 50
task_execute_time_seconds_bucket{task_type="my_worker",le="0.5"} 80
task_execute_time_seconds_bucket{task_type="my_worker",le="1"} 110
task_execute_time_seconds_bucket{task_type="my_worker",le="2.5"} 135
task_execute_time_seconds_bucket{task_type="my_worker",le="+Inf"} 142
task_execute_time_seconds_sum{task_type="my_worker"} 98.553
task_execute_time_seconds_count{task_type="my_worker"} 142

# HELP active_workers Workers currently executing tasks
# TYPE active_workers gauge
active_workers{task_type="my_worker"} 5

# HELP task_execution_queue_full_total Polls returning zero capacity
# TYPE task_execution_queue_full_total counter
task_execution_queue_full_total{task_type="my_worker"} 3
```

## Best Practices

1. **Use the OpenTelemetry Prometheus exporter for production.** It serves a standard `/metrics`
   endpoint that Prometheus can scrape directly.

2. **Set histogram bucket boundaries via OpenTelemetry Views** if the defaults don't match your
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
   rate(task_poll_total{task_type="my_worker"}[5m])
   ```

6. **Track p99 execution latency** using histogram quantiles:
   ```promql
   histogram_quantile(0.99, rate(task_execute_time_seconds_bucket[5m]))
   ```

7. **The `MetricsCollector` is available as a singleton via DI.** You can inject it into your
   own services to record `workflow_start_error_total`, `external_payload_used_total`, or any
   other metrics that occur outside the poll loop.
