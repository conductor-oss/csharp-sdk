using Conductor.Api;
using Conductor.Client;
using Conductor.Client.Extensions;
using Conductor.Definition;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Harness
{
    public class Program
    {
        private const string WorkflowName = "csharp_sleep_workflow";

        private static readonly (string TaskName, string Codename, int SleepSeconds)[] SleepWorkers =
        {
            ("csharp_worker_0", "quickpulse",  1),
            ("csharp_worker_1", "shadowfetch", 3),
            ("csharp_worker_2", "ironforge",   7),
            ("csharp_worker_3", "whisperlink", 2),
            ("csharp_worker_4", "deepcrawl",   9),
        };

        public static async Task Main(string[] args)
        {
            var config = ApiExtensions.GetConfiguration();

            RegisterMetadata(config);

            var workflowsPerSec = int.TryParse(
                Environment.GetEnvironmentVariable("HARNESS_WORKFLOWS_PER_SEC"), out var wps) ? wps : 2;

            var host = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddConductorWorker(config);
                    foreach (var (taskName, codename, sleepSeconds) in SleepWorkers)
                        services.AddConductorWorkflowTask(new SleepWorker(taskName, codename, sleepSeconds));
                    services.WithHostedService();

                    services.AddSingleton(config);
                    services.AddHostedService(sp => new WorkflowGovernor(
                        config,
                        sp.GetRequiredService<ILogger<WorkflowGovernor>>(),
                        WorkflowName,
                        workflowsPerSec));
                })
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .Build();

            await host.RunAsync();
        }

        private static void RegisterMetadata(Configuration config)
        {
            var metadataClient = config.GetClient<MetadataResourceApi>();

            var taskDefs = new List<Conductor.Client.Models.TaskDef>();
            foreach (var (taskName, codename, sleepSeconds) in SleepWorkers)
            {
                taskDefs.Add(new Conductor.Client.Models.TaskDef(name: taskName)
                {
                    Description = $"C# SDK harness sleep task ({codename}, {sleepSeconds}s)",
                    RetryCount = 1,
                    TimeoutSeconds = 300,
                    ResponseTimeoutSeconds = 300
                });
            }
            metadataClient.RegisterTaskDef(taskDefs);

            var workflow = new ConductorWorkflow()
                .WithName(WorkflowName)
                .WithVersion(1)
                .WithDescription("C# SDK harness sleep workflow")
                .WithOwner("csharp-sdk-harness@conductor.io")
                .WithTask(new Conductor.Definition.TaskType.SimpleTask("csharp_worker_3", "csharp_worker_3"))
                .WithTask(new Conductor.Definition.TaskType.SimpleTask("csharp_worker_0", "csharp_worker_0"))
                .WithTask(new Conductor.Definition.TaskType.SimpleTask("csharp_worker_1", "csharp_worker_1"))
                .WithTask(new Conductor.Definition.TaskType.SimpleTask("csharp_worker_4", "csharp_worker_4"))
                .WithTask(new Conductor.Definition.TaskType.SimpleTask("csharp_worker_2", "csharp_worker_2"));

            metadataClient.UpdateWorkflowDefinitions(
                new List<Conductor.Client.Models.WorkflowDef> { workflow }, overwrite: true);
        }
    }
}
