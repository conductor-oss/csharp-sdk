# Metrics Documentation

The C# SDK exposes worker metrics via the standard `System.Diagnostics.Metrics` API, making them
compatible with any .NET metrics listener -- most notably the
[OpenTelemetry .NET SDK](https://github.com/open-telemetry/opentelemetry-dotnet).

The SDK also includes a built-in Prometheus HTTP listener via `MetricsCollector.StartServer(port)`
with canonical bucket boundaries pre-configured.

## Table of Contents

- [Quick Reference](#quick-reference)
- [Configuration](#configuration)
  - [Built-in Prometheus Server](#built-in-prometheus-server)
  - [DI-Based Workers](#di-based-workers)
  - [WorkflowTaskHost Convenience API](#workflowtaskhost-convenience-api)
  - [Custom OpenTelemetry Setup](#custom-opentelemetry-setup)
  - [Console Exporter (Development)](#console-exporter-development)
- [Metric Types](#metric-types)
  - [Counters](#counters)
  - [Time Histograms](#time-histograms)
  - [Size Histograms](#size-histograms)
  - [Gauges](#gauges)
- [Labels](#labels)
- [Bucket Boundaries](#bucket-boundaries)
- [Best Practices](#best-practices)

## Quick Reference

All metrics are registered under the meter named `Conductor.Client`.

All label names use **camelCase** to align with the cross-SDK canonical specification.

### Counters

| Name | Labels | Description |
|---|---|---|
| `task_poll_total` | `taskType` | Total task poll attempts |
| `task_execution_started_total` | `taskType` | Tasks dispatched to the worker function |
| `task_poll_error_total` | `taskType`, `exception` | Total task poll errors |
| `task_execute_error_total` | `taskType`, `exception` | Total task execution errors |
| `task_update_error_total` | `taskType`, `exception` | Total task update errors (after all retries) |
| `task_ack_error_total` | `taskType`, `exception` | Task ack client-side errors |
| `task_ack_failed_total` | `taskType` | Task ack declined by server |
| `task_paused_total` | `taskType` | Polls skipped because the worker is paused |
| `task_execution_queue_full_total` | `taskType` | Polls returning zero capacity (all workers busy) |
| `thread_uncaught_exceptions_total` | `exception` | Uncaught exceptions in worker threads |
| `workflow_start_error_total` | `workflowType`, `exception` | Errors starting workflows |
| `external_payload_used_total` | `entityName`, `operation`, `payloadType` | External payload storage usage |

### Time Histograms

| Name | Labels | Description |
|---|---|---|
| `task_poll_time_seconds` | `taskType`, `status` | Task poll round-trip duration (seconds) |
| `task_execute_time_seconds` | `taskType`, `status` | Task execution duration (seconds) |
| `task_update_time_seconds` | `taskType`, `status` | Task result update duration (seconds) |
| `http_api_client_request_seconds` | `method`, `uri`, `status` | HTTP API client request duration (seconds) |

### Size Histograms

| Name | Labels | Description |
|---|---|---|
| `task_result_size_bytes` | `taskType` | Task result payload size (bytes) |
| `workflow_input_size_bytes` | `workflowType`, `version` | Workflow input payload size (bytes) |

### Gauge

| Name | Labels | Description |
|---|---|---|
| `active_workers` | `taskType` | Workers currently executing tasks |

## Configuration

### Built-in Prometheus Server

The simplest way to expose metrics is via the built-in Prometheus HTTP listener:

```csharp
using Conductor.Client.Telemetry;

var metricsCollector = host.Services.GetRequiredService<MetricsCollector>();
metricsCollector.StartServer(9991);

// Metrics are now available at http://localhost:9991/metrics
// with canonical bucket boundaries pre-configured.
```

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

// Start the built-in Prometheus server
var metrics = host.Services.GetRequiredService<MetricsCollector>();
metrics.StartServer(9991);
```

### WorkflowTaskHost Convenience API

If you use the one-liner `WorkflowTaskHost.CreateWorkerHost(...)`, metrics are registered
automatically via the same `AddConductorWorker()` call:

```csharp
var host = WorkflowTaskHost.CreateWorkerHost(config, workers: new MyWorker());
await host.RunAsync();
```

### Custom OpenTelemetry Setup

If you need custom bucket boundaries or additional exporters, you can configure your own
`MeterProvider` instead of calling `StartServer()`. The canonical bucket boundaries are
available as public constants:

```csharp
using OpenTelemetry;
using OpenTelemetry.Metrics;
using Conductor.Client.Telemetry;

var meterProvider = Sdk.CreateMeterProviderBuilder()
    .AddMeter(MetricsCollector.MeterName)
    .AddView("task_poll_time_seconds",
        new ExplicitBucketHistogramConfiguration { Boundaries = MetricsCollector.CanonicalTimeBuckets })
    .AddView("task_execute_time_seconds",
        new ExplicitBucketHistogramConfiguration { Boundaries = MetricsCollector.CanonicalTimeBuckets })
    .AddView("task_update_time_seconds",
        new ExplicitBucketHistogramConfiguration { Boundaries = MetricsCollector.CanonicalTimeBuckets })
    .AddView("http_api_client_request_seconds",
        new ExplicitBucketHistogramConfiguration { Boundaries = MetricsCollector.CanonicalTimeBuckets })
    .AddView("task_result_size_bytes",
        new ExplicitBucketHistogramConfiguration { Boundaries = MetricsCollector.CanonicalSizeBuckets })
    .AddView("workflow_input_size_bytes",
        new ExplicitBucketHistogramConfiguration { Boundaries = MetricsCollector.CanonicalSizeBuckets })
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
| `task_execution_started_total` | `taskType` | Incremented when a polled task is dispatched to the user's worker function. |
| `task_poll_error_total` | `taskType`, `exception` | Incremented when a poll HTTP call fails. `exception` is the exception class name. |
| `task_execute_error_total` | `taskType`, `exception` | Incremented when `Execute()` throws. `exception` is the exception class name. |
| `task_update_error_total` | `taskType`, `exception` | Incremented when all update retries are exhausted. `exception` is the exception class name. |
| `task_ack_error_total` | `taskType`, `exception` | Incremented when the server-side ack throws an exception client-side. |
| `task_ack_failed_total` | `taskType` | Incremented when the server returns a non-success ack response. |
| `task_paused_total` | `taskType` | Incremented when a poll is skipped because the worker is paused. |
| `task_execution_queue_full_total` | `taskType` | Incremented when a poll is skipped because all workers are busy (batch size reached). |
| `thread_uncaught_exceptions_total` | `exception` | Incremented on any exception in the top-level poll loop that is not an `OperationCanceledException`. |
| `workflow_start_error_total` | `workflowType`, `exception` | Incremented when a workflow start call fails. |
| `external_payload_used_total` | `entityName`, `operation`, `payloadType` | Incremented when external payload storage is used. `operation` is `READ` or `WRITE`. `payloadType` is one of `TASK_INPUT`, `TASK_OUTPUT`, `WORKFLOW_INPUT`, `WORKFLOW_OUTPUT`. |

### Time Histograms

Distribution metrics with sum, count, and bucket breakdowns. All time values are in **seconds**.
The `status` label is `"SUCCESS"` or `"FAILURE"`.

| Name | Labels | Description |
|---|---|---|
| `task_poll_time_seconds` | `taskType`, `status` | Wall-clock time for the poll HTTP call. `status=SUCCESS` even when the response is empty. |
| `task_execute_time_seconds` | `taskType`, `status` | Wall-clock time inside `worker.Execute()`. `status=FAILURE` if it throws. |
| `task_update_time_seconds` | `taskType`, `status` | Wall-clock time for the update call (including retries). |
| `http_api_client_request_seconds` | `method`, `uri`, `status` | Latency of every HTTP request made by the API client. `method` is the HTTP verb, `uri` is the request path, `status` is the HTTP status code as a string (or `"0"` on network failure). |

Canonical bucket boundaries: `{0.001, 0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10}`

### Size Histograms

| Name | Labels | Unit | Description |
|---|---|---|---|
| `task_result_size_bytes` | `taskType` | bytes | JSON-serialized size of `TaskResult.OutputData`. |
| `workflow_input_size_bytes` | `workflowType`, `version` | bytes | Workflow input payload size. |

Canonical bucket boundaries: `{100, 1000, 10000, 100000, 1000000, 10000000}`

### Gauges

Point-in-time values sampled by the metrics listener.

| Name | Labels | Description |
|---|---|---|
| `active_workers` | `taskType` | Number of concurrent task executions in progress. Updated on every poll cycle. |

## Labels

All labels use **camelCase** per the cross-SDK canonical specification.

| Label | Used By | Values |
|---|---|---|
| `taskType` | Most metrics | Task definition name (e.g. `"my_worker"`) |
| `exception` | Error counters, `thread_uncaught_exceptions_total` | Exception class name (e.g. `"HttpRequestException"`) |
| `status` | Time histograms | `"SUCCESS"` or `"FAILURE"` |
| `workflowType` | `workflow_start_error_total`, `workflow_input_size_bytes` | Workflow definition name |
| `version` | `workflow_input_size_bytes` | Workflow version string |
| `entityName` | `external_payload_used_total` | Entity name |
| `operation` | `external_payload_used_total` | `"READ"` or `"WRITE"` |
| `payloadType` | `external_payload_used_total` | `"TASK_INPUT"`, `"TASK_OUTPUT"`, `"WORKFLOW_INPUT"`, `"WORKFLOW_OUTPUT"` |
| `method` | `http_api_client_request_seconds` | HTTP verb (e.g. `"GET"`, `"POST"`) |
| `uri` | `http_api_client_request_seconds` | Request path |

## Bucket Boundaries

Available as public constants on `MetricsCollector`:

```csharp
MetricsCollector.CanonicalTimeBuckets
// { 0.001, 0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10 }

MetricsCollector.CanonicalSizeBuckets
// { 100, 1_000, 10_000, 100_000, 1_000_000, 10_000_000 }
```

## Best Practices

1. **Use `StartServer(port)` for quick production setup.** It configures canonical bucket
   boundaries and serves a standard `/metrics` endpoint that Prometheus can scrape directly.

2. **Alert on `task_update_error_total`.** A non-zero rate means task results are being lost
   after all retries are exhausted -- this is a critical failure.

3. **Monitor `task_execution_queue_full_total`.** A sustained rate indicates the worker needs
   more capacity (increase `BatchSize` or add replicas).

4. **Use `rate()` on counters, not raw values.** For example:
   ```promql
   rate(task_poll_total{taskType="my_worker"}[5m])
   ```

5. **Track p99 execution latency** using histogram quantiles:
   ```promql
   histogram_quantile(0.99, rate(task_execute_time_seconds_bucket[5m]))
   ```

6. **The `MetricsCollector` is available as a singleton via DI.** You can inject it into your
   own services to record `workflow_start_error_total`, `external_payload_used_total`, or any
   other metrics that occur outside the poll loop.
