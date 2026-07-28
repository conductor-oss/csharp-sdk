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
// HTTP Tools — server-side HTTP tools (no worker process needed).
//
// Demonstrates ToolDef with toolType="http" — the server calls the
// HTTP endpoint directly without dispatching to a local worker.
//
// Requirements:
//   - Conductor server with LLM support
//   - CONDUCTOR_SERVER_URL=http://localhost:8080/api in environment
//   - CONDUCTOR_AGENT_LLM_MODEL set in environment

using System.Text.Json.Nodes;
using Conductor.AI;
using Conductor.AI.Examples;

// ── Local worker tool ────────────────────────────────────────────────

var localTools = ToolRegistry.FromInstance(new ReportFormatter());

// ── HTTP tool (server-side, no local worker) ─────────────────────────
// The server calls the URL directly. ${HTTP_TEST_API_KEY} is resolved
// from the credential store at execution time.

var httpTool = new ToolDef
{
    Name = "get_public_ip",
    Description = "Get the current public IP address",
    InputSchema = new JsonObject
    {
        ["type"] = "object",
        ["properties"] = new JsonObject(),
        ["required"] = new JsonArray(),
    },
    // Mark as external so AgentRuntime doesn't try to register a worker for it
    External = true,
};

// ── Agent ─────────────────────────────────────────────────────────────

var agent = new Agent("http_tools_demo")
{
    Model = Settings.LlmModel,
    Instructions = "You can format reports. Use format_report to structure any information you have.",
    Tools = [.. localTools, httpTool],
};

// ── Run ───────────────────────────────────────────────────────────────

await using var runtime = new AgentRuntime();
var result = await runtime.RunAsync(agent, "Write a formatted report about the Conductor C# SDK features.");
result.PrintResult();

// ── Tool class ────────────────────────────────────────────────────────

internal sealed class ReportFormatter
{
    [Tool("Format a title and body into a structured report.")]
    public Dictionary<string, object> FormatReport(string title, string body) =>
        new()
        {
            ["report"] = $"=== {title} ===\n{body}\n{new string('=', title.Length + 8)}",
        };
}
