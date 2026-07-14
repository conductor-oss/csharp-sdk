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
// Sk16 — Data analyst plugin operating on a fixed sample dataset.
//
// Numbers are kept in the plugin instance so the LLM doesn't have to pass
// them on every call. Tools return summary statistics; the agent stitches
// them into a report.
//
// Requirements:
//   - AGENTSPAN_SERVER_URL=http://localhost:8080/api
//   - AGENTSPAN_LLM_MODEL=openai/gpt-4o-mini

using System.ComponentModel;
using Conductor.AI;
using Conductor.AI.Examples;
using Conductor.AI.SemanticKernel;
using Microsoft.SemanticKernel;

namespace Conductor.AI.Examples.Sk16;

public sealed class DataAnalystPlugin
{
    private readonly double[] _data = { 12, 7, 15, 22, 9, 18, 5, 14, 11, 30 };

    [KernelFunction, Description("Count of observations in the loaded dataset.")]
    public int Count() => _data.Length;

    [KernelFunction, Description("Arithmetic mean of the loaded dataset.")]
    public double Mean() => _data.Average();

    [KernelFunction, Description("Population standard deviation of the loaded dataset.")]
    public double Stddev()
    {
        var m = _data.Average();
        return Math.Sqrt(_data.Select(v => (v - m) * (v - m)).Average());
    }

    [KernelFunction, Description("Minimum and maximum, comma-separated, e.g. '5,30'.")]
    public string Range() => $"{_data.Min()},{_data.Max()}";
}

public static class Program
{
    public static async Task Main()
    {
        var agent = SemanticKernelAgent.From(
            name: "sk_data_analyst",
            model: Settings.LlmModel,
            instructions: "Use the tools to describe the dataset with count, mean, stddev, and range.",
            new DataAnalystPlugin());

        await using var runtime = new AgentRuntime(new AgentRuntimeOptions { ServerUrl = Settings.ServerUrl });
        var result = await runtime.RunAsync(
            agent,
            "Summarize the loaded dataset.");
        result.PrintResult();
    }
}
