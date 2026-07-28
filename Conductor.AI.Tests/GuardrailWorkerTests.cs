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
// GuardrailHandlerFactory: combined per-scope handler for local (custom) guardrails.
// Registration: one combined worker per scope ({scope}_output_guardrail), dedup
// against double registration.

using System.Text.Json;
using Conductor.Client;
using Xunit;

namespace Conductor.AI.Tests;

public sealed class GuardrailWorkerTests
{
    private static Dictionary<string, JsonElement> Args(string content, int? iteration = null)
    {
        var dict = new Dictionary<string, JsonElement> { ["content"] = JsonSerializer.SerializeToElement(content) };
        if (iteration is not null) dict["iteration"] = JsonSerializer.SerializeToElement(iteration.Value);
        return dict;
    }

    private static GuardrailDef Custom(
        string name, OnFail onFail, Func<string, GuardrailResult> fn, int maxRetries = 3) => new()
    {
        Name = name,
        OnFail = onFail,
        MaxRetries = maxRetries,
        Handler = content => Task.FromResult(fn(content)),
    };

    // ── handler factory ───────────────────────────────────────────────────

    [Fact]
    public async Task Factory_EvaluatesInOrder_FirstFailureWins()
    {
        var calls = new List<string>();
        var first = Custom("first", OnFail.Raise, c => { calls.Add("first"); return new GuardrailResult(false, "bad"); });
        var second = Custom("second", OnFail.Raise, c => { calls.Add("second"); return new GuardrailResult(true); });

        var handler = GuardrailHandlerFactory.Create([first, second]);
        var result = (Dictionary<string, object?>)(await handler(Args("x"), null))!;

        Assert.Equal(["first"], calls);
        Assert.False((bool)result["passed"]!);
        Assert.Equal("first", result["guardrail_name"]);
        Assert.Equal("raise", result["on_fail"]);
    }

    [Fact]
    public async Task Factory_AllPass_ReturnsPassShape()
    {
        var g = Custom("g", OnFail.Raise, _ => new GuardrailResult(true));
        var handler = GuardrailHandlerFactory.Create([g]);
        var result = (Dictionary<string, object?>)(await handler(Args("x"), null))!;

        Assert.True((bool)result["passed"]!);
        Assert.Equal("pass", result["on_fail"]);
        Assert.Equal("", result["guardrail_name"]);
        Assert.False((bool)result["should_continue"]!);
    }

    [Fact]
    public async Task Factory_Retry_EscalatesToRaise_AtMaxRetries()
    {
        var g = Custom("g", OnFail.Retry, _ => new GuardrailResult(false, "bad"), maxRetries: 2);
        var handler = GuardrailHandlerFactory.Create([g]);

        var underLimit = (Dictionary<string, object?>)(await handler(Args("x", iteration: 1), null))!;
        Assert.Equal("retry", underLimit["on_fail"]);
        Assert.True((bool)underLimit["should_continue"]!);

        var atLimit = (Dictionary<string, object?>)(await handler(Args("x", iteration: 2), null))!;
        Assert.Equal("raise", atLimit["on_fail"]);
        Assert.False((bool)atLimit["should_continue"]!);
    }

    [Fact]
    public async Task Factory_MissingIteration_TreatedAsZero()
    {
        // maxRetries=0 means even iteration 0 must escalate immediately.
        var g = Custom("g", OnFail.Retry, _ => new GuardrailResult(false, "bad"), maxRetries: 0);
        var handler = GuardrailHandlerFactory.Create([g]);

        var result = (Dictionary<string, object?>)(await handler(Args("x"), null))!;
        Assert.Equal("raise", result["on_fail"]);
    }

    [Fact]
    public async Task Factory_Fix_EscalatesToRaise_WhenNoFixedOutput()
    {
        var g = Custom("g", OnFail.Fix, _ => new GuardrailResult(false, "bad"));
        var handler = GuardrailHandlerFactory.Create([g]);

        var result = (Dictionary<string, object?>)(await handler(Args("x"), null))!;
        Assert.Equal("raise", result["on_fail"]);
    }

    [Fact]
    public async Task Factory_Fix_KeepsFix_WhenFixedOutputPresent()
    {
        var g = Custom("g", OnFail.Fix, _ => new GuardrailResult(false, "bad", FixedOutput: "cleaned"));
        var handler = GuardrailHandlerFactory.Create([g]);

        var result = (Dictionary<string, object?>)(await handler(Args("x"), null))!;
        Assert.Equal("fix", result["on_fail"]);
        Assert.Equal("cleaned", result["fixed_output"]);
    }

