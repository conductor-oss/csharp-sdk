using Conductor.Api;
using Conductor.Client;
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
        private readonly int _targetConcurrency;
        private readonly TimeSpan _pollInterval;

        public WorkflowGovernor(
            Configuration config,
            ILogger<WorkflowGovernor> logger,
            string workflowName,
            int targetConcurrency,
            TimeSpan pollInterval)
        {
            _workflowClient = config.GetClient<WorkflowResourceApi>();
            _logger = logger;
            _workflowName = workflowName;
            _targetConcurrency = targetConcurrency;
            _pollInterval = pollInterval;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "WorkflowGovernor started: workflow={Workflow}, targetConcurrency={Target}, pollInterval={Interval}s",
                _workflowName, _targetConcurrency, _pollInterval.TotalSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var running = _workflowClient.GetRunningWorkflow(_workflowName);
                    var currentCount = running?.Count ?? 0;
                    var deficit = _targetConcurrency - currentCount;

                    if (deficit > 0)
                    {
                        _logger.LogInformation(
                            "Governor: {Current}/{Target} running, starting {Deficit} new workflow(s)",
                            currentCount, _targetConcurrency, deficit);

                        for (var i = 0; i < deficit; i++)
                        {
                            _workflowClient.StartWorkflow(new Conductor.Client.Models.StartWorkflowRequest(name: _workflowName));
                        }
                    }
                    else
                    {
                        _logger.LogDebug("Governor: {Current}/{Target} running, no action needed",
                            currentCount, _targetConcurrency);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Governor: error during reconciliation loop");
                }

                await Task.Delay(_pollInterval, stoppingToken);
            }
        }
    }
}
