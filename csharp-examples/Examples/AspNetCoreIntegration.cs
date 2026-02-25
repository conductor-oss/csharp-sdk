/*
 * Copyright 2024 Conductor Authors.
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on
 * an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the
 * specific language governing permissions and limitations under the License.
 */
using Conductor.Client;
using Conductor.Client.Extensions;
using Conductor.Client.Interfaces;
using Conductor.Client.Models;
using Conductor.Client.Orkes;
using Conductor.Executor;
using System;
using System.Collections.Generic;

namespace Conductor.Examples
{
    /// <summary>
    /// Demonstrates how to integrate Conductor SDK with an ASP.NET Core application.
    /// Shows the patterns for dependency injection, service registration, and API controllers.
    ///
    /// NOTE: This is a reference example showing the integration patterns.
    /// In a real ASP.NET Core app, these would be registered in Startup.cs/Program.cs.
    /// </summary>
    public class AspNetCoreIntegration
    {
        /// <summary>
        /// Shows how to set up Conductor services for DI in ASP.NET Core.
        /// In a real app, this goes in Program.cs or Startup.ConfigureServices.
        /// </summary>
        public static void ShowServiceRegistrationPattern()
        {
            Console.WriteLine("=== ASP.NET Core Integration Example ===\n");

            Console.WriteLine("1. Service Registration Pattern (for Program.cs):");
            Console.WriteLine(@"
    // In Program.cs or Startup.ConfigureServices:
    builder.Services.AddSingleton<Configuration>(sp =>
    {
        return new Configuration
        {
            BasePath = Environment.GetEnvironmentVariable(""CONDUCTOR_SERVER_URL"")
                       ?? ""http://localhost:8080/api"",
            AuthenticationSettings = new OrkesAuthenticationSettings(
                Environment.GetEnvironmentVariable(""CONDUCTOR_AUTH_KEY""),
                Environment.GetEnvironmentVariable(""CONDUCTOR_AUTH_SECRET""))
        };
    });

    builder.Services.AddSingleton<OrkesClients>(sp =>
        new OrkesClients(sp.GetRequiredService<Configuration>()));

    builder.Services.AddSingleton<IWorkflowClient>(sp =>
        sp.GetRequiredService<OrkesClients>().GetWorkflowClient());

    builder.Services.AddSingleton<IMetadataClient>(sp =>
        sp.GetRequiredService<OrkesClients>().GetMetadataClient());

    builder.Services.AddSingleton<ITaskClient>(sp =>
        sp.GetRequiredService<OrkesClients>().GetTaskClient());

    builder.Services.AddSingleton<WorkflowExecutor>(sp =>
        new WorkflowExecutor(sp.GetRequiredService<Configuration>()));
");

            Console.WriteLine("2. Controller Pattern:");
            Console.WriteLine(@"
    [ApiController]
    [Route(""api/[controller]"")]
    public class WorkflowController : ControllerBase
    {
        private readonly IWorkflowClient _workflowClient;
        private readonly WorkflowExecutor _executor;

        public WorkflowController(IWorkflowClient workflowClient, WorkflowExecutor executor)
        {
            _workflowClient = workflowClient;
            _executor = executor;
        }

        [HttpPost(""start"")]
        public IActionResult StartWorkflow([FromBody] StartWorkflowRequest request)
        {
            var workflowId = _workflowClient.StartWorkflow(request);
            return Ok(new { workflowId });
        }

        [HttpGet(""{workflowId}"")]
        public IActionResult GetWorkflow(string workflowId)
        {
            var workflow = _workflowClient.GetWorkflow(workflowId, includeTasks: true);
            return Ok(workflow);
        }

        [HttpPost(""{workflowId}/pause"")]
        public IActionResult PauseWorkflow(string workflowId)
        {
            _workflowClient.PauseWorkflow(workflowId);
            return Ok();
        }

        [HttpPost(""{workflowId}/resume"")]
        public IActionResult ResumeWorkflow(string workflowId)
        {
            _workflowClient.ResumeWorkflow(workflowId);
            return Ok();
        }
    }
");

            Console.WriteLine("3. Background Worker Service Pattern:");
            Console.WriteLine(@"
    public class ConductorWorkerService : BackgroundService
    {
        private readonly WorkflowTaskHost _host;

        public ConductorWorkerService(Configuration configuration)
        {
            _host = WorkflowTaskHost.CreateWorkerHost(configuration, LogLevel.Information);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _host.StartAsync(stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            // Graceful shutdown
            await base.StopAsync(cancellationToken);
        }
    }

    // In Program.cs:
    builder.Services.AddHostedService<ConductorWorkerService>();
");
        }

        /// <summary>
        /// Demonstrates the actual SDK usage pattern that would be in a controller.
        /// </summary>
        public static void DemoControllerUsage()
        {
            Console.WriteLine("4. Live Demo - Controller-like usage:\n");

            var config = ApiExtensions.GetConfiguration();
            var clients = new OrkesClients(config);
            var workflowClient = clients.GetWorkflowClient();
            var executor = new WorkflowExecutor(config);

            // Start a workflow (like a POST /api/workflow/start endpoint)
            Console.WriteLine("   Simulating POST /api/workflow/start ...");
            var request = new StartWorkflowRequest(
                name: "simple_workflow",
                version: 1,
                input: new Dictionary<string, object> { { "key", "value" } }
            );

            try
            {
                var workflowId = workflowClient.StartWorkflow(request);
                Console.WriteLine($"   Started workflow: {workflowId}");

                // Get workflow status (like a GET /api/workflow/{id} endpoint)
                Console.WriteLine($"   Simulating GET /api/workflow/{workflowId} ...");
                var wf = workflowClient.GetWorkflow(workflowId, false);
                Console.WriteLine($"   Status: {wf.Status}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   (Expected if no server) Error: {ex.Message}");
            }

            Console.WriteLine("\nASP.NET Core Integration Example completed!");
        }
    }
}
