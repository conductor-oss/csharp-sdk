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
// Include Contents — control context passed to sub-agents.
//
// When IncludeContents = "none", a sub-agent starts with a clean slate
// and does NOT see the parent agent's conversation history. This is
// useful for sub-agents that should work independently.
//
// Requirements:
//   - Agentspan server with LLM support
//   - AGENTSPAN_SERVER_URL=http://localhost:6767/api in environment
//   - AGENTSPAN_LLM_MODEL set in environment

using Conductor.AI;
using Conductor.AI.Examples;

// ── Sub-agents ────────────────────────────────────────────────

// This sub-agent won't see the parent's conversation history
var independentSummarizer = new Agent("independent_summarizer_49")
{
    Model = Settings.LlmModel,
    Instructions = "You are a summarizer. Summarize any text given to you concisely.",
    Tools = ToolRegistry.FromInstance(new SummarizerTools49()),
    IncludeContents = "none",
};

// This sub-agent WILL see the parent's conversation history (default)
var contextAwareHelper = new Agent("context_aware_helper_49")
{
    Model = Settings.LlmModel,
    Instructions = "You are a helpful assistant that builds on prior conversation context.",
};

// ── Coordinator ───────────────────────────────────────────────

var coordinator = new Agent("coordinator_49")
{
    Model = Settings.LlmModel,
    Instructions =
        "You coordinate tasks. Route summarization requests to " +
        "independent_summarizer_49 and general questions to context_aware_helper_49.",
    Agents = [independentSummarizer, contextAwareHelper],
    Strategy = Strategy.Handoff,
};

// ── Run ───────────────────────────────────────────────────────

await using var runtime = new AgentRuntime();
var result = await runtime.RunAsync(
    coordinator,
    "Please summarize this: 'The quick brown fox jumps over the lazy dog. " +
    "This sentence contains every letter of the alphabet and is commonly " +
    "used for typography testing.'");

result.PrintResult();

// ── Tool class ────────────────────────────────────────────────

internal sealed class SummarizerTools49
{
    [Tool("Summarize a piece of text.")]
    public Dictionary<string, object> SummarizeText(string text)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var summary = string.Join(' ', words.Take(20)) + "...";
        return new() { ["summary"] = summary, ["word_count"] = words.Length };
    }
}
