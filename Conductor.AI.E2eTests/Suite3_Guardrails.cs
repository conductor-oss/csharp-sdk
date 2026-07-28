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
// Suite 3 — Guardrail execution: verify guardrail function bodies run, that
// server-evaluated (regex/llm) guardrails register no local worker at all,
// and that retry escalation actually fails a run instead of silently
// exhausting max_turns.
//
// Validation: Interlocked counters on the guardrail function body plus
// AgentResult.Status checks. We do NOT assert on LLM output text.
//
// CLAUDE.md rule: no LLM for validation; write test → make it fail → confirm failure.

using System.Text.RegularExpressions;
using System.Threading;
using Xunit;
using Conductor.AI.Examples;

namespace Conductor.AI.E2eTests;

[Collection("E2e")]
public sealed class Suite3_Guardrails
{
    private readonly E2eFixture _fixture;

    public Suite3_Guardrails(E2eFixture fixture) => _fixture = fixture;

    // ── 3.1  Output guardrail function body executes ─────────────────────

    [SkippableFact]
    public async Task OutputGuardrail_FunctionBodyExecutes()
    {
        _fixture.RequireServer();

        var host = new S3GuardrailHost();
        var guardrails = GuardrailRegistry.FromInstance(host);

        var agent = new Agent("s3_guardrail_exec")
        {
            Model = Settings.LlmModel,
            Instructions = "You are a helpful assistant. Answer concisely.",
            Guardrails = guardrails,
        };

        await using var runtime = new AgentRuntime();
        var result = await runtime.RunAsync(agent, "Say hello.");

        // The guardrail function must have been invoked at least once
        Assert.True(host.CheckCount > 0,
            $"Expected guardrail to execute but CheckCount was {host.CheckCount}.");
        Assert.True(result.IsSuccess || result.IsFailed,  // either outcome is valid
            $"Unexpected agent status: {result.Status}");
    }

    // ── 3.2  Passing guardrail: agent succeeds ───────────────────────────

    [SkippableFact]
    public async Task PassingGuardrail_AgentSucceeds()
    {
        _fixture.RequireServer();

        var host = new S3AlwaysPassGuardrailHost();
        var guardrails = GuardrailRegistry.FromInstance(host);

        var agent = new Agent("s3_pass_guardrail")
        {
            Model = Settings.LlmModel,
            Instructions = "Answer briefly.",
            Guardrails = guardrails,
        };

        await using var runtime = new AgentRuntime();
        var result = await runtime.RunAsync(agent, "What is 2+2?");

        Assert.True(host.CallCount > 0,
            "Expected always-pass guardrail to be called but it wasn't.");
        Assert.True(result.IsSuccess, $"Agent failed unexpectedly: {result.Error}");
    }

    // ── 3.3  Custom guardrail with a .NET regex check blocks PII ─────────
    // custom guardrail using System.Text.RegularExpressions in its handler —
    // not the server-evaluated RegexGuardrail.Create (that's 3.5)

    [SkippableFact]
    public async Task CustomGuardrail_DotNetRegexCheck_BlocksPii()
    {
        _fixture.RequireServer();

        var host = new S3PiiGuardrailHost();
        var tools = ToolRegistry.FromInstance(new S3PiiToolHost());
        var guardrails = GuardrailRegistry.FromInstance(host);

        var agent = new Agent("s3_pii_guardrail")
        {
            Model = Settings.LlmModel,
            Instructions = "You are a customer service agent. Use get_customer_info to answer.",
            Tools = tools,
            Guardrails = guardrails,
        };

        await using var runtime = new AgentRuntime();
        var result = await runtime.RunAsync(
            agent, "Get info for customer CUST-1 and tell me everything.");

        // Guardrail function MUST have been invoked
        Assert.True(host.CheckCount > 0,
            $"PII guardrail was never called. CheckCount={host.CheckCount}");

        // Agent must either succeed (LLM redacted PII) or fail (gave up after retries)
        // We do NOT assert on the LLM text output.
        Assert.True(result.IsSuccess || result.IsFailed,
            $"Unexpected status: {result.Status}");
    }

    // ── 3.4  Tool-level guardrail actually blocks a violating call ───────
    // real GuardrailDef attached via WithGuardrails; run must fail on violation

