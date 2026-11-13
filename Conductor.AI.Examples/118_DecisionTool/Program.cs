// A normal chat agent calls a branchless decision-backed SWITCH tool.
// Set CONDUCTOR_SERVER_URL and optionally CONDUCTOR_DECISION_PROVIDER.
using System.Text.Json.Nodes;
using Conductor.AI;
using Conductor.AI.Examples;

var decisionConfig = new Dictionary<string, object>
{
    ["model"] = "jev-1.13",
    ["questions"] = new
    {
        department = new
        {
            type = "choice",
            instructions = "Choose the correct support department.",
            choices = new
            {
                billing = "Payments and invoices",
                technical = "Product errors and troubleshooting",
            },
        },
    },
};
var provider = Environment.GetEnvironmentVariable("CONDUCTOR_DECISION_PROVIDER");
if (provider is not null) decisionConfig["provider"] = provider;
var classifyRequest = new ToolDef
{
    Name = "classify_request",
    Description = "Classify the customer request before answering.",
    ToolType = "decision",
    InputSchema = JsonNode.Parse("""
        {"type":"object","properties":{"state":{"type":"string"}},"required":["state"]}
        """)!.AsObject(),
    Config = decisionConfig,
};
var agent = new Agent("decision_tool_support_csharp")
{
    Model = Settings.LlmModel,
    Instructions = "Call classify_request before answering the customer.",
    Tools = [classifyRequest],
};
await using var runtime = new AgentRuntime();
using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(120));
var result = await runtime.RunAsync(agent,
    "I was charged twice on my latest invoice.", ct: timeout.Token);
result.PrintResult();
if (!result.IsSuccess) throw new InvalidOperationException($"{result.Status}: {result.Error}");
