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
// OpenAi03 — Structured Output.
//
// Forces an OpenAI Agents SDK agent to return a typed JSON object via
// the .OutputType("MovieList") hook. Server-side, the normalizer pins
// the LLM to that schema.
//
// Note: simplified from Java original — temperature/max_tokens not
// surfaced on the OpenAIAgent builder yet (Python parity gap, same as Java).
//
// Requirements:
//   - AGENTSPAN_SERVER_URL=http://localhost:8080/api
//   - AGENTSPAN_LLM_MODEL=openai/gpt-4o-mini

using Conductor.AI;
using Conductor.AI.Examples;
using Conductor.AI.OpenAI;

var agent = OpenAIAgent.Builder()
    .Name("movie_recommender")
    .Instructions(
        "You are a movie recommendation expert. When asked for movie suggestions, " +
        "return a structured list of recommendations with title, year, genre, " +
        "and a brief reason for each recommendation. Identify the overall theme.")
    .Model(Settings.LlmModel)
    .OutputType("MovieList")
    .Build();

await using var runtime = new AgentRuntime(new AgentRuntimeOptions { ServerUrl = Settings.ServerUrl });
var result = await runtime.RunAsync(
    agent,
    "Recommend 3 sci-fi movies that explore the concept of artificial intelligence.");
result.PrintResult();