    [SkippableFact]
    public async Task ToolGuardrail_BlocksViolatingCall_RunFails()
    {
        _fixture.RequireServer();

        var guardHost = new S3ToolGuardrailDefHost();
        var guardrail = GuardrailRegistry.FromInstance(guardHost).Single();

        var dbHost = new S3DbToolHost();
        var tools = ToolRegistry.FromInstance(dbHost)
            .Select(t => t.Name == "run_query" ? t.WithGuardrails(guardrail) : t)
            .ToList();

        var agent = new Agent("s3_tool_guardrail_raise")
        {
            Model = Settings.LlmModel,
            Instructions = "You must call the run_query tool with the exact argument "
                + "query=\"DROP TABLE users\" and nothing else.",
            Tools = tools,
        };

        await using var runtime = new AgentRuntime();
        var result = await runtime.RunAsync(agent, "Run the query as instructed.");

        Assert.True(guardHost.CheckCount > 0,
            $"Expected the tool guardrail to be invoked. CheckCount={guardHost.CheckCount}");
        Assert.True(result.IsFailed || result.Status == Status.Terminated,
            $"Expected the run to fail after the tool guardrail raised, got {result.Status}.");
        Assert.Equal(0, dbHost.QueryCallCount);  // blocked pre-execution — the tool body never ran
    }

    // ── 3.5  RegexGuardrail.Create is server-evaluated: no local worker ──

    [SkippableFact]
    public async Task RegexGuardrail_ServerEvaluated_NoLocalWorkerRegistered()
    {
        _fixture.RequireServer();

        var guard = RegexGuardrail.Create(
            pattern: @"\d{3}-\d{2}-\d{4}",
            name: "no_ssn_server_side",
            message: "Response must not contain a social security number.",
            onFail: OnFail.Raise);

        var agent = new Agent("s3_regex_server_evaluated")
        {
            Model = Settings.LlmModel,
            Instructions = "Answer briefly.",
            Guardrails = [guard],
        };

        await using var runtime = new AgentRuntime();
        var result = await runtime.RunAsync(agent, "Say hello.");

        // Server-evaluated guardrails carry no local Handler and register no
        // worker — the Conductor server compiles and runs them itself as an
        // INLINE GraalJS task.
        Assert.Null(runtime.WorkerForTesting("s3_regex_server_evaluated_output_guardrail"));
        Assert.Null(runtime.WorkerForTesting("no_ssn_server_side"));
        Assert.True(result.IsSuccess || result.IsFailed, $"Unexpected status: {result.Status}");
    }

    // ── 3.6  LLMGuardrail.Create is server-evaluated: no local worker ────

    [SkippableFact]
    public async Task LlmGuardrail_ServerEvaluated_NoLocalWorkerRegistered()
    {
        _fixture.RequireServer();

        var guard = LLMGuardrail.Create(
            model: Settings.LlmModel,
            policy: "Reject any response that contains a number.",
            name: "no_numbers_server_side",
            onFail: OnFail.Raise);

        var agent = new Agent("s3_llm_server_evaluated")
        {
            Model = Settings.LlmModel,
            Instructions = "Answer briefly.",
            Guardrails = [guard],
        };

        await using var runtime = new AgentRuntime();
        var result = await runtime.RunAsync(agent, "What is 2+2? Just give the number.");

        Assert.Null(runtime.WorkerForTesting("s3_llm_server_evaluated_output_guardrail"));
        Assert.Null(runtime.WorkerForTesting("no_numbers_server_side"));
        Assert.True(result.IsSuccess || result.IsFailed, $"Unexpected status: {result.Status}");
    }

    // ── 3.7  Agent-level RETRY escalates to RAISE after maxRetries ───────
    // always-fail guardrail, maxRetries=1 → run must fail before the turn cap;
    // COMPLETED would mean the loop exhausted max_turns without escalating

    [SkippableFact]
    public async Task AgentGuardrail_RetryEscalatesToRaise_AfterMaxRetries()
    {
        _fixture.RequireServer();

        var host = new S3AlwaysFailGuardrailHost();
        var guardrails = GuardrailRegistry.FromInstance(host);

        var agent = new Agent("s3_retry_escalation")
        {
            Model = Settings.LlmModel,
            Instructions = "Answer briefly.",
            Guardrails = guardrails,
            MaxTurns = 3,
        };

        await using var runtime = new AgentRuntime();
        var result = await runtime.RunAsync(agent, "Say hello.");

        Assert.True(host.CheckCount > 0, "Expected the guardrail to run at least once.");
        Assert.True(result.IsFailed || result.Status == Status.Terminated,
            $"Expected escalation to fail the run, got {result.Status}.");
        Assert.False(string.IsNullOrEmpty(result.Error),
            "Expected the guardrail failure message in Error, got null/empty.");
    }

    // ── 3.8  Tool-level RETRY escalates to RAISE after maxRetries ───────
    // same as 3.7 for a tool guardrail; env-gated — the server hardcodes the
    // tool-guardrail iteration ref to "1", so retry never escalates today.
    // set E2E_TOOL_GUARDRAIL_ESCALATION=true once the server fix ships

