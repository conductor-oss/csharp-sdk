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
// Sk10 — Pass a KernelPlugin instance instead of a plain object.
//
// Same plugin class as Sk01, but wrapped via KernelPluginFactory.CreateFromObject
// before being handed to SemanticKernelAgent.From. The bridge handles both
// shapes identically.
//
// Requirements:
//   - AGENTSPAN_SERVER_URL=http://localhost:8080/api
//   - AGENTSPAN_LLM_MODEL=openai/gpt-4o-mini

using System.ComponentModel;
using Conductor.AI;
using Conductor.AI.Examples;
using Conductor.AI.SemanticKernel;
using Microsoft.SemanticKernel;

namespace Conductor.AI.Examples.Sk10;

public sealed class CalculatorPlugin
{
    [KernelFunction, Description("Add two integers.")]
    public int Add(int a, int b) => a + b;

    [KernelFunction, Description("Subtract b from a.")]
    public int Subtract(int a, int b) => a - b;
}

public static class Program
{
    public static async Task Main()
    {
        KernelPlugin plugin = KernelPluginFactory.CreateFromObject(new CalculatorPlugin(), "calc");

        var agent = SemanticKernelAgent.From(
            name: "sk_kernelplugin",
            model: Settings.LlmModel,
            instructions: "Solve arithmetic using the calc plugin.",
            plugin);

        await using var runtime = new AgentRuntime(new AgentRuntimeOptions { ServerUrl = Settings.ServerUrl });
        var result = await runtime.RunAsync(agent, "Compute (12 + 30) - 7.");
        result.PrintResult();
    }
}
