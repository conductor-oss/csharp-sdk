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
// Sk05 — Multi-turn conversation with an SK-bridged agent.
//
// Two sequential RunAsync calls show that each turn is independent unless
// the prior context is folded into the prompt. The second turn explicitly
// references the first answer.
//
// Requirements:
//   - CONDUCTOR_SERVER_URL=http://localhost:8080/api
//   - CONDUCTOR_AGENT_LLM_MODEL=openai/gpt-4o-mini

using System.ComponentModel;
using Conductor.AI;
using Conductor.AI.Examples;
using Conductor.AI.SemanticKernel;
using Microsoft.SemanticKernel;

namespace Conductor.AI.Examples.Sk05;

public sealed class GeoPlugin
{
    [KernelFunction, Description("Return the capital of a country.")]
    public string Capital([Description("country name")] string country) =>
        country.ToLowerInvariant() switch
        {
            "france" => "Paris",
            "japan" => "Tokyo",
            "australia" => "Canberra",
            "united states" or "usa" or "us" => "Washington, D.C.",
            _ => "unknown",
        };
}

public static class Program
{
    public static async Task Main()
    {
        var agent = SemanticKernelAgent.From(
            name: "sk_geo",
            model: Settings.LlmModel,
            instructions: "Answer geography questions. Use the capital tool when needed.",
            new GeoPlugin());

        await using var runtime = new AgentRuntime(new AgentRuntimeOptions { ServerUrl = Settings.ServerUrl });

        Console.WriteLine("── Turn 1 ──");
        var first = await runtime.RunAsync(agent, "What is the capital of Japan?");
        first.PrintResult();

        Console.WriteLine("\n── Turn 2 (history folded into prompt) ──");
        var priorAnswer = first.Output?.TryGetValue("result", out var v) == true ? v?.ToString() : "(unknown)";
        var followUp = $"Previously you told me the capital of Japan was '{priorAnswer}'. " +
                       "What is the capital of France, and is it further from Tokyo or from London?";
        var second = await runtime.RunAsync(agent, followUp);
        second.PrintResult();
    }
}
