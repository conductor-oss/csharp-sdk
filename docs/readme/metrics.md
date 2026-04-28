# Metrics Guide

## Overview

The Conductor C# SDK collects metrics using the `System.Diagnostics.Metrics` API, making them compatible with any .NET metrics exporter (Prometheus, OpenTelemetry, Azure Monitor, etc.).

## Meter Name

All metrics are published under the meter name: `Conductor.Client`

```csharp
using Conductor.Client.Telemetry;

// Access the meter name
string meterName = ConductorMetrics.MeterName; // "Conductor.Client"
```

## Available Metrics

### Counters

| Name | Type | Tags | Description |
|------|------|------|-------------|
| `conductor.task.poll.count` | Counter<long> | taskType, workerId | Polls performed |
| `conductor.task.execution.count` | Counter<long> | taskType, workerId, status | Tasks executed |
| `conductor.task.update.count` | Counter<long> | taskType, workerId, status | Task updates sent |
| `conductor.api.call.count` | Counter<long> | endpoint, status | API calls made |
| `conductor.worker.restart.count` | Counter<long> | taskType, workerId | Worker restarts |

### Histograms

| Name | Type | Tags | Description |
|------|------|------|-------------|
| `conductor.task.poll.latency` | Histogram<double> | taskType | Poll latency (ms) |
| `conductor.task.execution.latency` | Histogram<double> | taskType | Execution time (ms) |
| `conductor.task.update.latency` | Histogram<double> | taskType | Update latency (ms) |
| `conductor.api.latency` | Histogram<double> | endpoint | API call latency (ms) |
| `conductor.task.payload.size` | Histogram<double> | taskType, direction | Payload size (bytes) |

## Using WorkerMetrics

The `WorkerMetrics` class provides a convenient per-worker metrics helper:

```csharp
using Conductor.Client.Telemetry;

var metrics = new WorkerMetrics("my_task_type", "worker-1");

// Record a poll
metrics.RecordPoll(success: true, taskCount: 3);
metrics.RecordPollLatency(15.5);

// Record execution with timing
using (metrics.TimeExecution())
{
    // ... do work ...
}
metrics.RecordExecution(success: true);

// Record task update
metrics.RecordUpdate(success: true);
metrics.RecordUpdateLatency(5.2);

// Record payload size
metrics.RecordPayloadSize(2048);
```

## MetricsConfig

Control which metric categories are collected:

```csharp
var config = new MetricsConfig
{
    Enabled = true,
    TaskPollingMetricsEnabled = true,
    TaskExecutionMetricsEnabled = true,
    TaskUpdateMetricsEnabled = false,    // Disable update metrics
    PayloadSizeMetricsEnabled = true
};

var metrics = new WorkerMetrics("my_task", "worker-1", config);
```

## Timing Scopes

Use `ConductorMetrics.Time()` for automatic latency recording:

```csharp
using (ConductorMetrics.Time(ConductorMetrics.ApiLatency,
    new KeyValuePair<string, object>("endpoint", "/api/tasks/poll")))
{
    // ... API call ...
}
```

## Consuming Metrics

### Using MeterListener (Built-in .NET)

```csharp
using System.Diagnostics.Metrics;

using var listener = new MeterListener();

listener.InstrumentPublished = (instrument, meterListener) =>
{
    if (instrument.Meter.Name == ConductorMetrics.MeterName)
    {
        meterListener.EnableMeasurementEvents(instrument);
    }
};

listener.SetMeasurementEventCallback<long>((instrument, measurement, tags, state) =>
{
    Console.WriteLine($"Counter: {instrument.Name} = {measurement}");
});

listener.SetMeasurementEventCallback<double>((instrument, measurement, tags, state) =>
{
    Console.WriteLine($"Histogram: {instrument.Name} = {measurement:F2}");
});

listener.Start();
```

### Using OpenTelemetry

```csharp
// Install: dotnet add package OpenTelemetry.Exporter.Prometheus.AspNetCore
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics.AddMeter(ConductorMetrics.MeterName);
        metrics.AddPrometheusExporter();
    });
```

### Using Prometheus (prometheus-net)

```csharp
// Install: dotnet add package prometheus-net
// The System.Diagnostics.Metrics are automatically bridged
// by prometheus-net when using the .NET metrics listener
```

## Direct Metric Access

For advanced use cases, access the metric instruments directly:

```csharp
// Counters
ConductorMetrics.TaskPollCount.Add(1,
    new KeyValuePair<string, object>("taskType", "my_task"));

ConductorMetrics.TaskExecutionCount.Add(1,
    new KeyValuePair<string, object>("taskType", "my_task"),
    new KeyValuePair<string, object>("status", "success"));

// Histograms
ConductorMetrics.TaskExecutionLatency.Record(42.5,
    new KeyValuePair<string, object>("taskType", "my_task"));

ConductorMetrics.TaskPayloadSize.Record(1024,
    new KeyValuePair<string, object>("taskType", "my_task"),
    new KeyValuePair<string, object>("direction", "input"));
```
