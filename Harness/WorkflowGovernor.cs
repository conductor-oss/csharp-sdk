using Conductor.Client;
using Conductor.Client.Telemetry;
using Conductor.Executor;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Harness
{
    public class WorkflowGovernor : BackgroundService
    {
        private readonly WorkflowExecutor _executor;
        private readonly ILogger<WorkflowGovernor> _logger;
        private readonly string _workflowName;
        private readonly int _workflowsPerSecond;

        public WorkflowGovernor(
            Configuration config,
            ILogger<WorkflowGovernor> logger,
            string workflowName,
            int workflowsPerSecond,
            MetricsCollector metrics = null)
        {
            _executor = new WorkflowExecutor(config, metrics);
            _logger = logger;
            _workflowName = workflowName;
            _workflowsPerSecond = workflowsPerSecond;
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
                        _executor.StartWorkflow(
                            new Conductor.Client.Models.StartWorkflowRequest(name: _workflowName, version: 1));
                    }

                    _logger.LogInformation("Governor: started {Count} workflow(s)", _workflowsPerSecond);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Governor: error starting workflows");
                }

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
    }
}
