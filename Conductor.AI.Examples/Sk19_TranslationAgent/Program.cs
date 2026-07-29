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
// Sk19 — Translation agent with a stub dictionary backend.
//
// detect_language returns a code from a few key markers; translate
// maps simple English phrases to French/Spanish. The agent picks the
// right source/target via the tools rather than guessing.
//
// Requirements:
//   - CONDUCTOR_SERVER_URL=http://localhost:8080/api
//   - CONDUCTOR_AGENT_LLM_MODEL=openai/gpt-4o-mini

using System.ComponentModel;
using Conductor.AI;
using Conductor.AI.Examples;
using Conductor.AI.SemanticKernel;
using Microsoft.SemanticKernel;

namespace Conductor.AI.Examples.Sk19;

public sealed class TranslatePlugin
{
    [KernelFunction, Description("Detect the language of a phrase. Returns ISO code: en, fr, es, or 'unknown'.")]
    public string DetectLanguage([Description("input phrase")] string text)
    {
        var t = text.ToLowerInvariant();
        if (t.Contains("bonjour") || t.Contains("merci")) return "fr";
        if (t.Contains("hola") || t.Contains("gracias")) return "es";
        if (t.Contains("hello") || t.Contains("thank")) return "en";
        return "unknown";
    }

    [KernelFunction, Description("Translate a short English phrase to French or Spanish using a tiny lookup table.")]
    public string Translate(
        [Description("English phrase, lowercase")] string phrase,
        [Description("target language code: 'fr' or 'es'")] string target)
    {
        var key = phrase.Trim().ToLowerInvariant();
        var dict = target switch
        {
            "fr" => new Dictionary<string, string>
            {
                ["hello"] = "bonjour",
                ["thank you"] = "merci",
                ["good morning"] = "bonjour",
                ["goodbye"] = "au revoir",
            },
            "es" => new Dictionary<string, string>
            {
                ["hello"] = "hola",
                ["thank you"] = "gracias",
                ["good morning"] = "buenos días",
                ["goodbye"] = "adiós",
            },
            _ => new Dictionary<string, string>(),
        };
        return dict.TryGetValue(key, out var v) ? v : "NOT_FOUND";
    }
}

public static class Program
{
    public static async Task Main()
    {
        var agent = SemanticKernelAgent.From(
            name: "sk_translate",
            model: Settings.LlmModel,
            instructions: "Detect the source language first, then translate the supplied phrase to the requested target.",
            new TranslatePlugin());

        await using var runtime = new AgentRuntime(new AgentRuntimeOptions { ServerUrl = Settings.ServerUrl });
        var result = await runtime.RunAsync(
            agent,
            "Translate 'thank you' to French and 'goodbye' to Spanish.");
        result.PrintResult();
    }
}
