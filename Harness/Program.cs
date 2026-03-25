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
        private const string WorkflowName = "csharp_echo_workflow";

        public static async Task Main(string[] args)
        {
            var config = ApiExtensions.GetConfiguration();

            RegisterMetadata(config);

            var targetConcurrency = int.TryParse(
                Environment.GetEnvironmentVariable("HARNESS_TARGET_CONCURRENCY"), out var tc) ? tc : 5;
            var pollIntervalSec = int.TryParse(
                Environment.GetEnvironmentVariable("HARNESS_POLL_INTERVAL_SEC"), out var pi) ? pi : 10;

            var host = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddConductorWorker(config);
                    services.AddConductorWorkflowTask(new EchoWorker());
                    services.WithHostedService();

                    services.AddSingleton(config);
                    services.AddHostedService(sp => new WorkflowGovernor(
                        config,
                        sp.GetRequiredService<ILogger<WorkflowGovernor>>(),
                        WorkflowName,
                        targetConcurrency,
                        TimeSpan.FromSeconds(pollIntervalSec)));
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

            metadataClient.RegisterTaskDef(new List<Conductor.Client.Models.TaskDef>
            {
                new Conductor.Client.Models.TaskDef(name: EchoWorker.TaskName)
                {
                    Description = "C# SDK harness echo task",
                    RetryCount = 1,
                    TimeoutSeconds = 300,
                    ResponseTimeoutSeconds = 300
                }
            });

            metadataClient.UpdateWorkflowDefinitions(new List<Conductor.Client.Models.WorkflowDef>
            {
                new ConductorWorkflow()
                    .WithName(WorkflowName)
                    .WithVersion(1)
                    .WithDescription("C# SDK harness echo workflow")
                    .WithOwner("csharp-sdk-harness@conductor.io")
                    .WithTask(new Conductor.Definition.TaskType.SimpleTask(EchoWorker.TaskName, EchoWorker.TaskName))
            }, overwrite: true);
        }
    }
}
