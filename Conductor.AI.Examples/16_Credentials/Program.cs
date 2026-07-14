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
// Credentials — per-user secrets resolved from the credential store.
//
// Demonstrates [Tool(Credentials = ["GITHUB_TOKEN"])] which tells the
// server to resolve GITHUB_TOKEN from the credential store and inject
// it into the tool's execution environment before the worker runs.
//
// Setup (one-time):
//   agentspan credentials set GITHUB_TOKEN <your-token>
//
// The declared credential name is stamped onto this tool's TaskDef at
// registration; a capable server resolves and delivers the value on the
// wire-only Task.RuntimeMetadata at poll time, which the worker injects
// into the process environment for the duration of the handler call.
//
// Requirements:
//   - Conductor/Agentspan server running at CONDUCTOR_SERVER_URL
//     (or AGENTSPAN_SERVER_URL as a fallback)
//   - AGENTSPAN_LLM_MODEL set in environment
//   - GITHUB_TOKEN stored via `agentspan credentials set`

using System.Net.Http.Headers;
using System.Text.Json;
using Conductor.AI;
using Conductor.AI.Examples;

var tools = ToolRegistry.FromInstance(new GitHubTools());

var agent = new Agent("github_agent")
{
    Model = Settings.LlmModel,
    Instructions =
        "You are a GitHub assistant. You can list repositories for a user. " +
        "Always report how many repos were found.",
    Tools = tools,
};

await using var runtime = new AgentRuntime();
var result = await runtime.RunAsync(
    agent,
    "List the 5 most recently updated repos for the 'agentspan-ai' GitHub org.");

result.PrintResult();

// ── Tool class ────────────────────────────────────────────────────────

internal sealed class GitHubTools
{
    private static readonly HttpClient _http = new();

    [Tool("List public repositories for a GitHub user or org.",
          Credentials = ["GITHUB_TOKEN"])]
    public async Task<Dictionary<string, object>> ListGithubRepos(
        string username, ToolContext? ctx = null)
    {
        // The worker resolves GITHUB_TOKEN from the server and injects it into
        // the process environment for the duration of this handler call
        // (under a process-wide lock — see secret-injection-contract.md).
        var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN") ?? "";

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"https://api.github.com/users/{username}/repos?per_page=5&sort=updated");

        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("agentspan-csharp-sdk", "0.1"));
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            var response = await _http.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return new() { ["error"] = $"GitHub API error {(int)response.StatusCode}" };

            var repos = JsonSerializer.Deserialize<JsonElement[]>(body) ?? [];
            var list = repos.Select(r => new
            {
                name = r.GetProperty("name").GetString(),
                stars = r.GetProperty("stargazers_count").GetInt32(),
            }).ToList();

            return new()
            {
                ["username"] = username,
                ["repos"] = list,
                ["authenticated"] = !string.IsNullOrEmpty(token),
            };
        }
        catch (Exception ex)
        {
            return new() { ["error"] = ex.Message };
        }
    }
}
