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
// Deploy — register agents on the server (CI/CD step).
//
// Demonstrates:
//   - runtime.DeployAsync() to compile and register multiple agents
//   - DeploymentInfo result with agent name and registered workflow name
//   - CI/CD use case: push agent definitions without executing them
//
// DeployAsync() sends agent configs to the server which compiles them into
// Conductor workflow definitions and registers task definitions. No local
// workers are started and no execution happens.
//
// In production, run this once during CI/CD:
//   dotnet run --project 63_Deploy  # registers agent definitions
//
// Then run the worker service separately (see 63b_Serve) to keep workers alive.
//
// Requirements:
//   - CONDUCTOR_SERVER_URL=http://localhost:8080/api in environment
//   - CONDUCTOR_AGENT_LLM_MODEL set in environment

using Conductor.AI;
using Conductor.AI.Examples;

// ── Agent definitions ───────────────────────────────────────────

var docAssistant = new Agent("doc_assistant_63")
{
    Model = Settings.LlmModel,
    Instructions = "Help users find documentation. Use search_docs to look up answers.",
    Tools = ToolRegistry.FromInstance(new DocTools()),
};

var opsBot = new Agent("ops_bot_63")
{
    Model = Settings.LlmModel,
    Instructions = "Monitor service health. Use check_status to inspect services.",
    Tools = ToolRegistry.FromInstance(new OpsTools()),
};

// ── Deploy (no workers, no execution) ──────────────────────────

await using var runtime = new AgentRuntime();

Console.WriteLine("Deploying agents...\n");
var results = await runtime.DeployAsync(docAssistant, opsBot);

foreach (var info in results)
    Console.WriteLine($"  Deployed: {info.AgentName} -> {info.RegisteredName}");

Console.WriteLine("\nAgents deployed. Run 63b_Serve to start worker processes.");
Console.WriteLine("Run 63c_RunByName to execute without local workers.");

// ── Tool classes ─────────────────────────────────────────────

internal sealed class DocTools
{
    [Tool("Search internal documentation.")]
    public string SearchDocs(string query)
        => $"Found 3 results for: {query}";
}

internal sealed class OpsTools
{
    [Tool("Check service health status.")]
    public string CheckStatus(string service)
        => $"{service}: healthy";
}
