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
// Credentials — GitHub CLI (gh) with automatic credential injection.
//
// Demonstrates:
//   - CliTool.Create(allowedCommands: ["gh"], credentials: ["GH_TOKEN"])
//     gives the agent a run_command tool pre-wired to inject the token
//   - GH_TOKEN is auto-injected into the tool environment before each
//     gh command — no subprocess boilerplate needed in the handler
//   - Requesting structured output via --json for reliable parsing
//
// Setup (one-time):
//   agentspan credentials set GITHUB_TOKEN <your-github-token>
//
// Requirements:
//   - AGENTSPAN_SERVER_URL=http://localhost:6767/api in environment
//   - AGENTSPAN_LLM_MODEL set in environment
//   - GITHUB_TOKEN stored via `agentspan credentials set`
//   - gh CLI installed (https://cli.github.com)

using Conductor.AI;
using Conductor.AI.Examples;

// CliTool.Create with credentials — GITHUB_TOKEN is injected before each gh invocation.
// Only `gh` is whitelisted; any other command is rejected by the SDK.
// Note: gh CLI accepts GITHUB_TOKEN directly (same as GH_TOKEN).
var ghTool = CliTool.Create(
    allowedCommands: ["gh"],
    credentials: ["GITHUB_TOKEN"]);

var agent = new Agent("github_cli_agent_16d")
{
    Model = Settings.LlmModel,
    Tools = [ghTool],
    Instructions =
        "You are a GitHub assistant that uses the `gh` CLI tool. " +
        "GH_TOKEN is already set in the environment — gh will use it automatically. " +
        "Use --json for structured output when listing repos, issues, or PRs.",
};

await using var runtime = new AgentRuntime();
var result = await runtime.RunAsync(
    agent,
    "List the 5 most recently updated repos for the 'agentspan-ai' organisation.");
result.PrintResult();
