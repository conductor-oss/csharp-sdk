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
// OpenAi01 — Basic OpenAI Agents SDK-shape agent.
//
// Simplest possible: a name + instructions + model wired through
// OpenAIAgent.From / Builder. The server's OpenAINormalizer compiles
// this into a Conductor workflow.
//
// Requirements:
//   - AGENTSPAN_SERVER_URL=http://localhost:6767/api
//   - AGENTSPAN_LLM_MODEL=openai/gpt-4o-mini

using Conductor.AI;
using Conductor.AI.Examples;
using Conductor.AI.OpenAI;

var agent = OpenAIAgent.Builder()
    .Name("greeter")
    .Instructions("You are a friendly assistant. Keep your responses concise and helpful.")
    .Model(Settings.LlmModel)
    .Build();

await using var runtime = new AgentRuntime(new AgentRuntimeOptions { ServerUrl = Settings.ServerUrl });
var result = await runtime.RunAsync(
    agent,
    "Say hello and tell me a fun fact about the C# programming language.");
result.PrintResult();
