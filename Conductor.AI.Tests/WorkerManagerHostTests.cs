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
// DD6/R9 — WorkerManager hosts its registered AgentToolWorkers on the Worker
// SDK (WorkflowTaskHost.CreateWorkerHost). StartAsync() must be idempotent —
// a shared manager invoked twice (e.g. overlapping runs) must not build a
// second host and double-poll every task type — and StopAsync()/DisposeAsync
// must tear the host down cleanly.

using Conductor.Client;
using Xunit;

namespace Conductor.AI.Tests;

public sealed class WorkerManagerHostTests
{
    private static Agent AgentWithOneTool(string name) => new(name)
    {
        Model = "openai/gpt-4o",
        Tools = [new ToolDef { Name = $"{name}_tool", Handler = (_, _) => Task.FromResult<object?>("ok") }],
    };

    [Fact]
    public async Task StartAsync_CalledTwice_OnlyBuildsHostOnce()
    {
        var configuration = new Configuration { BasePath = "http://127.0.0.1:1/api" };
        var manager = new WorkerManager(http: null!, configuration, pollIntervalMs: 60_000, threadCount: 1);
        manager.RegisterAgentTools(AgentWithOneTool("agent_one"));

        await manager.StartAsync();
        var firstHost = manager.HostForTesting;
        Assert.NotNull(firstHost);

        await manager.StartAsync(); // must be a no-op the second time
        Assert.Same(firstHost, manager.HostForTesting);

        await manager.DisposeAsync();
    }

    [Fact]
    public async Task StopAsync_ClearsHostAndWorkers()
    {
        var configuration = new Configuration { BasePath = "http://127.0.0.1:1/api" };
        var manager = new WorkerManager(http: null!, configuration, pollIntervalMs: 60_000, threadCount: 1);
        manager.RegisterAgentTools(AgentWithOneTool("agent_one"));

        await manager.StartAsync();
        Assert.NotNull(manager.HostForTesting);

        await manager.StopAsync();

        Assert.Null(manager.HostForTesting);
    }

    [Fact]
    public async Task StartAsync_NoWorkersRegistered_StillBuildsHost()
    {
        var configuration = new Configuration { BasePath = "http://127.0.0.1:1/api" };
        var manager = new WorkerManager(http: null!, configuration, pollIntervalMs: 60_000, threadCount: 1);

        await manager.StartAsync();

        Assert.NotNull(manager.HostForTesting);
        await manager.DisposeAsync();
    }
}
