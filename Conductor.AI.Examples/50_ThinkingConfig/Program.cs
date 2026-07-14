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
// Thinking Config — enable extended reasoning for complex tasks.
//
// When ThinkingBudgetTokens is set, the agent uses extended thinking
// mode, allowing the LLM to reason step-by-step before responding.
// This improves performance on complex analytical tasks.
//
// Requirements:
//   - Agentspan server with thinking config support
//   - A model that supports extended thinking (e.g., Claude with thinking)
//   - AGENTSPAN_SERVER_URL=http://localhost:8080/api in environment
//   - AGENTSPAN_LLM_MODEL set in environment

using Conductor.AI;
using Conductor.AI.Examples;

// ── Agent with extended thinking ──────────────────────────────

var agent = new Agent("deep_thinker_50")
{
    Model = Settings.LlmModel,
    Instructions =
        "You are an analytical assistant. Think carefully through complex " +
        "problems step by step. Use the calculate tool for math.",
    Tools = ToolRegistry.FromInstance(new CalculatorTools()),
    ThinkingBudgetTokens = 2048,
};

// ── Run ───────────────────────────────────────────────────────

await using var runtime = new AgentRuntime();
var result = await runtime.RunAsync(
    agent,
    "If a train travels 120 km in 2 hours, then speeds up by 50% for " +
    "the next 3 hours, what is the total distance traveled?");

result.PrintResult();

// ── Tool class ────────────────────────────────────────────────

internal sealed class CalculatorTools
{
    [Tool("Evaluate a mathematical expression.")]
    public Dictionary<string, object> Calculate(string expression)
    {
        // Safe evaluation of simple arithmetic expressions
        try
        {
            var result = EvalSimple(expression);
            return new() { ["expression"] = expression, ["result"] = result };
        }
        catch (Exception ex)
        {
            return new() { ["expression"] = expression, ["error"] = ex.Message };
        }
    }

    // Simple eval for +, -, *, / operations (no exec of arbitrary code)
    private static double EvalSimple(string expr)
    {
        var dt = new System.Data.DataTable();
        var result = dt.Compute(expr, "");
        return Convert.ToDouble(result);
    }
}
