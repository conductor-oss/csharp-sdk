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
// Adk11 — Sequential Agent Pipeline.
//
// Python's SequentialAgent runs sub-agents in fixed order with outputs
// flowing to the next. We model the same intent through a coordinator
// with sub-agents and instructions that dictate the execution order.
//
// Requirements:
//   - CONDUCTOR_SERVER_URL=http://localhost:8080/api
//   - CONDUCTOR_AGENT_LLM_MODEL=openai/gpt-4o-mini

using Conductor.AI;
using Conductor.AI.Examples;
using Conductor.AI.GoogleADK;

var researcher = GoogleADKAgent.Builder()
    .Name("researcher")
    .Model(Settings.LlmModel)
    .Instruction(
        "You are a research assistant. Given the user's topic, " +
        "provide 3 key facts about it in a numbered list. Be concise.")
    .Build();

var writer = GoogleADKAgent.Builder()
    .Name("writer")
    .Model(Settings.LlmModel)
    .Instruction(
        "You are a skilled writer. Take the research provided in the conversation " +
        "and write a single engaging paragraph summarizing the key points. " +
        "Keep it under 100 words.")
    .Build();

var editor = GoogleADKAgent.Builder()
    .Name("editor")
    .Model(Settings.LlmModel)
    .Instruction(
        "You are an editor. Review the paragraph from the writer and improve it. " +
        "Fix any issues with clarity, grammar, or flow. Output only the final polished paragraph.")
    .Build();

var pipeline = GoogleADKAgent.Builder()
    .Name("content_pipeline")
    .Model(Settings.LlmModel)
    .Instruction(
        "You orchestrate a content pipeline. Execute the steps in this order:\n" +
        "1. researcher gathers 3 key facts\n" +
        "2. writer composes a paragraph from those facts\n" +
        "3. editor polishes the paragraph\n" +
        "Return the editor's final paragraph.")
    .SubAgents(researcher, writer, editor)
    .Build();

await using var runtime = new AgentRuntime(new AgentRuntimeOptions { ServerUrl = Settings.ServerUrl });
var result = await runtime.RunAsync(pipeline, "The history of the Internet");
Console.WriteLine($"Status: {result.Status}");
result.PrintResult();
