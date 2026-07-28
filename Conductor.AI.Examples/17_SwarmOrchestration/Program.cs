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
// Swarm Orchestration — LLM-driven agent handoffs via transfer tools.
//
// Strategy.Swarm gives the front-line agent transfer_to_<peer> tools.
// The LLM decides which specialist to hand off to by calling the
// appropriate transfer tool.
//
//   support (SWARM)
//   ├── refund_specialist
//   └── tech_support
//
// Requirements:
//   - Conductor server with LLM support
//   - CONDUCTOR_SERVER_URL=http://localhost:8080/api in environment
//   - CONDUCTOR_AGENT_LLM_MODEL set in environment

using Conductor.AI;
using Conductor.AI.Examples;

// ── Specialist agents ────────────────────────────────────────────────

var refundAgent = new Agent("refund_specialist")
{
    Model = Settings.LlmModel,
    Instructions =
        "You are a refund specialist. Process the customer's refund request. " +
        "Check eligibility, confirm the refund amount, and state the timeline. " +
        "Be empathetic and clear. Do NOT ask follow-up questions — " +
        "just process the refund based on what the customer told you.",
};

var techAgent = new Agent("tech_support")
{
    Model = Settings.LlmModel,
    Instructions =
        "You are a technical support specialist. Diagnose the customer's " +
        "technical issue and provide clear troubleshooting steps.",
};

// ── Front-line support agent with SWARM handoffs ─────────────────────

var support = new Agent("support")
{
    Model = Settings.LlmModel,
    Instructions =
        "You are the front-line customer support agent. Triage customer requests. " +
        "If the customer needs a refund, transfer to the refund specialist. " +
        "If they have a technical issue, transfer to tech support. " +
        "Use the transfer tools available to you to hand off the conversation.",
    Agents = [refundAgent, techAgent],
    Strategy = Strategy.Swarm,
    MaxTurns = 3,
};

// ── Run two scenarios ─────────────────────────────────────────────────

await using var runtime = new AgentRuntime();

Console.WriteLine("--- Refund scenario ---");
var result1 = await runtime.RunAsync(
    support,
    "I bought a product last week and it arrived damaged. I want my money back.");
result1.PrintResult();

Console.WriteLine("--- Technical issue scenario ---");
var result2 = await runtime.RunAsync(
    support,
    "My app keeps crashing whenever I try to upload files. It started after the latest update.");
result2.PrintResult();
