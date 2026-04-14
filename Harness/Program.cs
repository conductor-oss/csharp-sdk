using Conductor.Api;
using Conductor.Client;
using Conductor.Client.Extensions;
using Conductor.Client.Telemetry;
using Conductor.Definition;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Harness
{
    public class Program
    {
        private const string WorkflowName = "csharp_simulated_tasks_workflow";
        private const int DefaultMetricsPort = 9991;

        private static readonly (string TaskName, string Codename, int SleepSeconds)[] SimulatedWorkers =
        {
            ("csharp_worker_0", "quickpulse",  1),
            ("csharp_worker_1", "whisperlink", 2),
            ("csharp_worker_2", "shadowfetch",   3),
            ("csharp_worker_3", "ironforge", 4),
            ("csharp_worker_4", "deepcrawl",   5),
        };

        public static async Task Main(string[] args)
        {
            var config = ApiExtensions.GetConfiguration();

            RegisterMetadata(config);

            var workflowsPerSec = int.TryParse(
                Environment.GetEnvironmentVariable("HARNESS_WORKFLOWS_PER_SEC"), out var wps) ? wps : 2;
            var batchSize = int.TryParse(
                Environment.GetEnvironmentVariable("HARNESS_BATCH_SIZE"), out var bs) ? bs : 20;
            var pollIntervalMs = int.TryParse(
                Environment.GetEnvironmentVariable("HARNESS_POLL_INTERVAL_MS"), out var pi) ? pi : 100;
            var metricsPort = int.TryParse(
                Environment.GetEnvironmentVariable("HARNESS_METRICS_PORT"), out var mp) ? mp : DefaultMetricsPort;

            var meterProvider = Sdk.CreateMeterProviderBuilder()
                .AddMeter(MetricsCollector.MeterName)
                .AddPrometheusHttpListener(options =>
                {
                    options.UriPrefixes = new[] { $"http://*:{metricsPort}/" };
                })
                .Build();

            Console.WriteLine($"Prometheus metrics server started on port {metricsPort}");

            var host = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddConductorWorker(config);
                    foreach (var (taskName, codename, sleepSeconds) in SimulatedWorkers)
                        services.AddConductorWorkflowTask(new SimulatedTaskWorker(
                            taskName, codename, sleepSeconds, batchSize, pollIntervalMs));
                    services.WithHostedService();

                    services.AddSingleton(config);
                    services.AddHostedService(sp => new WorkflowGovernor(
                        config,
                        sp.GetRequiredService<ILogger<WorkflowGovernor>>(),
                        WorkflowName,
                        workflowsPerSec,
                        sp.GetRequiredService<MetricsCollector>()));
                })
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .Build();

            try
            {
                await host.RunAsync();
            }
            finally
            {
                meterProvider?.Dispose();
            }
        }

        private static void RegisterMetadata(Configuration config)
        {
            var metadataClient = config.GetClient<MetadataResourceApi>();

            var taskDefs = new List<Conductor.Client.Models.TaskDef>();
            foreach (var (taskName, codename, sleepSeconds) in SimulatedWorkers)
            {
                taskDefs.Add(new Conductor.Client.Models.TaskDef(name: taskName)
                {
                    Description = $"C# SDK harness simulated task ({codename}, default delay {sleepSeconds}s)",
                    RetryCount = 1,
                    TimeoutSeconds = 300,
                    ResponseTimeoutSeconds = 300
                });
            }
            metadataClient.RegisterTaskDef(taskDefs);

            var workflow = new ConductorWorkflow()
                .WithName(WorkflowName)
                .WithVersion(1)
                .WithDescription("C# SDK harness simulated task workflow")
                .WithOwner("csharp-sdk-harness@conductor.io");

            foreach (var (taskName, codename, _) in SimulatedWorkers)
                workflow.WithTask(new Conductor.Definition.TaskType.SimpleTask(taskName, codename));

            metadataClient.UpdateWorkflowDefinitions(
                new List<Conductor.Client.Models.WorkflowDef> { workflow }, overwrite: true);
        }
    }
}
