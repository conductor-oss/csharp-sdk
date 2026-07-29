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
// Nested Strategies — parallel agents inside a sequential pipeline.
//
// Demonstrates composing strategies: a Parallel phase runs multiple
// research agents concurrently, followed by a sequential summarizer.
//
//   pipeline = parallel_research >> summarizer
//
// Requirements:
//   - Conductor server with LLM support
//   - CONDUCTOR_SERVER_URL=http://localhost:8080/api in environment
//   - CONDUCTOR_AGENT_LLM_MODEL set in environment

using Conductor.AI;
using Conductor.AI.Examples;

// ── Parallel research phase ───────────────────────────────────

var marketAnalyst = new Agent("market_analyst_52")
{
    Model = Settings.LlmModel,
    Instructions =
        "You are a market analyst. Analyze the market size, growth rate, " +
        "and key players for the given topic. Be concise (3-4 bullet points).",
};

var riskAnalyst = new Agent("risk_analyst_52")
{
    Model = Settings.LlmModel,
    Instructions =
        "You are a risk analyst. Identify the top 3 risks: regulatory, " +
        "technical, and competitive. Be concise.",
};

// Both analysts run concurrently
var parallelResearch = new Agent("research_phase_52")
{
    Agents = [marketAnalyst, riskAnalyst],
    Strategy = Strategy.Parallel,
};

// ── Sequential summarizer ─────────────────────────────────────

var summarizer = new Agent("summarizer_52")
{
    Model = Settings.LlmModel,
    Instructions =
        "You are an executive briefing writer. Synthesize the market analysis " +
        "and risk assessment into a concise executive summary (1 paragraph).",
};

// ── Pipeline: parallel research → summary ─────────────────────

var pipeline = parallelResearch >> summarizer;

// ── Run ───────────────────────────────────────────────────────

await using var runtime = new AgentRuntime();
var result = await runtime.RunAsync(
    pipeline,
    "Launching an AI-powered healthcare diagnostics tool in the US");

result.PrintResult();
