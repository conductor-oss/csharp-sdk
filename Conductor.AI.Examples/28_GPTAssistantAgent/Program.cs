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
// GPTAssistantAgent — wrap OpenAI Assistants API as an Agentspan agent.
//
// GPTAssistantAgent.Create() builds an Agent whose internal tool
// creates an OpenAI Thread, posts a message, polls the Run to completion,
// and returns the assistant's reply.
//
// Two modes:
//   1. Create a new assistant on the fly (this example)
//   2. Use an existing assistant by ID
//
// Requirements:
//   - OPENAI_API_KEY in environment
//   - Agentspan server with LLM support
//   - AGENTSPAN_SERVER_URL=http://localhost:8080/api in environment
//   - AGENTSPAN_LLM_MODEL set in environment

using Conductor.AI;
using Conductor.AI.Examples;

// ── Example 1: Create assistant on the fly ───────────────────────────

var dataAnalyst = GPTAssistantAgent.Create(
    name: "data_analyst",
    model: Settings.LlmModel,
    instructions: "You are a data analyst. Use the code interpreter to analyze data and perform calculations.",
    openAiTools: [new Dictionary<string, object> { ["type"] = "code_interpreter" }]);

// ── Example 2: Use an existing assistant ─────────────────────────────

// If you already have an assistant created in the OpenAI dashboard:
// var existingAssistant = GPTAssistantAgent.FromExistingAssistant(
//     name:        "my_assistant",
//     assistantId: "asst_abc123def456");

// ── Run ───────────────────────────────────────────────────────────────

await using var runtime = new AgentRuntime();

Console.WriteLine("--- GPT Assistant with Code Interpreter ---");
var result = await runtime.RunAsync(
    dataAnalyst,
    "Calculate the standard deviation of these numbers: 4, 8, 15, 16, 23, 42");

result.PrintResult();
