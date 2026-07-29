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
// Prompt Templates — use server-side templates for agent instructions.
//
// Instead of embedding instructions inline, agents can reference a
// named template stored on the Conductor server. Variables substitute
// ${var} placeholders at execution time, letting you update wording
// centrally without redeploying code.
//
// Requires a template named "order-support" on the server.
// Create it in the Conductor UI (Definitions → Prompt Templates) with body:
//
//   You are an order support specialist.
//   Maximum refund authority: ${max_refund}.
//   For escalations, contact: ${escalation_email}.
//
// If the template is absent the agent still runs with server defaults.
//
// Requirements:
//   - Conductor server with the "order-support" prompt template defined
//   - CONDUCTOR_SERVER_URL=http://localhost:8080/api in environment
//   - CONDUCTOR_AGENT_LLM_MODEL set in environment

using Conductor.AI;
using Conductor.AI.Examples;

// ── Agent with prompt template ────────────────────────────────

var tools = ToolRegistry.FromInstance(new OrderTools());

var orderAgent = new Agent("order_assistant_34")
{
    Model = Settings.LlmModel,
    PromptTemplateInstructions = new PromptTemplate(
        Name: "order-support",
        Variables: new()
        {
            ["max_refund"] = "$500",
            ["escalation_email"] = "help@acme.com",
        }
    ),
    Tools = [.. tools],
};

// ── Run ───────────────────────────────────────────────────────

await using var runtime = new AgentRuntime();

var result = await runtime.RunAsync(orderAgent, "Can you check order #12345?");
result.PrintResult();

// ── Tool class ────────────────────────────────────────────────

internal sealed class OrderTools
{
    [Tool("Look up an order by ID.")]
    public object LookupOrder(string orderId) =>
        new { order_id = orderId, status = "shipped", eta = "2 days" };

    [Tool("Look up customer details by email.")]
    public object LookupCustomer(string email) =>
        new { email, name = "Jane Doe", tier = "premium" };
}
