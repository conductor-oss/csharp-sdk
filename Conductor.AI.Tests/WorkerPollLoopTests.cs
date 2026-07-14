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
// Spec R9 / DD2: WorkerManager.Start() must be idempotent — a shared manager
// can have Start() invoked more than once (e.g. overlapping runs sharing the
// same AgentRuntime), and re-starting an already-polling loop would spawn a
// second full set of poll tasks, double-dequeuing tasks of that type.

using System.Threading.Tasks;
using Conductor.Api;
using Conductor.Client;
using Xunit;

namespace Conductor.AI.Tests;

public sealed class WorkerPollLoopTests
{
    [Fact]
    public async Task Start_CalledTwice_OnlySpawnsPollTasksOnce()
    {
        // A poll interval far longer than the test's lifetime — no real HTTP
        // poll attempt should occur before DisposeAsync cancels the loop.
        var taskClient = new TaskResourceApi(new Configuration { BasePath = "http://127.0.0.1:1/api" });
        var loop = new WorkerPollLoop(
            taskClient, http: null!, taskName: "noop_task",
            handler: (_, _) => Task.FromResult<object?>(null),
            pollIntervalMs: 60_000, threadCount: 2);

        loop.Start();
        loop.Start(); // must be a no-op the second time

        Assert.Equal(2, loop.PollTaskCount);

        await loop.DisposeAsync();
    }
}
