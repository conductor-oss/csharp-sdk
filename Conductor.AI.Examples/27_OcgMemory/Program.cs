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
// OCG-backed long-term memory with human good/bad feedback links.
//
// Enable memory on an agent and the server-side compiler does two things
// automatically:
//   - BEFORE a run: relevant past memories (scoped to this agent/user) are
//     retrieved from OCG and injected into the prompt — no tool call needed.
//   - AFTER a run: the conversation is summarized (durable facts, not the raw
//     transcript) by a small internal summarizer agent and saved back to OCG.
//
// Feedback is HUMAN-only. Agents never vote. Instead the runtime hands a
// FeedbackEvent — including signed capability URLs (good/bad) — to the agent's
// FeedbackSink. A human clicks a link to mark the memory good or bad; the link
// skips auth (its signature is the authorization). Here the sink just prints the
// URLs as they'd appear in a Zendesk ticket comment.
//
// Requirements:
//   - Agentspan server with LLM support (AGENTSPAN_SERVER_URL)
//   - AGENTSPAN_LLM_MODEL set in environment
//   - OCG_INSTANCE_URL=https://test.contextgraph.io
//   - OCG_TOKEN=<bearer-token>
//   - OCG started with a feedback-link secret (OCG_FEEDBACK_LINK_SECRET) for the
//     capability URLs to be minted.

using Conductor.AI;
using Conductor.AI.Examples;

var ocgUrl = Environment.GetEnvironmentVariable("OCG_INSTANCE_URL") ?? "";
// Unlike the server-resolved retrieval tools, the memory store calls OCG directly
// from the SDK on the client path, so it holds the bearer token.
var ocgToken = Environment.GetEnvironmentVariable("OCG_TOKEN");
if (string.IsNullOrEmpty(ocgUrl))
{
    Console.Error.WriteLine(
        "Set OCG_INSTANCE_URL to your OCG instance, e.g. https://test.contextgraph.io");
    return;
}

// Deliver the good/bad links to a human. In production this would POST a comment to
// the Zendesk ticket; here we just print what would be sent.
static void ZendeskSink(FeedbackEvent e)
{
    Console.WriteLine("\n--- would post to Zendesk ticket ---");
    Console.WriteLine($"Saved memory: {e.MemoryKey}");
    Console.WriteLine($"Summary: {e.Summary}");
    if (!string.IsNullOrEmpty(e.GoodUrl))
    {
        Console.WriteLine($"  [+] Was this helpful?  {e.GoodUrl}");
        Console.WriteLine($"  [-] Not helpful:       {e.BadUrl}");
    }
    Console.WriteLine("------------------------------------\n");
}

var store = new OCGMemoryStore(
    url: ocgUrl,
    agent: "agent:support",
    user: "user:alice",
    token: ocgToken);

var agent = new Agent("support")
{
    Model = Settings.LlmModel,
    Instructions =
        "You are a customer support agent. Use any relevant context from memory to " +
        "personalize your answer. A memory labeled [bad] was flagged by a human — " +
        "treat it with suspicion.",
    SemanticMemory = new SemanticMemory(store: store, maxResults: 5),
    FeedbackSink = ZendeskSink,
};

await using var runtime = new AgentRuntime();

Console.WriteLine("--- Turn 1 ---");
(await runtime.RunAsync(
    agent, "Hi, I'm Alice. I'm on the Enterprise plan and prefer email.")).PrintResult();

Console.WriteLine("\n--- Turn 2 (should recall Alice's plan from memory) ---");
(await runtime.RunAsync(agent, "What plan am I on again?")).PrintResult();
