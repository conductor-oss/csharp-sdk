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
// Shared State — tools sharing state across calls via ToolContext.
//
// Tools read and write to context.State, a dictionary that persists
// across all tool calls within the same agent execution. This lets
// tools accumulate data without relying on the LLM to relay state.
//
// Requirements:
//   - Conductor server with LLM support
//   - CONDUCTOR_SERVER_URL=http://localhost:8080/api in environment
//   - CONDUCTOR_AGENT_LLM_MODEL set in environment

using System.Text.Json;
using Conductor.AI;
using Conductor.AI.Examples;

var tools = ToolRegistry.FromInstance(new ShoppingListTools());

var agent = new Agent("shopping_assistant")
{
    Model = Settings.LlmModel,
    Instructions =
        "You help manage a shopping list. Use add_item to add items, " +
        "get_list to view the list, and clear_list to reset it. " +
        "IMPORTANT: Always add all items first, then call get_list separately " +
        "to verify the list contents.",
    Tools = tools,
};

await using var runtime = new AgentRuntime();
var result = await runtime.RunAsync(
    agent,
    "Add milk, eggs, and bread to my shopping list, then show me the list.");

result.PrintResult();

// ── Tool class ────────────────────────────────────────────────────────

internal sealed class ShoppingListTools
{
    [Tool("Add an item to the shared shopping list.")]
    public Dictionary<string, object> AddItem(string item, ToolContext? ctx = null)
    {
        var items = GetItems(ctx);
        items.Add(item);
        SetItems(ctx, items);
        return new() { ["added"] = item, ["total_items"] = items.Count };
    }

    [Tool("Get the current shopping list from shared state.")]
    public Dictionary<string, object> GetList(ToolContext? ctx = null)
    {
        var items = GetItems(ctx);
        return new() { ["items"] = items, ["total_items"] = items.Count };
    }

    [Tool("Clear the shopping list.")]
    public Dictionary<string, object> ClearList(ToolContext? ctx = null)
    {
        SetItems(ctx, []);
        return new() { ["status"] = "cleared" };
    }

    // ── Helpers ──────────────────────────────────────────────────────

    private static List<string> GetItems(ToolContext? ctx)
    {
        if (ctx?.State is null) return [];
        if (!ctx.State.TryGetValue("shopping_list", out var raw) || raw is null) return [];

        // In-memory list (after add within same invocation)
        if (raw is List<string> list) return list;

        // JsonElement from server-persisted state
        if (raw is JsonElement je && je.ValueKind == JsonValueKind.Array)
            return je.EnumerateArray().Select(e => e.GetString() ?? "").Where(s => s.Length > 0).ToList();

        return [];
    }

    private static void SetItems(ToolContext? ctx, List<string> items)
    {
        if (ctx?.State is not null)
            ctx.State["shopping_list"] = items;
    }
}
