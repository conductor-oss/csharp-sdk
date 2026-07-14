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
// OpenAi06 — Model Settings.
//
// Two agents with identical wiring but different stylistic intent —
// a high-temperature creative writer and a low-temperature precise
// code reviewer.
//
// Note: simplified from Java original — temperature / max_tokens not
// surfaced on the OpenAIAgent builder yet. Intended settings from the
// Python original:
//   - creative_writer: temperature=0.9, max_tokens=500
//   - code_reviewer:   temperature=0.1, max_tokens=300
//
// Requirements:
//   - AGENTSPAN_SERVER_URL=http://localhost:8080/api
//   - AGENTSPAN_LLM_MODEL=openai/gpt-4o-mini

using Conductor.AI;
using Conductor.AI.Examples;
using Conductor.AI.OpenAI;

var creativeAgent = OpenAIAgent.Builder()
    .Name("creative_writer")
    .Instructions(
        "You are a creative writing assistant. Write with vivid imagery " +
        "and unexpected metaphors. Be bold and imaginative.")
    .Model(Settings.LlmModel)
    .Build();

var preciseAgent = OpenAIAgent.Builder()
    .Name("code_reviewer")
    .Instructions(
        "You are a precise code reviewer. Analyze code snippets for bugs, " +
        "security issues, and best practices. Be concise and specific.")
    .Model(Settings.LlmModel)
    .Build();

await using var runtime = new AgentRuntime(new AgentRuntimeOptions { ServerUrl = Settings.ServerUrl });

Console.WriteLine("=== Creative Agent (temp=0.9) ===");
var creative = await runtime.RunAsync(
    creativeAgent,
    "Write a two-sentence story about a robot learning to paint.");
creative.PrintResult();

Console.WriteLine("\n=== Precise Agent (temp=0.1) ===");
var precise = await runtime.RunAsync(
    preciseAgent,
    "Review this Python code: `data = eval(user_input)`");
precise.PrintResult();
