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
// Sk02 — ReAct loop with multiple [KernelFunction] tools.
//
// One plain class exposes three tools. The agent picks the right one(s)
// in sequence to answer a compound question.
//
// Requirements:
//   - AGENTSPAN_SERVER_URL=http://localhost:8080/api
//   - AGENTSPAN_LLM_MODEL=openai/gpt-4o-mini

using System.ComponentModel;
using System.Globalization;
using Conductor.AI;
using Conductor.AI.Examples;
using Conductor.AI.SemanticKernel;
using Microsoft.SemanticKernel;

namespace Conductor.AI.Examples.Sk02;

public sealed class UtilityPlugin
{
    [KernelFunction, Description("Compute the length of a string.")]
    public int StringLength([Description("input text")] string text) => text.Length;

    [KernelFunction, Description("Reverse a string.")]
    public string Reverse([Description("input text")] string text)
        => new string(text.Reverse().ToArray());

    [KernelFunction, Description("Today's date in ISO format (yyyy-MM-dd) in UTC.")]
    public string Today() => DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}

public static class Program
{
    public static async Task Main()
    {
        var agent = SemanticKernelAgent.From(
            name: "sk_react",
            model: Settings.LlmModel,
            instructions: "Use the tools when relevant. Be concise.",
            new UtilityPlugin());

        await using var runtime = new AgentRuntime(new AgentRuntimeOptions { ServerUrl = Settings.ServerUrl });
        var result = await runtime.RunAsync(
            agent,
            "What is the length of the string 'agentspan', what is it reversed, and what is today's date?");
        result.PrintResult();
    }
}
