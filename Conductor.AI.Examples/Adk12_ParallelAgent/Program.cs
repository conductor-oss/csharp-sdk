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
// Adk12 — Parallel Agent.
//
// Python's ParallelAgent runs sub-agents concurrently and aggregates
// results. The coordinator delegates to multiple analysts in parallel.
//
// Requirements:
//   - AGENTSPAN_SERVER_URL=http://localhost:6767/api
//   - AGENTSPAN_LLM_MODEL=openai/gpt-4o-mini

using Conductor.AI;
using Conductor.AI.Examples;
using Conductor.AI.GoogleADK;

var marketAnalyst = GoogleADKAgent.Builder()
    .Name("market_analyst")
    .Model(Settings.LlmModel)
    .Instruction(
        "You are a market analyst. Given the company or product topic, " +
        "provide a brief 2-3 sentence market analysis. Focus on trends and competition.")
    .Build();

var techAnalyst = GoogleADKAgent.Builder()
    .Name("tech_analyst")
    .Model(Settings.LlmModel)
    .Instruction(
        "You are a technology analyst. Given the company or product topic, " +
        "provide a brief 2-3 sentence technical evaluation. Focus on innovation and capabilities.")
    .Build();

var riskAnalyst = GoogleADKAgent.Builder()
    .Name("risk_analyst")
    .Model(Settings.LlmModel)
    .Instruction(
        "You are a risk analyst. Given the company or product topic, " +
        "provide a brief 2-3 sentence risk assessment. Focus on potential challenges.")
    .Build();

var parallelAnalysis = GoogleADKAgent.Builder()
    .Name("parallel_analysis")
    .Model(Settings.LlmModel)
    .Instruction(
        "You orchestrate three parallel analysts: market_analyst, tech_analyst, " +
        "and risk_analyst. Dispatch the user's topic to all three concurrently, " +
        "then aggregate their findings into a combined report.")
    .SubAgents(marketAnalyst, techAnalyst, riskAnalyst)
    .Build();

await using var runtime = new AgentRuntime(new AgentRuntimeOptions { ServerUrl = Settings.ServerUrl });
var result = await runtime.RunAsync(parallelAnalysis, "Analyze Tesla's electric vehicle business");
Console.WriteLine($"Status: {result.Status}");
result.PrintResult();
