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
using System.ComponentModel;
using Conductor.AI;
using Conductor.AI.SemanticKernel;
using Microsoft.SemanticKernel;

namespace Conductor.AI.Examples.Sk01;

/// <summary>
/// Bridge a plain C# class with [KernelFunction] methods into Conductor.
///
/// No Kernel setup, no SK ChatClient — Conductor uses the LLM, SK only contributes
/// the function metadata + invocation glue.
/// </summary>
public sealed class CalculatorPlugin
{
    [KernelFunction, Description("Add two integers and return their sum.")]
    public int Add(
        [Description("first number")] int a,
        [Description("second number")] int b) => a + b;

    [KernelFunction, Description("Multiply two integers.")]
    public int Multiply(int a, int b) => a * b;
}

public static class Program
{
    public static async Task Main()
    {
        var agent = SemanticKernelAgent.From(
            name: "sk_calc_agent",
            model: Settings.LlmModel,
            instructions: "You are a calculator. Use the tools to answer math questions.",
            new CalculatorPlugin());

        await using var runtime = new AgentRuntime(new AgentRuntimeOptions { ServerUrl = Settings.ServerUrl });
        var result = await runtime.RunAsync(agent, "What is 7 + 8, and then multiply that by 3?");

        result.PrintResult();
    }
}
