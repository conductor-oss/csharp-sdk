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
using Conductor.Api;
using Conductor.Client.Models;
using Conductor.Definition;
using Conductor.Definition.TaskType;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using Tests.Integration.Helpers;
using Xunit;

namespace Tests.Integration.Workflow
{
    /// <summary>
    /// Runs only against a server whose Decision provider is scripts/decision-stub.py
    /// (scripts/docker-compose-oss.yaml on Conductor OSS 3.33.0+), signalled by
    /// CONDUCTOR_DECISION_STUB=true. The stub picks the choice whose words appear in
    /// the request, so each request has one known branch.
    /// </summary>
    public sealed class DecisionStubTheoryAttribute : TheoryAttribute
    {
        public DecisionStubTheoryAttribute()
        {
            if (!string.Equals(System.Environment.GetEnvironmentVariable("CONDUCTOR_DECISION_STUB"), "true", StringComparison.OrdinalIgnoreCase))
                Skip = "Set CONDUCTOR_DECISION_STUB=true with the decision stub server (scripts/docker-compose-oss.yaml).";
        }
    }

    [Collection("Integration")]
    [Trait("Category", "Integration")]
    public class DecisionSwitchTests : IClassFixture<ConductorFixture>
    {
        private readonly WorkflowResourceApi _workflowClient;
        private readonly MetadataResourceApi _metadataClient;
        private readonly string _workflowName = TestPrefix.Name("decision_switch_wf");

        public DecisionSwitchTests(ConductorFixture fixture)
        {
            _workflowClient = fixture.Configuration.GetClient<WorkflowResourceApi>();
            _metadataClient = fixture.Configuration.GetClient<MetadataResourceApi>();
        }

        [DecisionStubTheory]
        [InlineData("I was charged twice on my latest invoice.", "billing")]
        [InlineData("The product crashes on startup with error 500. Help troubleshoot it.", "technical")]
        public void DecisionSwitch_RunsTheSelectedBranch(string request, string expectedCase)
        {
            var workflow = new ConductorWorkflow { OutputParameters = new Dictionary<string, object>() }
                .WithName(_workflowName).WithVersion(1).WithOwner("sdk-test@conductor.io")
                .WithInputParameter("request");
            var decision = SwitchTask.Decision("route_request", "route");
            decision.WithInput("model", "jev-1.13")
                .WithInput("state", workflow.Input("request"))
                .WithInput("questions", new Dictionary<string, object>
                {
                    ["route"] = new Dictionary<string, object>
                    {
                        ["type"] = "choice",
                        ["instructions"] = "Choose the team best suited to handle this request.",
                        ["choices"] = new Dictionary<string, object>
                        {
                            ["billing"] = "Payments, invoices, refunds, or subscriptions.",
                            ["technical"] = "Errors, outages, or product troubleshooting.",
                        },
                    },
                });
            var billing = new JavascriptTask("handle_billing", "({team: 'billing'})");
            var technical = new JavascriptTask("handle_technical", "({team: 'technical'})");
            decision.WithDecisionCase("billing", billing).WithDecisionCase("technical", technical);
            workflow.WithTask(decision)
                .WithOutputParameter("selectedCase", decision.Output("selectedCase"))
                .WithOutputParameter("answers", decision.Output("answers"))
                .WithOutputParameter("billing", billing.Output("result"))
                .WithOutputParameter("technical", technical.Output("result"));
            _metadataClient.UpdateWorkflowDefinitions(new List<WorkflowDef> { workflow }, true);

            var id = _workflowClient.StartWorkflow(new StartWorkflowRequest(
                name: _workflowName, version: 1,
                input: new Dictionary<string, object> { ["request"] = request }));
            try
            {
                var run = WaitForTerminal(id);
                Assert.True(run.Status == Conductor.Client.Models.Workflow.StatusEnum.COMPLETED,
                    $"{run.Status}: {run.ReasonForIncompletion}");
                var output = JObject.FromObject(run.Output);
                Assert.Equal(expectedCase, (string)output["selectedCase"]);
                Assert.Equal(expectedCase, (string)output["answers"]["route"]["choice"]);
                Assert.Equal(expectedCase, (string)output[expectedCase]["team"]);
                var otherCase = expectedCase == "billing" ? "technical" : "billing";
                Assert.True(output[otherCase] == null || output[otherCase].Type == JTokenType.Null,
                    $"branch {otherCase} should not run");
            }
            finally
            {
                try { _workflowClient.Delete(id, false); } catch { }
            }
        }

        private Conductor.Client.Models.Workflow WaitForTerminal(string id)
        {
            var deadline = DateTime.UtcNow.AddSeconds(30);
            while (true)
            {
                var run = _workflowClient.GetWorkflow(id, false);
                if (run.Status != Conductor.Client.Models.Workflow.StatusEnum.RUNNING || DateTime.UtcNow > deadline)
                    return run;
                Thread.Sleep(500);
            }
        }
    }
}
