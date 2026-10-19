using Conductor.Definition.TaskType;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Conductor.AI.Tests;

public class DecisionRoutingTests
{
    private static ToolDef Selector(string? provider = null) => new()
    {
        Name = "selector",
        ToolType = "decision",
        Config = new Dictionary<string, object>
        {
            ["model"] = "jev-1.13",
            ["questions"] = new { agent = new { type = "choice", choices = new { billing = "Invoices" } } },
            // A null provider must be omitted, not sent as JSON null.
            ["provider"] = provider!,
        },
    };

    [Theory]
    [InlineData(null)]
    [InlineData("openrouter")]
    public void DecisionRouterEmitsSelectorWithoutAgentOrWorkerFields(string? provider)
    {
        var agent = new Agent("support")
        {
            Strategy = Strategy.Router,
            DecisionRouter = Selector(provider),
            Agents = [new Agent("billing") { Model = "openai/gpt-4o-mini" }],
            Synthesize = false,
        };
        var config = AgentConfigSerializer.SerializeAgent(agent);
        var router = config["router"]!.AsObject();
        Assert.Equal("decision", router["kind"]!.GetValue<string>());
        Assert.Equal("jev-1.13", router["model"]!.GetValue<string>());
        Assert.Equal("Invoices", router["questions"]!["agent"]!["choices"]!["billing"]!.GetValue<string>());
        Assert.Equal(provider is null ? 4 : 5, router.Count);
        Assert.Equal(provider, router["provider"]?.GetValue<string>());
        Assert.False(router.ContainsKey("provider") && provider is null);
        Assert.Null(config["external"]);
        Assert.False(config["synthesize"]!.GetValue<bool>());
    }

    [Theory]
    [InlineData("worker", "jev-1.13", true)]
    [InlineData("decision", null, true)]
    [InlineData("decision", " ", true)]
    [InlineData("decision", "jev-1.13", false)]
    public void InvalidDecisionSelectorsFailBeforeSubmission(string toolType, string? model, bool hasQuestions)
    {
        var config = new Dictionary<string, object> { ["model"] = model! };
        if (hasQuestions) config["questions"] = Selector().Config!["questions"];
        var agent = new Agent("support")
        {
            DecisionRouter = new ToolDef { Name = "selector", ToolType = toolType, Config = config },
        };
        Assert.Throws<InvalidOperationException>(() => AgentConfigSerializer.SerializeAgent(agent));
        config["questions"] = new Dictionary<string, object>();
        Assert.Throws<InvalidOperationException>(() => AgentConfigSerializer.SerializeAgent(agent));
    }

    [Fact]
    public void RouterBuilderPreservesAgentApiAndLastSetterWins()
    {
        var legacy = new Agent("classifier") { Model = "openai/gpt-4o-mini" };
        var builder = AgentBuilder.Create("support").WithRouter(Selector()).WithRouter(legacy);
        var agent = builder.Build();
        Assert.Same(legacy, agent.Router);
        Assert.Null(agent.DecisionRouter);
        Assert.Equal("classifier", AgentConfigSerializer.SerializeAgent(agent)["router"]!["name"]!.GetValue<string>());
        builder.WithRouter(Selector());
        Assert.Null(agent.Router);
        Assert.NotNull(agent.DecisionRouter);
        agent.Router = legacy;
        Assert.Throws<InvalidOperationException>(() => AgentConfigSerializer.SerializeAgent(agent));
    }

    [Fact]
    public void DecisionToolRetainsToolWireContract()
    {
        var config = AgentConfigSerializer.SerializeAgent(new Agent("support") { Tools = [Selector()] });
        var tool = config["tools"]![0]!;
        Assert.Equal("decision", tool["toolType"]!.GetValue<string>());
        Assert.Equal("jev-1.13", tool["config"]!["model"]!.GetValue<string>());
        Assert.Null(tool["taskName"]);
    }

    [Fact]
    public void DecisionSwitchPreservesExpressionAndLegacyValueParamBehavior()
    {
        var decision = new SwitchTask("route_request", "route", useJavascript: true)
        {
            Type = "SWITCH",
            EvaluatorType = "decision",
        };
        decision.WithInput("model", "jev-1.13");
        var json = JObject.Parse(decision.ToJson());
        Assert.Equal("SWITCH", (string?)json["type"]);
        Assert.Equal("decision", (string?)json["evaluatorType"]);
        Assert.Equal("route", (string?)json["expression"]);
        Assert.Null(json["inputParameters"]!["switchCaseValue"]);
        var legacy = new SwitchTask("legacy", "${workflow.input.route}");
        Assert.Equal("switchCaseValue", legacy.Expression);
        Assert.Equal("${workflow.input.route}", legacy.InputParameters["switchCaseValue"]);
    }
}
