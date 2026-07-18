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
// OpenAi02 — Function Tools.
//
// Demonstrates an OpenAI Agents SDK-style agent that calls multiple
// function-tools (weather, calculator, population lookup). Each tool
// method carries the Agentspan [Tool] attribute — the OpenAIAgent
// bridge reflects them into Agentspan worker tools.
//
// Requirements:
//   - AGENTSPAN_SERVER_URL=http://localhost:8080/api
//   - AGENTSPAN_LLM_MODEL=openai/gpt-4o-mini

using Conductor.AI;
using Conductor.AI.Examples;
using Conductor.AI.OpenAI;

var agent = OpenAIAgent.Builder()
    .Name("multi_tool_agent")
    .Instructions(
        "You are a helpful assistant with access to weather, calculator, " +
        "and population lookup tools. Use them to answer questions accurately.")
    .Model(Settings.LlmModel)
    .Tools(new WeatherTools())
    .Build();

await using var runtime = new AgentRuntime(new AgentRuntimeOptions { ServerUrl = Settings.ServerUrl });
var result = await runtime.RunAsync(
    agent,
    "What's the weather in San Francisco? Also, what's the population there " +
    "and what's the square root of that number (just the digits)?");
result.PrintResult();

internal sealed class WeatherTools
{
    private static readonly Dictionary<string, string> _weather = new(StringComparer.OrdinalIgnoreCase)
    {
        ["new york"] = "72F, Partly Cloudy",
        ["san francisco"] = "58F, Foggy",
        ["miami"] = "85F, Sunny",
        ["london"] = "55F, Rainy",
    };

    private static readonly Dictionary<string, string> _populations = new(StringComparer.OrdinalIgnoreCase)
    {
        ["new york"] = "8.3 million",
        ["san francisco"] = "874,000",
        ["miami"] = "442,000",
        ["london"] = "8.8 million",
    };

    [Tool(Name = "get_weather", Description = "Get the current weather for a city.")]
    public string GetWeather(string city)
        => _weather.TryGetValue(city, out var v) ? v : $"Weather data not available for {city}";

    [Tool(Name = "calculate", Description = "Evaluate a mathematical expression and return the result.")]
    public string Calculate(string expression)
    {
        try
        {
            var trimmed = expression.Trim();
            string[] ops = { "+", "-", "*", "/" };
            foreach (var op in ops)
            {
                var idx = trimmed.IndexOf(op, 1, StringComparison.Ordinal);
                if (idx > 0)
                {
                    var a = double.Parse(trimmed.Substring(0, idx).Trim());
                    var b = double.Parse(trimmed.Substring(idx + 1).Trim());
                    double r = op switch
                    {
                        "+" => a + b,
                        "-" => a - b,
                        "*" => a * b,
                        "/" => a / b,
                        _ => 0,
                    };
                    return r == (long)r ? ((long)r).ToString() : r.ToString();
                }
            }
            if (trimmed.StartsWith("sqrt(") && trimmed.EndsWith(")"))
            {
                var v = double.Parse(trimmed[5..^1]);
                return Math.Sqrt(v).ToString();
            }
            return double.Parse(trimmed).ToString();
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    [Tool(Name = "lookup_population", Description = "Look up the population of a city.")]
    public string LookupPopulation(string city)
        => _populations.TryGetValue(city, out var v) ? v : "Unknown";
}
