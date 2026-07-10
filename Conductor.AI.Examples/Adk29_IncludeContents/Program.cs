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
// Adk29 — Include Contents.
//
// ADK's include_contents="none" prevents a sub-agent from inheriting
// the parent's conversation history.
//
// Note: simplified from Java original — the GoogleADKAgent builder does
// not expose include_contents directly; the intent is documented in
// each agent's instruction.
//
// Requirements:
//   - AGENTSPAN_SERVER_URL=http://localhost:6767/api
//   - AGENTSPAN_LLM_MODEL=openai/gpt-4o-mini

using Conductor.AI;
using Conductor.AI.Examples;
using Conductor.AI.GoogleADK;

var independentSummarizer = GoogleADKAgent.Builder()
    .Name("independent_summarizer")
    .Model(Settings.LlmModel)
    .Instruction("You are a summarizer. Summarize any text given to you concisely.")
    .Build();

var contextAwareHelper = GoogleADKAgent.Builder()
    .Name("context_aware_helper")
    .Model(Settings.LlmModel)
    .Instruction("You are a helpful assistant that builds on prior conversation context.")
    .Build();

var coordinator = GoogleADKAgent.Builder()
    .Name("coordinator")
    .Model(Settings.LlmModel)
    .Instruction(
        "You coordinate tasks. Route summarization to independent_summarizer " +
        "and general questions to context_aware_helper.")
    .SubAgents(independentSummarizer, contextAwareHelper)
    .Build();

await using var runtime = new AgentRuntime(new AgentRuntimeOptions { ServerUrl = Settings.ServerUrl });
var result = await runtime.RunAsync(coordinator,
    "Please summarize this: 'The quick brown fox jumps over the lazy dog. " +
    "This sentence contains every letter of the alphabet and is commonly " +
    "used for typography testing.'");
result.PrintResult();
