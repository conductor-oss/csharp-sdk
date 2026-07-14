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
// Human Tool — LLM-initiated human interaction.
//
// Unlike approval_required tools (09_HumanInTheLoop) where humans gate
// tool execution, HumanTool.Create() lets the LLM ask the human questions
// at any point. The LLM decides when to call the tool, and the human's
// response is returned as the tool output.
//
// The tool is entirely server-side (Conductor HUMAN task) — no worker process
// needed. The server generates the response form and validation pipeline
// automatically, so this works with any SDK language.
//
// Demonstrates:
//   - HumanTool.Create() for LLM-initiated human interaction
//   - Mixing human tools with regular tools
//   - The LLM using human input to make decisions
//
// Requirements:
//   - AGENTSPAN_SERVER_URL=http://localhost:8080/api in environment
//   - AGENTSPAN_LLM_MODEL set in environment

using System.Text.Json;
using Conductor.AI;
using Conductor.AI.Examples;

// Server-side human question tool (no worker needed)
var askUser = HumanTool.Create(
    name: "ask_user",
    description: "Ask the user a question when you need clarification or additional information.");

var agent = new Agent("it_support_09d")
{
    Model = Settings.LlmModel,
    Tools = [askUser, .. ToolRegistry.FromInstance(new ItSupportTools())],
    Instructions =
        "You are an IT support assistant. Help users create support tickets. " +
        "Use lookup_employee to find employee info. " +
        "If you need clarification about the issue or any details, use ask_user " +
        "to ask the user directly. Always confirm the ticket details with the user " +
        "before submitting.",
};

await using var runtime = new AgentRuntime();

var handle = await runtime.StartAsync(agent, "I need to file a ticket for Alice about a laptop issue");
Console.WriteLine($"Started: {handle.ExecutionId}\n");

await foreach (var evt in handle.StreamAsync())
{
    switch (evt.Type)
    {
        case EventType.Thinking:
            Console.WriteLine($"  [thinking] {evt.Content}");
            break;

        case EventType.ToolCall:
            Console.WriteLine($"  [tool_call] {evt.ToolName}");
            break;

        case EventType.ToolResult:
            Console.WriteLine($"  [tool_result] {evt.ToolName}");
            break;

        case EventType.Waiting:
            // Agent is waiting for human input via the human_tool
            var status = await handle.GetStatusAsync();
            var pt = status.PendingTool ?? new();
            if (pt.TryGetValue("args", out var argsObj))
            {
                var toolArgs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                    JsonSerializer.Serialize(argsObj));
                var question = toolArgs?.TryGetValue("question", out var q) == true ? q.GetString() : "Input required:";
                Console.Write($"\n  [human] {question} ");
                var answer = Console.ReadLine() ?? "";
                await handle.RespondAsync(new { answer });
            }
            break;

        case EventType.Done:
            Console.WriteLine($"\nDone: {evt.Content}");
            break;
    }
}

// ── Tool class ─────────────────────────────────────────────────

internal sealed class ItSupportTools
{
    private static readonly Dictionary<string, Dictionary<string, object>> Employees = new()
    {
        ["alice"] = new() { ["name"] = "Alice Chen", ["department"] = "Engineering", ["level"] = "Senior" },
        ["bob"] = new() { ["name"] = "Bob Martinez", ["department"] = "Sales", ["level"] = "Manager" },
        ["carol"] = new() { ["name"] = "Carol Wu", ["department"] = "Engineering", ["level"] = "Staff" },
    };

    [Tool("Look up an employee by name and return their info.")]
    public Dictionary<string, object> LookupEmployee(string name)
    {
        var key = name.ToLowerInvariant().Split(' ')[0];
        return Employees.TryGetValue(key, out var e) ? e
            : new() { ["error"] = $"Employee '{name}' not found" };
    }

    [Tool("Submit an IT support ticket.")]
    public Dictionary<string, object> SubmitTicket(string title, string priority, string assignee)
        => new() { ["ticket_id"] = "TKT-4821", ["title"] = title, ["priority"] = priority, ["assignee"] = assignee };
}
