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
// Agent Discussion — round-robin debate piped to a summarizer.
//
// Two agents debate a topic using Strategy.RoundRobin for 6 turns,
// then the transcript is piped (>>) to a summarizer agent for a
// balanced conclusion.
//
// Requirements:
//   - Agentspan server with LLM support
//   - AGENTSPAN_SERVER_URL=http://localhost:8080/api in environment
//   - AGENTSPAN_LLM_MODEL set in environment

using Conductor.AI;
using Conductor.AI.Examples;

// ── Discussion participants ──────────────────────────────────────────

var optimist = new Agent("optimist")
{
    Model = Settings.LlmModel,
    Instructions =
        "You are an optimistic technologist debating a topic. " +
        "Argue FOR the topic. Keep your response to 2-3 concise paragraphs. " +
        "Acknowledge the other side's points before making your case.",
};

var skeptic = new Agent("skeptic")
{
    Model = Settings.LlmModel,
    Instructions =
        "You are a thoughtful skeptic debating a topic. " +
        "Raise concerns and argue AGAINST the topic. " +
        "Keep your response to 2-3 concise paragraphs. " +
        "Acknowledge the other side's points before making your case.",
};

var summarizer = new Agent("debate_summarizer")
{
    Model = Settings.LlmModel,
    Instructions =
        "You are a neutral moderator. You have just observed a debate " +
        "between an optimist and a skeptic. Summarize the key arguments " +
        "from both sides and provide a balanced conclusion. " +
        "Structure your response with: Key Arguments For, " +
        "Key Arguments Against, and Balanced Conclusion.",
};

// ── Round-robin discussion: 6 turns (3 rounds of back-and-forth) ────

var discussion = new Agent("discussion")
{
    Model = Settings.LlmModel,
    Agents = [optimist, skeptic],
    Strategy = Strategy.RoundRobin,
    MaxTurns = 6,
};

// Pipe discussion transcript to summarizer
var pipeline = discussion >> summarizer;

// ── Run ─────────────────────────────────────────────────────────────

await using var runtime = new AgentRuntime();
var result = await runtime.RunAsync(
    pipeline,
    "Debate: Will AI replace most knowledge workers within 10 years?");

result.PrintResult();
