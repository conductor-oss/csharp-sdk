using Conductor.Client.Extensions;
using Conductor.Client.Interfaces;
using Conductor.Client.Worker;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Harness
{
    public class EchoWorker : IWorkflowTask
    {
        public const string TaskName = "csharp_echo_task";

        public string TaskType => TaskName;

        public WorkflowTaskExecutorConfiguration WorkerSettings { get; } = new()
        {
            WorkerId = "csharp_harness_worker",
            BatchSize = 5,
            PollInterval = TimeSpan.FromSeconds(1)
        };

        public Task<Conductor.Client.Models.TaskResult> Execute(Conductor.Client.Models.Task task, CancellationToken token = default)
        {
            var output = new Dictionary<string, object>(task.InputData ?? new Dictionary<string, object>())
            {
                ["echoed_by"] = WorkerSettings.WorkerId,
                ["echoed_at"] = DateTimeOffset.UtcNow.ToString("o")
            };
            return Task.FromResult(task.Completed(outputData: output));
        }

        public Conductor.Client.Models.TaskResult Execute(Conductor.Client.Models.Task task)
        {
            return Execute(task, CancellationToken.None).Result;
        }
    }
}
