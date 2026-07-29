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
// Sequential Pipeline — Agent >> Agent >> Agent.
//
// Demonstrates the sequential strategy where agents run in order and the
// output of each agent becomes the input of the next.
//
// Also shows the >> operator shorthand.
//
// Requirements:
//   - Conductor server with LLM support
//   - CONDUCTOR_SERVER_URL=http://localhost:8080/api in environment
//   - CONDUCTOR_AGENT_LLM_MODEL set in environment (optional)

using Conductor.AI;
using Conductor.AI.Examples;

// ── Pipeline agents ─────────────────────────────────────────────────

var researcher = new Agent("researcher")
{
    Model = Settings.LlmModel,
    Instructions =
        "You are a researcher. Given a topic, provide key facts and data points. " +
        "Be thorough but concise. Output raw research findings.",
};

var writer = new Agent("writer")
{
    Model = Settings.LlmModel,
    Instructions =
        "You are a writer. Take research findings and write a clear, engaging " +
        "article. Use headers and bullet points where appropriate.",
};

var editor = new Agent("editor")
{
    Model = Settings.LlmModel,
    Instructions =
        "You are an editor. Review the article for clarity, grammar, and tone. " +
        "Make improvements and output the final polished version.",
};

// ── Build pipeline with >> operator ────────────────────────────────
// researcher >> writer produces a sequential wrapper agent,
// then >> editor appends editor to its sub-agents list.

var pipeline = researcher >> writer >> editor;

// ── Run ─────────────────────────────────────────────────────────────

await using var runtime = new AgentRuntime();
var result = await runtime.RunAsync(pipeline, "The impact of AI agents on software development in 2025");
result.PrintResult();

// Option 2: Using Strategy parameter (equivalent)
// var pipeline = new Agent("content_pipeline")
// {
//     Model = Settings.LlmModel,
//     Agents = [researcher, writer, editor],
//     Strategy = Strategy.Sequential,
// };
