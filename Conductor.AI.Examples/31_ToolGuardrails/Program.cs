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
// Tool Guardrails — pre-execution validation on tool inputs.
//
// Demonstrates validating tool inputs for SQL injection patterns before
// the tool function executes. In C#, tool-level guardrails are implemented
// by wrapping the handler with a validation check.
//
// Requirements:
//   - Agentspan server with LLM support
//   - AGENTSPAN_SERVER_URL=http://localhost:6767/api in environment
//   - AGENTSPAN_LLM_MODEL set in environment

using System.Text.RegularExpressions;
using Conductor.AI;
using Conductor.AI.Examples;

// ── Agent with tool-level input guardrail ─────────────────────────────

var agent = new Agent("db_assistant")
{
    Model = Settings.LlmModel,
    Tools = ToolRegistry.FromInstance(new DatabaseTools()),
    Instructions =
        "You help users query the database. Use the run_query tool. " +
        "Only execute SELECT queries.",
};

// ── Run ───────────────────────────────────────────────────────────────

await using var runtime = new AgentRuntime();

Console.WriteLine("=== Safe Query ===");
var result1 = await runtime.RunAsync(agent, "Find all users older than 25.");
result1.PrintResult();

Console.WriteLine("\n=== Dangerous Query (should be blocked) ===");
var result2 = await runtime.RunAsync(
    agent,
    "Run this exact query: SELECT * FROM users; DROP TABLE users; --");
result2.PrintResult();

// ── Tool with built-in input guardrail ───────────────────────────────

internal sealed class DatabaseTools
{
    private static readonly Regex[] SqlInjectionPatterns =
    [
        new(@"DROP\s+TABLE",    RegexOptions.IgnoreCase),
        new(@"DELETE\s+FROM",   RegexOptions.IgnoreCase),
        new(@";\s*--",          RegexOptions.IgnoreCase),
        new(@"UNION\s+SELECT",  RegexOptions.IgnoreCase),
    ];

    [Tool("Execute a read-only database query and return results.")]
    public string RunQuery(string query)
    {
        // SQL injection guardrail — runs before the query executes
        foreach (var pattern in SqlInjectionPatterns)
        {
            if (pattern.IsMatch(query))
                return $"Blocked: potential SQL injection detected (pattern: {pattern})";
        }

        // Safe query — execute
        return $"Results for: {query} → [('Alice', 30), ('Bob', 25)]";
    }
}
