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
// Simple Agent Guardrails — output validation on a no-tool agent.
//
// Demonstrates mixed guardrail types:
//   - RegexGuardrail: block bullet-point lists (server-side)
//   - Custom guardrail via [Guardrail] attribute: enforce minimum length (worker)
//
// Requirements:
//   - Agentspan server with LLM support
//   - AGENTSPAN_SERVER_URL=http://localhost:6767/api in environment
//   - AGENTSPAN_LLM_MODEL set in environment

using Conductor.AI;
using Conductor.AI.Examples;

// ── RegexGuardrail: block bullet-point lists ──────────────────────────

var noBulletLists = RegexGuardrail.Create(
    patterns: [@"^\s*[-*]\s", @"^\s*\d+\.\s"],
    mode: "block",
    name: "no_lists",
    message:
        "Do not use bullet points or numbered lists. " +
        "Write in flowing prose paragraphs instead.",
    position: Position.Output,
    onFail: OnFail.Retry,
    maxRetries: 3);

// ── Custom guardrail: minimum 50 words ───────────────────────────────

var minLengthGuard = GuardrailRegistry.FromInstance(new LengthGuardrailWorker());

// ── Agent (no tools) ─────────────────────────────────────────────────

var agent = new Agent("essay_writer")
{
    Model = Settings.LlmModel,
    Instructions =
        "You are a concise essay writer. Answer the user's question in " +
        "well-structured prose paragraphs. Do NOT use bullet points or " +
        "numbered lists.",
    Guardrails = [noBulletLists, .. minLengthGuard],
};

// ── Run ───────────────────────────────────────────────────────────────

await using var runtime = new AgentRuntime();
var result = await runtime.RunAsync(agent, "Explain why the sky is blue.");
result.PrintResult();

// Verify guardrails
var output = result.Output?.GetValueOrDefault("result")?.ToString() ?? "";
bool hasBullets = output.Split('\n').Any(line =>
{
    var t = line.TrimStart();
    return t.StartsWith("- ") || t.StartsWith("* ") || (t.Length > 1 && char.IsDigit(t[0]) && t[1] == '.');
});
int wordCount = output.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

if (hasBullets) Console.WriteLine("[WARN] Output contains bullet points — guardrail may not have fired");
else if (wordCount < 50) Console.WriteLine($"[WARN] Output too short ({wordCount} words)");
else Console.WriteLine($"[OK] Prose response, {wordCount} words — guardrails passed");

// ── Guardrail worker ──────────────────────────────────────────────────

internal sealed class LengthGuardrailWorker
{
    [Guardrail("min_length", OnFail = OnFail.Retry, MaxRetries = 3)]
    public GuardrailResult CheckMinLength(string content)
    {
        int wordCount = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        if (wordCount < 50)
            return new GuardrailResult(false,
                $"Response is too short ({wordCount} words). " +
                "Please provide a more detailed answer with at least 50 words.");
        return new GuardrailResult(true);
    }
}
