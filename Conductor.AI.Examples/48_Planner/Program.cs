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
// Planner — agent that plans before executing.
//
// When Planner = true, the server enhances the system prompt with planning
// instructions so the agent creates a step-by-step plan before executing
// tools. This improves performance on complex, multi-step tasks.
//
// Requirements:
//   - Conductor server with planner support
//   - CONDUCTOR_SERVER_URL=http://localhost:8080/api in environment
//   - CONDUCTOR_AGENT_LLM_MODEL set in environment

using Conductor.AI;
using Conductor.AI.Examples;

var agent = new Agent("research_writer_48")
{
    Model = Settings.LlmModel,
    Instructions =
        "You are a research writer. Research topics thoroughly and " +
        "write structured reports with multiple sections.",
    Tools = ToolRegistry.FromInstance(new ResearchTools()),
    EnablePlanning = true,
};

await using var runtime = new AgentRuntime();
var result = await runtime.RunAsync(
    agent,
    "Write a brief report on renewable energy and climate change solutions.");

result.PrintResult();

// ── Tools ─────────────────────────────────────────────────────────────

internal sealed class ResearchTools
{
    private static readonly Dictionary<string, string[]> KnowledgeBase = new()
    {
        ["climate change"] = ["Solar energy costs dropped 89% since 2010", "Wind power is cheapest in many regions"],
        ["renewable energy"] = ["Renewables = 30% of global electricity (2023)", "Solar capacity grew 50% year-over-year"],
    };

    [Tool("Search the web for information on a topic.")]
    public Dictionary<string, object> SearchWeb(string query)
    {
        foreach (var (key, results) in KnowledgeBase)
        {
            if (key.Split(' ').Any(word => query.Contains(word, StringComparison.OrdinalIgnoreCase)))
                return new() { ["query"] = query, ["results"] = results };
        }
        return new() { ["query"] = query, ["results"] = new[] { "No specific results." } };
    }

    [Tool("Write a section of a report with a title and content.")]
    public Dictionary<string, object> WriteSection(string title, string content)
        => new() { ["section"] = $"## {title}\n\n{content}" };
}
