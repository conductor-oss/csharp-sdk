// One synchronous decision-backed SWITCH with INLINE branches. No local worker.
// Set CONDUCTOR_SERVER_URL and optionally CONDUCTOR_DECISION_PROVIDER.
// Pass a request as the first argument to exercise the technical branch.
using Conductor.AI.Examples;
using Conductor.Api;
using Conductor.Client;
using Conductor.Client.Models;
using Conductor.Definition;
using Conductor.Definition.TaskType;
using Conductor.Executor;
using Newtonsoft.Json;

var config = new Configuration { BasePath = Settings.ServerUrl };
var workflow = new ConductorWorkflow { OutputParameters = new Dictionary<string, object>() }
    .WithName("ai_decision_routing_csharp").WithVersion(1)
    .WithOwner("examples@conductor-oss.org").WithInputParameter("request");
// useJavascript=true preserves the expression; the evaluator is replaced below.
// The legacy value-param constructor otherwise creates a switchCaseValue binding.
var decision = new SwitchTask("route_request", "route", useJavascript: true)
{
    Type = "SWITCH",
    EvaluatorType = "decision",
};
decision.WithInput("model", "jev-1.13")
    .WithInput("state", workflow.Input("request"))
    .WithInput("questions", new Dictionary<string, object>
    {
        ["route"] = new
        {
            type = "choice",
            instructions = "Choose the team best suited to handle this request.",
            choices = new
            {
                billing = "Payments, invoices, refunds, or subscriptions.",
                technical = "Errors, outages, or product troubleshooting.",
            },
        },
    });
var provider = Environment.GetEnvironmentVariable("CONDUCTOR_DECISION_PROVIDER");
if (provider is not null) decision.WithInput("provider", provider);
var billing = new JavascriptTask("handle_billing",
    "({team: 'billing', nextAction: 'review_invoice', " +
    "message: 'Check invoice line items and payment records.', request: $.request})") { Type = "INLINE" };
billing.WithInput("request", workflow.Input("request"));
var technical = new JavascriptTask("handle_technical",
    "({team: 'technical', nextAction: 'collect_diagnostics', " +
    "message: 'Collect error logs and steps to reproduce.', request: $.request})") { Type = "INLINE" };
technical.WithInput("request", workflow.Input("request"));
decision.WithDecisionCase("billing", billing).WithDecisionCase("technical", technical);
var result = new JavascriptTask("selected_result", "$.billing || $.technical") { Type = "INLINE" };
result.WithInput("billing", billing.Output("result"))
    .WithInput("technical", technical.Output("result"));
workflow.WithTask(decision, result)
    .WithOutputParameter("selectedCase", decision.Output("selectedCase"))
    .WithOutputParameter("answers", decision.Output("answers"))
    .WithOutputParameter("usage", decision.Output("usage"))
    .WithOutputParameter("cost", decision.Output("usage.cost"))
    .WithOutputParameter("latencyMs", decision.Output("latencyMs"))
    .WithOutputParameter("result", result.Output("result"));

var executor = new WorkflowExecutor(config);
executor.RegisterWorkflow(workflow, overwrite: true);
var start = workflow.GetStartWorkflowRequest();
start.Input = new Dictionary<string, object>
{
    ["request"] = args.FirstOrDefault() ?? "I was charged twice on my latest invoice.",
};
var id = executor.StartWorkflow(start);
Console.WriteLine($"Workflow: {id}");
var client = config.GetClient<WorkflowResourceApi>();
using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(120));
while (true)
{
    var run = client.GetWorkflow(id, includeTasks: false);
    if (run.Status == Workflow.StatusEnum.COMPLETED)
    {
        Console.WriteLine(JsonConvert.SerializeObject(run.Output, Formatting.Indented));
        break;
    }
    if (run.Status != Workflow.StatusEnum.RUNNING)
        throw new InvalidOperationException($"{run.Status}: {run.ReasonForIncompletion}");
    await System.Threading.Tasks.Task.Delay(1000, timeout.Token);
}
