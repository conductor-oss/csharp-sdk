using Conductor.Api;
using Conductor.Client;
using Conductor.Client.Telemetry;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Harness
{
    public class WorkflowGovernor : BackgroundService
    {
        private readonly WorkflowResourceApi _workflowClient;
        private readonly ILogger<WorkflowGovernor> _logger;
        private readonly string _workflowName;
        private readonly int _workflowsPerSecond;
        private readonly MetricsCollector _metrics;

        public WorkflowGovernor(
            Configuration config,
            ILogger<WorkflowGovernor> logger,
            string workflowName,
            int workflowsPerSecond,
            MetricsCollector metrics = null)
        {
            _workflowClient = config.GetClient<WorkflowResourceApi>();
            _logger = logger;
            _workflowName = workflowName;
            _workflowsPerSecond = workflowsPerSecond;
            _metrics = metrics;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "WorkflowGovernor started: workflow={Workflow}, rate={Rate}/sec",
                _workflowName, _workflowsPerSecond);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    for (var i = 0; i < _workflowsPerSecond; i++)
                    {
                        _workflowClient.StartWorkflow(
                            new Conductor.Client.Models.StartWorkflowRequest(name: _workflowName));
                    }

                    _logger.LogInformation("Governor: started {Count} workflow(s)", _workflowsPerSecond);
                }
                catch (Exception ex)
                {
                    _metrics?.RecordWorkflowStartError(_workflowName);
                    _logger.LogError(ex, "Governor: error starting workflows");
                }

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
    }
}