    [Fact]
    public async Task Factory_HandlerException_Propagates()
    {
        var g = Custom("g", OnFail.Raise, _ => throw new InvalidOperationException("boom"));
        var handler = GuardrailHandlerFactory.Create([g]);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler(Args("x"), null));
    }

    // ── registration: one combined worker per scope, dedup ────────────────

    private static WorkerManager NewManager() =>
        new(new Configuration { BasePath = "http://127.0.0.1:1/api" }, pollIntervalMs: 60_000, threadCount: 1);

    private static Conductor.Client.Models.Task NewTask(string content, int? iteration = null)
    {
        var input = new Dictionary<string, object> { ["content"] = content };
        if (iteration is not null) input["iteration"] = iteration.Value;
        return new() { WorkflowInstanceId = "wf-1", TaskId = "task-1", InputData = input };
    }

    private sealed class MixedGuardrailHost
    {
        [Guardrail("check_one")]
        public GuardrailResult CheckOne(string content) => new(true);

        [Guardrail("check_two")]
        public GuardrailResult CheckTwo(string content) => new(true);
    }

    [Fact]
    public void OneOfEachType_RegistersExactlyOneWorker_NamedForScope()
    {
        var manager = NewManager();
        var agent = new Agent("support_agent")
        {
            Model = "openai/gpt-4o",
            Guardrails =
            [
                .. GuardrailRegistry.FromInstance(new MixedGuardrailHost()),
                RegexGuardrail.Create("secret", name: "no_secret"),
                LLMGuardrail.Create("openai/gpt-4o-mini", "no bad stuff", name: "safety"),
            ],
        };

        manager.RegisterAgentTools(agent);

        Assert.NotNull(manager.WorkerForTesting("support_agent_output_guardrail"));
        // Regex/llm have no Handler — they register nothing (server-evaluated).
        Assert.Null(manager.WorkerForTesting("no_secret"));
        Assert.Null(manager.WorkerForTesting("safety"));
    }

    [Fact]
    public void ToolScope_RegistersWorker_NamedForTool()
    {
        var manager = NewManager();
        var tool = new ToolDef
        {
            Name = "run_query",
            Description = "run a query",
            Guardrails = [.. GuardrailRegistry.FromInstance(new MixedGuardrailHost())],
        };
        var agent = new Agent("support_agent") { Model = "openai/gpt-4o", Tools = [tool] };

        manager.RegisterAgentTools(agent);

        Assert.NotNull(manager.WorkerForTesting("run_query_output_guardrail"));
    }

    [Fact]
    public async Task CombinedWorker_EvaluatesBothGuardrails_FirstFailureWins()
    {
        var manager = NewManager();
        var agent = new Agent("support_agent")
        {
            Model = "openai/gpt-4o",
            Guardrails = [.. GuardrailRegistry.FromInstance(new MixedGuardrailHost())],
        };
        manager.RegisterAgentTools(agent);

        var worker = manager.WorkerForTesting("support_agent_output_guardrail");
        Assert.NotNull(worker);

        var result = await worker!.Execute(NewTask("hello"), CancellationToken.None);
        Assert.True((bool)result.OutputData["passed"]!);
    }

    [Fact]
    public void DuplicateRegistration_SameScope_RegistersOnlyOneWorker()
    {
        var manager = NewManager();
        var agent = new Agent("support_agent")
        {
            Model = "openai/gpt-4o",
            Guardrails = [.. GuardrailRegistry.FromInstance(new MixedGuardrailHost())],
        };

        manager.RegisterAgentTools(agent);
        manager.RegisterAgentTools(agent); // re-registered, e.g. overlapping runs

        Assert.Equal(1, manager.WorkerCountForTesting("support_agent_output_guardrail"));
    }

    [Fact]
    public void AgentAndTool_SharingScopeName_RegisterOnlyOneWorker()
    {
        // An agent-level guardrail and a tool named the same as the agent both
        // want "x_output_guardrail" — without dedup this would double-poll one
        // Conductor task queue with two competing handlers.
        var manager = NewManager();
        var tool = new ToolDef
        {
            Name = "x",
            Description = "d",
            Guardrails = [.. GuardrailRegistry.FromInstance(new MixedGuardrailHost())],
        };
        var agent = new Agent("x")
        {
            Model = "openai/gpt-4o",
            Tools = [tool],
            Guardrails = [.. GuardrailRegistry.FromInstance(new MixedGuardrailHost())],
        };

        manager.RegisterAgentTools(agent);

        Assert.Equal(1, manager.WorkerCountForTesting("x_output_guardrail"));
    }
}
