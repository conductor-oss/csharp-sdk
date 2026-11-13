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

    [Fact]
    public void DecisionRouterKeepsChoiceKeysFromAnonymousObjects()
    {
        var agent = new Agent("support")
        {
            Strategy = Strategy.Router,
            DecisionRouter = new ToolDef
            {
                Name = "selector",
                ToolType = "decision",
                Config = new Dictionary<string, object>
                {
                    ["model"] = "jev-1.13",
                    ["questions"] = new { Agent = new { type = "choice", choices = new { BillingAgent = "Invoices" } } },
                },
            },
            Agents = [new Agent("BillingAgent") { Model = "openai/gpt-4o-mini" }],
        };
        var router = AgentConfigSerializer.SerializeAgent(agent)["router"]!;
        // Choice keys must equal agent names exactly; the server matches them verbatim.
        Assert.NotNull(router["questions"]!["Agent"]!["choices"]!["BillingAgent"]);
    }

    [Theory]
    [InlineData("provider-not-string", 42)]
    [InlineData("provider-blank", " ")]
    public void DecisionRouterOmitsNonStringOrBlankProvider(string _, object provider)
    {
        var config = new Dictionary<string, object>(Selector().Config!) { ["provider"] = provider };
        var agent = new Agent("support") { DecisionRouter = new ToolDef { Name = "s", ToolType = "decision", Config = config } };
        Assert.False(AgentConfigSerializer.SerializeAgent(agent)["router"]!.AsObject().ContainsKey("provider"));
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
    public void DecisionSwitchSendsExpressionVerbatimWithoutSwitchCaseValue()
    {
        var decision = SwitchTask.Decision("route_request", "route");
        decision.WithDecisionCase("billing").WithDecisionCase("technical");
        decision.WithInput("model", "jev-1.13")
            .WithInput("questions", new { route = new { type = "choice", choices = new { billing = "B", technical = "T" } } });
        var json = JObject.Parse(decision.ToJson());
        Assert.Equal("SWITCH", (string?)json["workflowTaskType"]);
        Assert.Equal("decision", (string?)json["evaluatorType"]);
        Assert.Equal("route", (string?)json["expression"]);
        Assert.Null(json["inputParameters"]!["switchCaseValue"]);
        Assert.Equal(
            ((JObject)json["inputParameters"]!["questions"]!["route"]!["choices"]!).Properties().Select(p => p.Name),
            ((JObject)json["decisionCases"]!).Properties().Select(p => p.Name));
    }

    [Theory]
    [InlineData(false, "${workflow.input.route}", "value-param", "switchCaseValue")]
    [InlineData(true, "return $.route;", "javascript", "return $.route;")]
    public void ExistingSwitchConstructorsAreUnchanged(bool useJavascript, string expression, string evaluator, string sentExpression)
    {
        var task = new SwitchTask("route", expression, useJavascript);
        Assert.Equal(evaluator, task.EvaluatorType);
        Assert.Equal(sentExpression, task.Expression);
        Assert.Equal(useJavascript ? null : expression, task.InputParameters.GetValueOrDefault("switchCaseValue"));
    }
}
