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
// Constrained Speaker Transitions — control which agents can follow which.
//
// AllowedTransitions restricts which agent can speak after which.
// Enforces a code review workflow:
//   developer → reviewer (code must be reviewed)
//   reviewer  → developer OR approver
//   approver  → developer (request revisions)
//
// Requirements:
//   - Conductor server with LLM support
//   - CONDUCTOR_SERVER_URL=http://localhost:8080/api in environment
//   - CONDUCTOR_AGENT_LLM_MODEL set in environment

using Conductor.AI;
using Conductor.AI.Examples;

// ── Code review team ─────────────────────────────────────────────────

var developer = new Agent("developer")
{
    Model = Settings.LlmModel,
    Instructions =
        "You are a software developer. Write or revise code based on feedback. " +
        "Keep responses focused on code changes.",
};

var reviewer = new Agent("reviewer")
{
    Model = Settings.LlmModel,
    Instructions =
        "You are a code reviewer. Review the developer's code for bugs, style, " +
        "and best practices. Provide specific, actionable feedback.",
};

var approver = new Agent("approver")
{
    Model = Settings.LlmModel,
    Instructions =
        "You are the tech lead. Review the code and feedback. Either approve " +
        "the code or request revisions with specific guidance.",
};

// Constrained transitions enforce the review protocol
var codeReview = new Agent("code_review")
{
    Model = Settings.LlmModel,
    Agents = [developer, reviewer, approver],
    Strategy = Strategy.RoundRobin,
    MaxTurns = 6,
    AllowedTransitions = new Dictionary<string, List<string>>
    {
        ["developer"] = ["reviewer"],
        ["reviewer"] = ["developer", "approver"],
        ["approver"] = ["developer"],
    },
};

// ── Run ─────────────────────────────────────────────────────────────

await using var runtime = new AgentRuntime();
var result = await runtime.RunAsync(
    codeReview,
    "Write a C# method to validate email addresses using regex.");

result.PrintResult();
