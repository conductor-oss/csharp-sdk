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
// Credentials — in-process tools with environment variable injection.
//
// Demonstrates:
//   - [Tool(Credentials = ["GITHUB_TOKEN"])] on a class method
//   - The SDK resolves the credential from the server and injects it
//     into the process environment before the handler runs
//   - Environment.GetEnvironmentVariable("GITHUB_TOKEN") to access it
//   - Graceful degradation when the credential isn't found
//
// In C#, all tool workers run in the same process (no subprocess isolation).
// The Conductor SDK automatically resolves declared credentials from the
// server's credential store and injects them as environment variables before
// each tool invocation.
//
// Setup (one-time):
//   agentspan credentials set GITHUB_TOKEN <your-github-token>
//
// Requirements:
//   - CONDUCTOR_SERVER_URL=http://localhost:8080/api in environment
//   - CONDUCTOR_AGENT_LLM_MODEL set in environment
//   - GITHUB_TOKEN stored via `agentspan credentials set`

using System.Net.Http.Headers;
using System.Text.Json;
using Conductor.AI;
using Conductor.AI.Examples;

var agent = new Agent("github_agent_16b")
{
    Model = Settings.LlmModel,
    Instructions =
        "You are a GitHub assistant. You can look up user profiles. " +
        "Always show the number of public repos and followers.",
    Tools = ToolRegistry.FromInstance(new GitHubLookupTools()),
};

await using var runtime = new AgentRuntime();
var result = await runtime.RunAsync(agent, "Look up the GitHub profile for 'torvalds'.");
result.PrintResult();

// ── Tool class ─────────────────────────────────────────────────

internal sealed class GitHubLookupTools
{
    private static readonly HttpClient _http = new();

    // The SDK injects GITHUB_TOKEN into Environment.GetEnvironmentVariable
    // before this handler is called. If the credential isn't stored on the
    // server, it falls back to the local environment (useful in development).
    [Tool("Look up a GitHub user's public profile.",
          Credentials = ["GITHUB_TOKEN"])]
    public async Task<Dictionary<string, object>> LookupGithubUser(string username)
    {
        var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        if (string.IsNullOrEmpty(token))
            return new() { ["error"] = "GITHUB_TOKEN not found — run: agentspan credentials set GITHUB_TOKEN <your-token>" };

        var request = new HttpRequestMessage(
            HttpMethod.Get, $"https://api.github.com/users/{username}");
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("conductor-csharp", "0.1"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            var response = await _http.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return new() { ["error"] = $"GitHub API error {(int)response.StatusCode}: {body[..Math.Min(100, body.Length)]}" };

            var user = JsonSerializer.Deserialize<JsonElement>(body);
            return new()
            {
                ["login"] = user.GetProperty("login").GetString() ?? username,
                ["name"] = user.TryGetProperty("name", out var n) ? (n.GetString() ?? "(no name)") : "(no name)",
                ["public_repos"] = user.GetProperty("public_repos").GetInt32(),
                ["followers"] = user.GetProperty("followers").GetInt32(),
                ["bio"] = user.TryGetProperty("bio", out var b) ? (b.GetString() ?? "") : "",
            };
        }
        catch (Exception ex)
        {
            return new() { ["error"] = ex.Message };
        }
    }
}
