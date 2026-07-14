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
// R13/T18 — swarm transfer contract: transfer-to workers echo an optional
// hand-off message, and each agent's check_transfer worker does first-wins
// selection over tool_calls, tolerating both inputParameters and arguments
// key shapes, and surfacing dropped_transfers when more than one transfer
// call lands in the same turn.

using Conductor.Client;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Conductor.AI.Tests;

public sealed class SwarmTransferTests
{
    private static (WorkerManager Manager, Agent Root) BuildSwarm()
    {
        var configuration = new Configuration { BasePath = "http://127.0.0.1:1/api" };
        var manager = new WorkerManager(configuration, pollIntervalMs: 60_000, threadCount: 1);
        var root = new Agent("agent_one")
        {
            Model = "openai/gpt-4o",
            Strategy = Strategy.Swarm,
            Agents = [new Agent("agent_two") { Model = "openai/gpt-4o" }, new Agent("agent_three") { Model = "openai/gpt-4o" }],
        };
        manager.RegisterAgentTools(root);
        return (manager, root);
    }

    private static Conductor.Client.Models.Task NewTask(Dictionary<string, object> inputData) => new()
    {
        WorkflowInstanceId = "wf-1",
        TaskId = "task-1",
        InputData = inputData,
    };

    [Fact]
    public async Task TransferWorker_WithMessage_EchoesItInOutput()
    {
        var (manager, _) = BuildSwarm();
        var worker = manager.WorkerForTesting("agent_one_transfer_to_agent_two");
        Assert.NotNull(worker);

        var result = await worker!.Execute(NewTask(new Dictionary<string, object> { ["message"] = "handing off to you" }), CancellationToken.None);

        Assert.Equal("handing off to you", result.OutputData["message"]);
    }

    [Fact]
    public async Task TransferWorker_NoMessage_ProducesEmptyOutput()
    {
        var (manager, _) = BuildSwarm();
        var worker = manager.WorkerForTesting("agent_one_transfer_to_agent_two");
        Assert.NotNull(worker);

        var result = await worker!.Execute(NewTask(new Dictionary<string, object>()), CancellationToken.None);

        Assert.Empty(result.OutputData);
    }

    [Fact]
    public async Task CheckTransfer_NoToolCalls_ReportsNoTransfer()
    {
        var (manager, _) = BuildSwarm();
        var worker = manager.WorkerForTesting("agent_one_check_transfer");
        Assert.NotNull(worker);

        var result = await worker!.Execute(NewTask(new Dictionary<string, object>()), CancellationToken.None);

        Assert.Equal(false, result.OutputData["is_transfer"]);
        Assert.Equal("", result.OutputData["transfer_to"]);
        Assert.Equal("", result.OutputData["transfer_message"]);
        Assert.DoesNotContain("dropped_transfers", result.OutputData.Keys);
    }

    [Fact]
    public async Task CheckTransfer_SingleTransferCall_ExtractsTargetAndMessageFromInputParameters()
    {
        var (manager, _) = BuildSwarm();
        var worker = manager.WorkerForTesting("agent_one_check_transfer");
        Assert.NotNull(worker);

        var task = NewTask(new Dictionary<string, object>
        {
            ["tool_calls"] = new List<object>
            {
                new Dictionary<string, object>
                {
                    ["name"] = "agent_one_transfer_to_agent_two",
                    ["inputParameters"] = new Dictionary<string, object> { ["message"] = "please continue" },
                },
            },
        });

        var result = await worker!.Execute(task, CancellationToken.None);

        Assert.Equal(true, result.OutputData["is_transfer"]);
        Assert.Equal("agent_two", result.OutputData["transfer_to"]);
        Assert.Equal("please continue", result.OutputData["transfer_message"]);
        Assert.DoesNotContain("dropped_transfers", result.OutputData.Keys);
    }

    [Fact]
    public async Task CheckTransfer_ArgumentsKeyVariant_IsTolerated()
    {
        var (manager, _) = BuildSwarm();
        var worker = manager.WorkerForTesting("agent_one_check_transfer");
        Assert.NotNull(worker);

        var task = NewTask(new Dictionary<string, object>
        {
            ["tool_calls"] = new List<object>
            {
                new Dictionary<string, object>
                {
                    ["name"] = "agent_one_transfer_to_agent_three",
                    ["arguments"] = new Dictionary<string, object> { ["message"] = "via arguments shape" },
                },
            },
        });

        var result = await worker!.Execute(task, CancellationToken.None);

        Assert.Equal(true, result.OutputData["is_transfer"]);
        Assert.Equal("agent_three", result.OutputData["transfer_to"]);
        Assert.Equal("via arguments shape", result.OutputData["transfer_message"]);
    }

    [Fact]
    public async Task CheckTransfer_MultipleTransferCalls_FirstWinsAndRestAreDropped()
    {
        var (manager, _) = BuildSwarm();
        var worker = manager.WorkerForTesting("agent_one_check_transfer");
        Assert.NotNull(worker);

        var task = NewTask(new Dictionary<string, object>
        {
            ["tool_calls"] = new List<object>
            {
                new Dictionary<string, object>
                {
                    ["name"] = "agent_one_transfer_to_agent_two",
                    ["inputParameters"] = new Dictionary<string, object> { ["message"] = "first" },
                },
                new Dictionary<string, object>
                {
                    ["name"] = "agent_one_transfer_to_agent_three",
                    ["inputParameters"] = new Dictionary<string, object> { ["message"] = "second" },
                },
            },
        });

        var result = await worker!.Execute(task, CancellationToken.None);

        Assert.Equal(true, result.OutputData["is_transfer"]);
        Assert.Equal("agent_two", result.OutputData["transfer_to"]);
        Assert.Equal("first", result.OutputData["transfer_message"]);

        var dropped = Assert.IsType<JArray>(result.OutputData["dropped_transfers"]);
        Assert.Single(dropped);
        Assert.Equal("agent_three", dropped[0]["transfer_to"]!.ToString());
        Assert.Equal("second", dropped[0]["message"]!.ToString());
    }
}
