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
// Callbacks — lifecycle hooks before and after LLM calls.
//
// Demonstrates using BeforeModelCallback and AfterModelCallback to
// intercept and inspect LLM interactions. Callbacks are registered as
// Conductor worker tasks and execute server-side.
//
// Requirements:
//   - Agentspan server with LLM support
//   - AGENTSPAN_SERVER_URL=http://localhost:8080/api in environment
//   - AGENTSPAN_LLM_MODEL set in environment

using System.Text.Json;
using Conductor.AI;
using Conductor.AI.Examples;

// ── Agent with callbacks ───────────────────────────────────────

var agent = new Agent("monitored_agent_47")
{
    Model = Settings.LlmModel,
    Instructions = "You are a helpful assistant. Use get_facts when asked about topics.",
    Tools = ToolRegistry.FromInstance(new FactTools()),

    BeforeModelCallback = messages =>
    {
        int count = messages?.Count ?? 0;
        Console.WriteLine($"  [before_model] Sending {count} messages to LLM");
        return [];  // empty = continue to LLM normally
    },

    AfterModelCallback = llmResult =>
    {
        int length = llmResult?.Length ?? 0;
        Console.WriteLine($"  [after_model] LLM returned {length} characters");
        return [];  // empty = keep original response
    },
};

// ── Run ───────────────────────────────────────────────────────

await using var runtime = new AgentRuntime();
var result = await runtime.RunAsync(agent, "Tell me interesting facts about AI and space.");
result.PrintResult();

// ── Tool class ────────────────────────────────────────────────

internal sealed class FactTools
{
    private static readonly Dictionary<string, List<string>> Facts = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ai"] = ["AI was coined in 1956", "GPT-4 has ~1.7T parameters"],
        ["space"] = ["The ISS orbits at 17,500 mph", "Mars has the tallest volcano"],
    };

    [Tool("Get interesting facts about a topic.")]
    public Dictionary<string, object> GetFacts(string topic)
    {
        foreach (var (key, vals) in Facts)
            if (topic.Contains(key, StringComparison.OrdinalIgnoreCase))
                return new() { ["topic"] = topic, ["facts"] = vals };
        return new() { ["topic"] = topic, ["facts"] = new List<string> { "No specific facts found." } };
    }
}
