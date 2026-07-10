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
// Adk06 — Streaming.
//
// A documentation lookup ADK agent. Demonstrates StreamAsync() — yields
// events as the agent thinks, calls tools, and completes.
//
// Requirements:
//   - AGENTSPAN_SERVER_URL=http://localhost:6767/api
//   - AGENTSPAN_LLM_MODEL=openai/gpt-4o-mini

using Conductor.AI;
using Conductor.AI.Examples;
using Conductor.AI.GoogleADK;

var agent = GoogleADKAgent.Builder()
    .Name("docs_assistant")
    .Model(Settings.LlmModel)
    .Instruction(
        "You are a documentation assistant. Use the search tool to find " +
        "relevant docs and provide clear, well-formatted answers.")
    .Tools(new DocsTools())
    .Build();

await using var runtime = new AgentRuntime(new AgentRuntimeOptions { ServerUrl = Settings.ServerUrl });

Console.WriteLine("Streaming agent execution:");
Console.WriteLine(new string('-', 40));

await foreach (var ev in runtime.StreamAsync(agent, "How do I authenticate with the API?"))
{
    switch (ev.Type)
    {
        case EventType.Thinking:
            Console.WriteLine($"  [thinking] {ev.Content}");
            break;
        case EventType.ToolCall:
            Console.WriteLine($"  [tool_call] {ev.ToolName}({ev.Args})");
            break;
        case EventType.ToolResult:
            Console.WriteLine($"  [tool_result] {ev.ToolName} -> {ev.Result}");
            break;
        case EventType.Done:
            Console.WriteLine();
            Console.WriteLine($"Result: {ev.Content}");
            break;
        case EventType.Error:
            Console.WriteLine($"  [error] {ev.Content}");
            break;
    }
}

internal sealed class DocsTools
{
    private static readonly Dictionary<string, Dictionary<string, object>> _docs = new()
    {
        ["installation"] = new()
        {
            ["title"] = "Installation Guide",
            ["content"] = "Run `pip install mypackage`. Requires Python 3.9+.",
        },
        ["authentication"] = new()
        {
            ["title"] = "Authentication",
            ["content"] = "Use API keys via the X-API-Key header. Keys are managed in the dashboard.",
        },
        ["rate limits"] = new()
        {
            ["title"] = "Rate Limiting",
            ["content"] = "Free tier: 100 req/min. Pro: 1000 req/min. Enterprise: unlimited.",
        },
    };

    [Tool(Name = "search_documentation", Description = "Search the product documentation.")]
    public Dictionary<string, object> SearchDocumentation(string query)
    {
        var lower = query.ToLowerInvariant();
        foreach (var (k, v) in _docs)
        {
            if (lower.Contains(k))
            {
                var r = new Dictionary<string, object> { ["found"] = true };
                foreach (var (kk, vv) in v) r[kk] = vv;
                return r;
            }
        }
        return new Dictionary<string, object> { ["found"] = false, ["message"] = "No matching documentation found." };
    }
}
