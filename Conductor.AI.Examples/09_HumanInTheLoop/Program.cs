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
// Human-in-the-Loop — approval workflows.
//
// Demonstrates how tools with ApprovalRequired=true pause the workflow
// until a human approves or rejects the action via the console.
//
// Requirements:
//   - Agentspan server with LLM support
//   - AGENTSPAN_SERVER_URL=http://localhost:8080/api in environment
//   - AGENTSPAN_LLM_MODEL set in environment

using Conductor.AI;
using Conductor.AI.Examples;

// ── Tools ────────────────────────────────────────────────────────────

var tools = ToolRegistry.FromInstance(new BankingTools());

// ── Agent ─────────────────────────────────────────────────────────────

var agent = new Agent("banker")
{
    Model = Settings.LlmModel,
    Instructions =
        "You are a banking assistant. Use check_balance for balance inquiries. " +
        "When asked to transfer money, first check the balance, then call " +
        "transfer_funds to request the transfer. The runtime will pause for " +
        "human approval before the transfer executes.",
    Tools = tools,
};

// ── Run with HITL streaming ──────────────────────────────────────────

await using var runtime = new AgentRuntime();
var handle = await runtime.StartAsync(
    agent, "Transfer $500 from ACC-789 to ACC-456. Check the balance first.");

Console.WriteLine($"Started: {handle.ExecutionId}\n");

await foreach (var ev in handle.StreamAsync())
{
    switch (ev.Type)
    {
        case EventType.Thinking:
            Console.WriteLine($"  [thinking] {ev.Content}");
            break;

        case EventType.ToolCall:
            Console.WriteLine($"  [tool_call] {ev.ToolName}(...)");
            break;

        case EventType.ToolResult:
            Console.WriteLine($"  [tool_result] {ev.ToolName} -> {ev.Result}");
            break;

        case EventType.Waiting:
            Console.WriteLine("\n--- Human approval required ---");
            Console.Write("  Approve transfer? (y/n): ");
            var input = Console.ReadLine()?.Trim().ToLower();
            var approved = input is "y" or "yes";

            if (approved)
            {
                await handle.ApproveAsync();
                Console.WriteLine("  Approved.\n");
            }
            else
            {
                await handle.RejectAsync("Rejected by operator.");
                Console.WriteLine("  Rejected.\n");
            }
            break;

        case EventType.Done:
            Console.WriteLine($"\nDone: {ev.Content ?? ev.Status}");
            break;

        case EventType.Error:
            Console.WriteLine($"\nError: {ev.Content}");
            break;
    }
}

// ── Tool class ────────────────────────────────────────────────────────

internal sealed class BankingTools
{
    [Tool("Check the balance of an account.")]
    public Dictionary<string, object> CheckBalance(string accountId) =>
        new() { ["account_id"] = accountId, ["balance"] = 15000.00 };

    [Tool("Request a funds transfer; pauses for human approval before execution.",
          ApprovalRequired = true, TimeoutSeconds = 300)]
    public Dictionary<string, object> TransferFunds(string fromAcct, string toAcct, double amount) =>
        new() { ["status"] = "completed", ["from"] = fromAcct, ["to"] = toAcct, ["amount"] = amount };
}
