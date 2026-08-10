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
// Router Agent — LLM-based routing to specialists.
//
// Demonstrates the ROUTER strategy where a dedicated router/classifier agent
// decides which specialist sub-agent handles each request.
//
// Architecture:
//   team (ROUTER, router=selector)
//   ├── planner   — design/architecture tasks
//   ├── coder     — implementation tasks
//   └── reviewer  — code review tasks
//
// The selector is a separate agent whose only job is routing.
// It is NOT one of the specialist agents.
//
// Requirements:
//   - Conductor server with LLM support
//   - CONDUCTOR_SERVER_URL=http://localhost:8080/api in environment
//   - CONDUCTOR_AGENT_LLM_MODEL set in environment (optional)

using Conductor.AI;
using Conductor.AI.Examples;

// ── Specialist agents ───────────────────────────────────────────────

var planner = new Agent("planner")
{
    Model = Settings.LlmModel,
    Instructions = "You create implementation plans. Break down tasks into clear numbered steps.",
};

var coder = new Agent("coder")
{
    Model = Settings.LlmModel,
    Instructions = "You write code. Output clean, well-documented C# code.",
};

var reviewer = new Agent("reviewer")
{
    Model = Settings.LlmModel,
    Instructions = "You review code. Check for bugs, style issues, and suggest improvements.",
};

// ── Dedicated router/classifier (separate from specialists) ─────────

var selector = new Agent("dev_team_selector")
{
    Model = Settings.LlmModel,
    Instructions =
        "You are a request classifier. Select the right specialist:\n" +
        "- planner: for design, architecture, or planning tasks\n" +
        "- coder: for writing or implementing code\n" +
        "- reviewer: for reviewing, auditing, or improving existing code",
};

// ── Router team ─────────────────────────────────────────────────────

var team = new Agent("dev_team")
{
    Model = Settings.LlmModel,
    Agents = [planner, coder, reviewer],
    Strategy = Strategy.Router,
    Router = selector, // dedicated classifier — not one of the specialists
};

// ── Run ─────────────────────────────────────────────────────────────

await using var runtime = new AgentRuntime();
var result = await runtime.RunAsync(
    team,
    "Write a C# method to validate email addresses using regex");
result.PrintResult();
