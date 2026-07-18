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
// Basic Agent — 5-line hello world.
//
// Demonstrates the simplest possible agent: define an agent, call
// runtime.RunAsync(), and print the result.
//
// Requirements:
//   - Agentspan server with LLM support
//   - AGENTSPAN_SERVER_URL=http://localhost:8080/api in environment
//   - AGENTSPAN_LLM_MODEL set in environment (optional, defaults to openai/gpt-4o-mini)

using Conductor.AI;
using Conductor.AI.Examples;

var agent = new Agent("greeter")
{
    Model = Settings.LlmModel,
    Instructions = "You are a friendly assistant. Keep responses brief.",
};

var prompt = "Say hello and tell me a fun fact about C#.";

await using var runtime = new AgentRuntime();
var result = await runtime.RunAsync(agent, prompt);
result.PrintResult();
