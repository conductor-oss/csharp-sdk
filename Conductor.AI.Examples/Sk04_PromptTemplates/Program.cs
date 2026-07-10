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
// Sk04 — Prompt templating exposed as a [KernelFunction].
//
// SK is often used to template prompts. Here a plugin function takes
// variables, builds a structured prompt fragment, and the LLM consumes
// the result to produce the final user-facing answer.
//
// Requirements:
//   - AGENTSPAN_SERVER_URL=http://localhost:6767/api
//   - AGENTSPAN_LLM_MODEL=openai/gpt-4o-mini

using System.ComponentModel;
using Conductor.AI;
using Conductor.AI.Examples;
using Conductor.AI.SemanticKernel;
using Microsoft.SemanticKernel;

namespace Conductor.AI.Examples.Sk04;

public sealed class TemplatePlugin
{
    [KernelFunction, Description("Format a product blurb prompt fragment.")]
    public string BuildProductBlurb(
        [Description("product name")] string product,
        [Description("target audience")] string audience,
        [Description("desired tone, e.g. 'playful'")] string tone)
        => $"Write a 2-sentence blurb for product '{product}' aimed at {audience} in a {tone} tone.";
}

public static class Program
{
    public static async Task Main()
    {
        var agent = SemanticKernelAgent.From(
            name: "sk_templater",
            model: Settings.LlmModel,
            instructions: "Call build_product_blurb first to assemble the brief, then write the blurb.",
            new TemplatePlugin());

        await using var runtime = new AgentRuntime(new AgentRuntimeOptions { ServerUrl = Settings.ServerUrl });
        var result = await runtime.RunAsync(
            agent,
            "Write a blurb for product 'NoteSpark' for high school students with a playful tone.");
        result.PrintResult();
    }
}
