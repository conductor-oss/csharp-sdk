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
// Handoffs — agent delegating to sub-agents.
//
// Demonstrates the HANDOFF strategy where the parent agent's LLM decides
// which sub-agent to delegate to. Sub-agents appear as callable tools.
//
// Architecture:
//   support (HANDOFF)
//   ├── billing   — balance, payments
//   ├── technical — orders, shipping
//   └── sales     — pricing, promotions
//
// Requirements:
//   - Conductor server with LLM support
//   - CONDUCTOR_SERVER_URL=http://localhost:8080/api in environment
//   - CONDUCTOR_AGENT_LLM_MODEL set in environment (optional)

using Conductor.AI;
using Conductor.AI.Examples;

// ── Sub-agent tool hosts ────────────────────────────────────────────

var billingTools = ToolRegistry.FromInstance(new BillingTools());
var technicalTools = ToolRegistry.FromInstance(new TechnicalTools());
var salesTools = ToolRegistry.FromInstance(new SalesTools());

// ── Specialist agents ───────────────────────────────────────────────

var billingAgent = new Agent("billing")
{
    Model = Settings.LlmModel,
    Instructions = "You handle billing questions: balances, payments, invoices.",
    Tools = billingTools,
};

var technicalAgent = new Agent("technical")
{
    Model = Settings.LlmModel,
    Instructions = "You handle technical questions: order status, shipping, returns.",
    Tools = technicalTools,
};

var salesAgent = new Agent("sales")
{
    Model = Settings.LlmModel,
    Instructions = "You handle sales questions: pricing, products, promotions.",
    Tools = salesTools,
};

// ── Orchestrator with handoffs ──────────────────────────────────────

var support = new Agent("support")
{
    Model = Settings.LlmModel,
    Instructions = "Route customer requests to the right specialist: billing, technical, or sales.",
    Agents = [billingAgent, technicalAgent, salesAgent],
    Strategy = Strategy.Handoff,
};

// ── Run ─────────────────────────────────────────────────────────────

await using var runtime = new AgentRuntime();
var result = await runtime.RunAsync(support, "What's the balance on account ACC-123?");
result.PrintResult();

// ── Tool hosts ──────────────────────────────────────────────────────

internal sealed class BillingTools
{
    [Tool("Check the balance of a bank account.")]
    public Dictionary<string, object> CheckBalance(string accountId)
        => new() { ["account_id"] = accountId, ["balance"] = 5432.10, ["currency"] = "USD" };
}

internal sealed class TechnicalTools
{
    [Tool("Look up the status of an order.")]
    public Dictionary<string, object> LookupOrder(string orderId)
        => new() { ["order_id"] = orderId, ["status"] = "shipped", ["eta"] = "2 days" };
}

internal sealed class SalesTools
{
    [Tool("Get pricing information for a product.")]
    public Dictionary<string, object> GetPricing(string product)
        => new() { ["product"] = product, ["price"] = 99.99, ["discount"] = "10% off" };
}
