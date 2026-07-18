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
using Conductor.Client.Interfaces;
using Conductor.Client.Models;
using Conductor.Client.Worker;
using Task = Conductor.Client.Models.Task;

namespace Conductor.AI;

/// <summary>
/// A tool task riding the Worker SDK directly (guide §25.1 — tools are
/// ordinary Conductor workers). Implements <see cref="IWorkflowTask"/>
/// directly rather than through <c>GenericWorker</c>'s reflection mapping,
/// which assumes parameter-shaped worker methods; agent tool handlers are
/// dictionary-in/object-out. <see cref="WorkflowTaskExecutorConfiguration.BatchSize"/>
/// is bounded to the runtime's configured thread count — the SDK default of
/// <c>2×ProcessorCount</c> would over-poll a per-run tool queue.
/// </summary>
internal sealed class AgentToolWorker : IWorkflowTask
{
    private readonly ToolTaskExecutor _executor;

    internal AgentToolWorker(
        string taskType, ToolTaskExecutor executor, int pollIntervalMs, int threadCount, string? domain)
    {
        TaskType = taskType;
        _executor = executor;
        WorkerSettings = new WorkflowTaskExecutorConfiguration
        {
            Domain = domain,
            PollInterval = TimeSpan.FromMilliseconds(pollIntervalMs),
            BatchSize = threadCount,
        };
    }

    public string TaskType { get; }

    public WorkflowTaskExecutorConfiguration WorkerSettings { get; }

    public async System.Threading.Tasks.Task<TaskResult> Execute(Task task, CancellationToken token = default)
        => await _executor.ExecuteAsync(task, token);

    [Obsolete("Execute is going to be deprecated. Instead of TaskResult Execute method use the overloaded Task<TaskResult> Execute method going forward")]
    public TaskResult Execute(Task task) => Execute(task, default).GetAwaiter().GetResult();
}
