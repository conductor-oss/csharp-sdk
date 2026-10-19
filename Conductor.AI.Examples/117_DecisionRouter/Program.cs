// Decision ToolDef selects one chat agent on the server. No local worker.
// Set CONDUCTOR_SERVER_URL and optionally CONDUCTOR_DECISION_PROVIDER.
// Prints a plan by default; pass --run for inference.
using Conductor.AI;
using Conductor.AI.Examples;

var billing = new Agent("billing_csharp")
{
    Model = Settings.LlmModel,
    Instructions = "Help the customer with billing and invoice questions.",
};
var technical = new Agent("technical_csharp")
{
    Model = Settings.LlmModel,
    Instructions = "Help the customer troubleshoot product issues.",
};
var decisionConfig = new Dictionary<string, object>
{
    ["model"] = "jev-1.13",
    ["questions"] = new
    {
        agent = new
        {
            type = "choice",
            instructions = "Choose the right support team.",
            choices = new
            {
                billing_csharp = "Payments and invoices",
                technical_csharp = "Product errors and troubleshooting",
            },
        },
    },
};
var provider = Environment.GetEnvironmentVariable("CONDUCTOR_DECISION_PROVIDER");
if (provider is not null) decisionConfig["provider"] = provider;
var selector = new ToolDef
{
    Name = "support_selector_csharp",
    Description = "Choose the support agent that should handle the request.",
    ToolType = "decision",
    Config = decisionConfig,
};
var agent = new Agent("decision_support_router_csharp")
{
    Strategy = Strategy.Router,
    DecisionRouter = selector,
    Agents = [billing, technical],
    MaxTurns = 1,
    Synthesize = false,
};
await using var runtime = new AgentRuntime();
if (!args.Contains("--run"))
{
    Console.WriteLine((await runtime.PlanAsync(agent))?.ToJsonString());
    return;
}
await runtime.DeployAsync(agent);
using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(120));
var result = await runtime.RunAsync(agent,
    "The customer reports a duplicate charge on the latest invoice.", ct: timeout.Token);
result.PrintResult();
if (!result.IsSuccess) throw new InvalidOperationException($"{result.Status}: {result.Error}");