    [SkippableFact]
    public async Task ToolGuardrail_RetryEscalatesToRaise_AfterMaxRetries()
    {
        _fixture.RequireServer();
        Skip.IfNot(
            Environment.GetEnvironmentVariable("E2E_TOOL_GUARDRAIL_ESCALATION") == "true",
            "Tool-guardrail retry escalation is disabled server-side — set "
            + "E2E_TOOL_GUARDRAIL_ESCALATION=true once the server fix ships.");

        var guardHost = new S3AlwaysFailToolGuardrailHost();
        var guardrail = GuardrailRegistry.FromInstance(guardHost).Single();

        var tools = ToolRegistry.FromInstance(new S3DbToolHost())
            .Select(t => t.Name == "run_query" ? t.WithGuardrails(guardrail) : t)
            .ToList();

        var agent = new Agent("s3_tool_retry_escalation")
        {
            Model = Settings.LlmModel,
            Instructions = "Call run_query with any query to answer database questions.",
            Tools = tools,
            MaxTurns = 3,
        };

        await using var runtime = new AgentRuntime();
        var result = await runtime.RunAsync(agent, "Look up all users.");

        Assert.True(guardHost.CheckCount > 0, "Expected the tool guardrail to run at least once.");
        Assert.True(result.IsFailed || result.Status == Status.Terminated,
            $"Expected escalation to fail the run, got {result.Status}.");
    }
}

// ── Guardrail and tool hosts ──────────────────────────────────────────────────

internal sealed class S3GuardrailHost
{
    private int _checkCount;
    public int CheckCount => _checkCount;

    [Guardrail(Position = Position.Output, OnFail = OnFail.Retry, MaxRetries = 1)]
    public GuardrailResult CheckOutput(string content)
    {
        Interlocked.Increment(ref _checkCount);
        // Always pass — we only want to verify the body ran.
        return new GuardrailResult(true);
    }
}

internal sealed class S3AlwaysPassGuardrailHost
{
    private int _callCount;
    public int CallCount => _callCount;

    [Guardrail(Position = Position.Output, OnFail = OnFail.Raise)]
    public GuardrailResult AlwaysPass(string content)
    {
        Interlocked.Increment(ref _callCount);
        return new GuardrailResult(true);
    }
}

internal sealed class S3PiiGuardrailHost
{
    private static readonly Regex CcPattern = new(@"\b\d{4}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}\b");

    private int _checkCount;
    public int CheckCount => _checkCount;

    [Guardrail(Position = Position.Output, OnFail = OnFail.Retry, MaxRetries = 2)]
    public GuardrailResult NoPii(string content)
    {
        Interlocked.Increment(ref _checkCount);
        if (CcPattern.IsMatch(content))
            return new GuardrailResult(false, "Response contains a credit card number. Redact it.");
        return new GuardrailResult(true);
    }
}

internal sealed class S3PiiToolHost
{
    [Tool("Get customer information including payment details.")]
    public Dictionary<string, object> GetCustomerInfo(string customerId) => new()
    {
        ["customer_id"] = customerId,
        ["name"] = "Test User",
        ["card"] = "4532-0150-1234-5678",  // PII intentionally in tool output
    };
}

internal sealed class S3DbToolHost
{
    private int _queryCalls;
    public int QueryCallCount => _queryCalls;

    [Tool("Execute a database query.")]
    public string RunQuery(string query)
    {
        Interlocked.Increment(ref _queryCalls);
        return "Results: [('Alice', 30), ('Bob', 25)]";
    }
}

internal sealed class S3ToolGuardrailDefHost
{
    private static readonly Regex DangerPattern = new(@"DROP\s+TABLE|DELETE\s+FROM", RegexOptions.IgnoreCase);

    private int _checkCount;
    public int CheckCount => _checkCount;

    [Guardrail("no_drop_table", Position = Position.Input, OnFail = OnFail.Raise)]
    public GuardrailResult NoDestructiveSql(string content)
    {
        Interlocked.Increment(ref _checkCount);
        if (DangerPattern.IsMatch(content))
            return new GuardrailResult(false, "Destructive SQL detected in tool call.");
        return new GuardrailResult(true);
    }
}

internal sealed class S3AlwaysFailGuardrailHost
{
    private int _checkCount;
    public int CheckCount => _checkCount;

    [Guardrail(Position = Position.Output, OnFail = OnFail.Retry, MaxRetries = 1)]
    public GuardrailResult AlwaysFail(string content)
    {
        Interlocked.Increment(ref _checkCount);
        return new GuardrailResult(false, "always fails, to force escalation");
    }
}

internal sealed class S3AlwaysFailToolGuardrailHost
{
    private int _checkCount;
    public int CheckCount => _checkCount;

    [Guardrail(Position = Position.Input, OnFail = OnFail.Retry, MaxRetries = 1)]
    public GuardrailResult AlwaysFail(string content)
    {
        Interlocked.Increment(ref _checkCount);
        return new GuardrailResult(false, "always fails, to force escalation");
    }
}
