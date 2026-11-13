// Suite 21 — Decision (Jev) routing.
//
// Corresponds to examples:
//   117_DecisionRouter — decision ToolDef as Strategy.Router selector
//   118_DecisionTool   — decision ToolDef called by a chat agent
//
// Validation strategy: plan-only. The server compiles both shapes without calling
// a Decision provider or an LLM, so these run with no keys. Needs Conductor OSS
// 3.33.0+; older servers skip.

using System.Net.Http;
using System.Text.Json.Nodes;
using Xunit;
using Conductor.AI.Examples;

namespace Conductor.AI.E2eTests;

[Collection("E2e")]
public sealed class Suite21_DecisionRouting
{
    private static readonly Version MinServer = new(3, 33, 0);
    private readonly E2eFixture _fixture;

    public Suite21_DecisionRouting(E2eFixture fixture) => _fixture = fixture;

    // ── 21.1  Decision router compiles to one decision SWITCH over the agents ──

    [SkippableFact]
    public async Task DecisionRouter_CompilesToDecisionSwitch()
    {
        await RequireDecisionServerAsync();

        // PascalCase names: choice keys must reach the server verbatim to match.
        var agent = new Agent("s21_decision_router")
        {
            Strategy = Strategy.Router,
            DecisionRouter = new ToolDef
            {
                Name = "s21_selector",
                ToolType = "decision",
                Config = new Dictionary<string, object>
                {
                    ["model"] = "jev-1.13",
                    ["questions"] = new
                    {
                        Team = new
                        {
                            type = "choice",
                            choices = new { S21Billing = "Invoices", S21Technical = "Errors" },
                        },
                    },
                },
            },
            Agents =
            [
                new Agent("S21Billing") { Model = Settings.LlmModel },
                new Agent("S21Technical") { Model = Settings.LlmModel },
            ],
            MaxTurns = 1,
            Synthesize = false,
        };

        await using var runtime = new AgentRuntime();
        var plan = await runtime.PlanAsync(agent);

        var decisions = E2eHelpers.AllTasksFlat(plan?["workflowDef"])
            .Where(t => t["evaluatorType"]?.GetValue<string>() == "decision")
            .ToList();
        var router = Assert.Single(decisions);
        Assert.Equal("SWITCH", router["type"]!.GetValue<string>());
        Assert.Equal("Team", router["expression"]!.GetValue<string>());
        Assert.Equal(["S21Billing", "S21Technical"],
            router["decisionCases"]!.AsObject().Select(kv => kv.Key).OrderBy(k => k));
        Assert.Equal("jev-1.13", router["inputParameters"]!["model"]!.GetValue<string>());
        Assert.False(router["inputParameters"]!.AsObject().ContainsKey("provider"));
    }

    // ── 21.2  Decision tool compiles to a SWITCH tool with no worker ──────────

    [SkippableFact]
    public async Task DecisionTool_CompilesToSwitchToolWithoutWorker()
    {
        await RequireDecisionServerAsync();

        var agent = new Agent("s21_decision_tool")
        {
            Model = Settings.LlmModel,
            Instructions = "Call s21_classify before answering.",
            Tools =
            [
                new ToolDef
                {
                    Name = "s21_classify",
                    Description = "Classify the request.",
                    ToolType = "decision",
                    InputSchema = JsonNode.Parse(
                        """{"type":"object","properties":{"state":{"type":"string"}},"required":["state"]}""")!.AsObject(),
                    Config = new Dictionary<string, object>
                    {
                        ["model"] = "jev-1.13",
                        ["questions"] = new Dictionary<string, object>
                        {
                            ["department"] = new Dictionary<string, object>
                            {
                                ["type"] = "choice",
                                ["choices"] = new Dictionary<string, object> { ["billing"] = "B", ["technical"] = "T" },
                            },
                        },
                    },
                },
            ],
        };

        await using var runtime = new AgentRuntime();
        var plan = await runtime.PlanAsync(agent);

        var llmTools = E2eHelpers.AllTasksFlat(plan?["workflowDef"])
            .Where(t => t["type"]?.GetValue<string>() == "LLM_CHAT_COMPLETE")
            .SelectMany(t => t["inputParameters"]?["tools"]?.AsArray() ?? [])
            .Where(t => t?["name"]?.GetValue<string>() == "s21_classify")
            .ToList();
        Assert.NotEmpty(llmTools);
        Assert.All(llmTools, t => Assert.Equal("SWITCH", t!["type"]!.GetValue<string>()));

        var workers = plan?["requiredWorkers"]?.AsArray().Select(w => w?.ToJsonString()) ?? [];
        Assert.DoesNotContain(workers, w => w is not null && w.Contains("s21_classify"));
    }

    private async Task RequireDecisionServerAsync()
    {
        _fixture.RequireServer();
        var url = (Environment.GetEnvironmentVariable("CONDUCTOR_SERVER_URL") ?? "http://localhost:8080/api")
            .TrimEnd('/') + "/version";
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        var raw = (await http.GetStringAsync(url)).Trim().Trim('"');
        var core = raw.Split('-', '+')[0];
        Skip.IfNot(Version.TryParse(core, out var version) && version >= MinServer,
            $"Decision routing needs Conductor OSS {MinServer}+, server reports '{raw}'.");
    }
}
