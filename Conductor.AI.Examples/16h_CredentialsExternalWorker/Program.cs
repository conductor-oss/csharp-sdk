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
// Credentials — External worker credential delivery.
//
// Demonstrates:
//   - External tool declared as a ToolDef with External = true and
//     Credentials = ["GITHUB_TOKEN"]. In C#, external tools must be
//     created as ToolDef objects directly (unlike local tools which use
//     [Tool] attributes and ToolRegistry.FromInstance).
//   - The external worker reads the resolved credential value directly off
//     the polled Task's RuntimeMetadata dictionary — the server delivers it
//     on the wire at poll time; there is no separate fetch call.
//   - Works for workers running in separate processes, containers, or machines.
//
// Two sides are shown:
//   1. Agent definition (declares the external tool with credentials)
//   2. External worker pattern (shown in comments; runs in a separate process)
//
// Setup (one-time):
//   agentspan credentials set GITHUB_TOKEN <your-github-token>
//
// Requirements:
//   - Conductor/Agentspan server running at CONDUCTOR_SERVER_URL
//     (or AGENTSPAN_SERVER_URL as a fallback)
//   - AGENTSPAN_LLM_MODEL set in environment
//   - GITHUB_TOKEN stored via `agentspan credentials set`
//   - An external worker polling for "github_lookup" tasks (see comments below)

using System.Text.Json.Nodes;
using Conductor.AI;
using Conductor.AI.Examples;

// ── External tool declaration ─────────────────────────────────
//
// External tools are created as ToolDef objects with External = true.
// They have no local handler — execution dispatches to an external
// Conductor worker process.

var githubLookup = new ToolDef
{
    Name = "github_lookup",
    Description = "Look up a GitHub user's public profile. Runs on an external worker.",
    External = true,
    Credentials = ["GITHUB_TOKEN"],
    InputSchema = new JsonObject
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["username"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "The GitHub username to look up.",
            },
        },
        ["required"] = new JsonArray { "username" },
    },
};

// ── Agent side: declare external tool with credentials ──────────

var agent = new Agent("external_cred_agent_16h")
{
    Model = Settings.LlmModel,
    Tools = [githubLookup],
    Instructions =
        "You can look up GitHub users. Use the github_lookup tool. " +
        "GITHUB_TOKEN is automatically resolved by the external worker.",
};

// ── Run ───────────────────────────────────────────────────────

Console.WriteLine("=== External Worker Credentials ===");
Console.WriteLine("The agent declares the external tool; a separate worker handles execution.");
Console.WriteLine("GITHUB_TOKEN arrives on the polled task's RuntimeMetadata — the server");
Console.WriteLine("delivers it at poll time, no separate fetch call needed.\n");
Console.WriteLine("Note: This example requires an external worker to be running.");
Console.WriteLine("See the comment block below for the worker implementation pattern.\n");

await using var runtime = new AgentRuntime();
var result = await runtime.RunAsync(agent, "Look up the GitHub profile for torvalds.");
result.PrintResult();

/*
 * ── External worker side (runs in a separate process) ─────────────────
 *
 * The external worker polls Conductor for tasks named "github_lookup". The
 * server delivers the resolved GITHUB_TOKEN value directly on the polled
 * Task's wire-only RuntimeMetadata dictionary — declared credential names
 * are stamped onto the task def at registration, and a capable server
 * (agentspan > 0.4.2 / conductor-oss >= 3.32.0-rc.8) resolves and delivers
 * the values at poll time. There is no separate fetch call, and a missing
 * credential should be treated as fail-closed (fail the task rather than
 * falling back to ambient process env).
 *
 * Implementation sketch:
 *
 *   using OrchestratorSDK.Client;           // Conductor .NET SDK
 *   using System.Net.Http.Headers;
 *
 *   var serverUrl = Environment.GetEnvironmentVariable("CONDUCTOR_SERVER_URL")
 *       ?? Environment.GetEnvironmentVariable("AGENTSPAN_SERVER_URL")!;
 *   var taskClient = new TaskResourceApi(new Configuration { BasePath = serverUrl });
 *
 *   while (true)
 *   {
 *       var task = await taskClient.PollAsync("github_lookup", workerid: "worker-1");
 *       if (task is null) { await Task.Delay(1000); continue; }
 *
 *       // The resolved value arrives directly on RuntimeMetadata — fail
 *       // closed if it's missing rather than reading ambient process env.
 *       if (!(task.RuntimeMetadata?.TryGetValue("GITHUB_TOKEN", out var token) ?? false))
 *       {
 *           // ... fail the task: FAILED_WITH_TERMINAL_ERROR, credential not delivered
 *           continue;
 *       }
 *
 *       // Use the credential to call the GitHub API
 *       var username = task.InputData["username"].ToString();
 *       using var ghClient = new HttpClient();
 *       ghClient.DefaultRequestHeaders.Authorization =
 *           new AuthenticationHeaderValue("Bearer", token);
 *       ghClient.DefaultRequestHeaders.Add("User-Agent", "agentspan-worker");
 *
 *       var resp = await ghClient.GetAsync($"https://api.github.com/users/{username}");
 *       // ... complete the Conductor task with the response data
 *   }
 */
