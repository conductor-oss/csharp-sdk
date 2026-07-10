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
// Single-Turn Tool Call — LLM calls a tool and answers in one shot.
//
// The simplest tool-calling pattern: the user asks a question, the LLM
// calls a tool to get data, then responds with the answer. No iterative
// loop — the agent runs for exactly one exchange.
//
// Compiled workflow:
//
//     LLM(prompt, tools) → tool executes → LLM sees result → answer
//
// Requirements:
//   - AGENTSPAN_SERVER_URL=http://localhost:6767/api in environment
//   - AGENTSPAN_LLM_MODEL set in environment

using Conductor.AI;
using Conductor.AI.Examples;

var agent = new Agent("weather_agent_33b")
{
    Model = Settings.LlmModel,
    Instructions = "You are a weather assistant. Use the get_weather tool to answer.",
    Tools = ToolRegistry.FromInstance(new WeatherTools()),
    MaxTurns = 2,  // 1 turn to call the tool, 1 turn to answer
};

await using var runtime = new AgentRuntime();
var result = await runtime.RunAsync(agent, "What's the weather in San Francisco?");
result.PrintResult();

// ── Tool class ─────────────────────────────────────────────────

internal sealed class WeatherTools
{
    [Tool("Get the current weather for a city.")]
    public Dictionary<string, object> GetWeather(string city)
        => new() { ["city"] = city, ["temp_f"] = 72, ["condition"] = "Sunny" };
}
