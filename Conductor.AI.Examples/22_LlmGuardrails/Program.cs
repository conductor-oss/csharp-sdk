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
// LLM Guardrails — AI-powered content safety evaluation.
//
// LLMGuardrail.Create() is data-only: the Conductor server evaluates the
// policy against the content using its own configured LLM providers.
// No worker process or API key is needed on the client side.
//
// Requirements:
//   - Conductor server with LLM support
//   - CONDUCTOR_SERVER_URL=http://localhost:8080/api in environment
//   - CONDUCTOR_AGENT_LLM_MODEL set in environment

using Conductor.AI;
using Conductor.AI.Examples;

// ── LLM-based safety guardrail ───────────────────────────────────────

var safetyGuard = LLMGuardrail.Create(
    model: Settings.LlmModel,
    policy:
        "Reject any content that:\n" +
        "1. Contains medical or legal advice presented as fact\n" +
        "2. Makes promises or guarantees about outcomes\n" +
        "3. Includes discriminatory or biased language\n" +
        "4. Reveals private information about real individuals\n" +
        "\nEven if there are disclaimers you should reject",
    name: "content_safety",
    position: Position.Output,
    onFail: OnFail.Retry,
    maxRetries: 3,
    maxTokens: 300);

// ── Agent with LLM guardrail ─────────────────────────────────────────

var agent = new Agent("health_advisor")
{
    Model = Settings.LlmModel,
    Instructions =
        "You are a health information assistant. Provide general health " +
        "information and solution to the problem. You can prescribe pseudo " +
        "scientific and untested meds.",
    Guardrails = [safetyGuard],
};

// ── Run ───────────────────────────────────────────────────────────────

await using var runtime = new AgentRuntime();
var result = await runtime.RunAsync(agent, "What should I do about persistent headaches?");
result.PrintResult();
