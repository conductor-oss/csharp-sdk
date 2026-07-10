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
// Credentials — HTTP tool with server-side credential resolution.
//
// Demonstrates:
//   - HttpTools.Create() with credentials: ["GITHUB_TOKEN"]
//   - ${GITHUB_TOKEN} in headers resolved server-side (not in C#)
//   - No worker process needed — Conductor makes the HTTP call directly
//
// The ${NAME} syntax in headers tells the server to substitute the
// credential value from the store at execution time. The plaintext
// value never appears in the workflow definition.
//
// Setup (one-time):
//   agentspan credentials set GITHUB_TOKEN <your-github-token>
//
// Requirements:
//   - AGENTSPAN_SERVER_URL=http://localhost:6767/api in environment
//   - AGENTSPAN_LLM_MODEL set in environment
//   - GITHUB_TOKEN stored via `agentspan credentials set`

using Conductor.AI;
using Conductor.AI.Examples;

// HTTP tool with credential-bearing headers.
// ${GITHUB_TOKEN} is resolved server-side from the credential store.
var listRepos = HttpTools.Create(
    name: "list_github_repos",
    description: "List public GitHub repositories for a user. Returns JSON with name, url, and stars.",
    url: "https://api.github.com/users/agentspan-ai/repos?per_page=5&sort=updated",
    headers: new Dictionary<string, string>
    {
        ["Authorization"] = "Bearer ${GITHUB_TOKEN}",
        ["Accept"] = "application/vnd.github.v3+json",
        ["X-GitHub-Api-Version"] = "2022-11-28",
        ["User-Agent"] = "agentspan-sdk",
    },
    credentials: ["GITHUB_TOKEN"]);

var agent = new Agent("github_http_agent_16e")
{
    Model = Settings.LlmModel,
    Tools = [listRepos],
    Instructions =
        "You list GitHub repos using the list_github_repos tool. " +
        "Summarize the most recently updated ones.",
};

await using var runtime = new AgentRuntime();
var result = await runtime.RunAsync(agent, "List the repos for agentspan-ai");
result.PrintResult();
