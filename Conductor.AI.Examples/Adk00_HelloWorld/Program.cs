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
// Adk00 — Hello World.
//
// Minimal Google ADK greeting agent — no tools, no structured output,
// one turn. The simplest possible ADK agent.
//
// Requirements:
//   - CONDUCTOR_SERVER_URL=http://localhost:8080/api
//   - CONDUCTOR_AGENT_LLM_MODEL=openai/gpt-4o-mini

using Conductor.AI;
using Conductor.AI.Examples;
using Conductor.AI.GoogleADK;

var agent = GoogleADKAgent.Builder()
    .Name("greeter")
    .Model(Settings.LlmModel)
    .Instruction("You are a friendly greeter. Reply with a warm hello and one fun fact.")
    .Build();

await using var runtime = new AgentRuntime(new AgentRuntimeOptions { ServerUrl = Settings.ServerUrl });
var result = await runtime.RunAsync(agent, "Say hello!");
result.PrintResult();
