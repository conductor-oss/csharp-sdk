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
using System.Text.Json.Nodes;

namespace Conductor.AI;

/// <summary>
/// Per-invocation LLM overrides applied on top of an <see cref="Agent"/>'s own
/// settings. Pass to <c>run</c>/<c>start</c>/<c>stream</c> (and their client-level
/// conveniences). Only non-null fields override the agent; everything else is
/// left as the agent defined it. Overrides mutate the serialized <b>root</b>
/// agent config before <c>start</c>, so they flow into the LLM tasks without a
/// new server field — sub-agents keep their own settings (no cascade). There is
/// no <c>TopP</c> — it does not exist in the agentConfig wire contract.
/// </summary>
/// <example>
/// <code>
/// var result = await runtime.RunAsync(agent, "Summarize this",
///     runSettings: new RunSettings(Model: "openai/gpt-4o", Temperature: 0.2, MaxTokens: 2048));
/// </code>
/// </example>
public sealed record RunSettings(
    string? Model = null,
    double? Temperature = null,
    int? MaxTokens = null,
    string? ReasoningEffort = null,
    int? ThinkingBudgetTokens = null)
{
    /// <summary>
    /// Apply the set fields onto the start payload's root agent config
    /// (<c>agentConfig</c>, or <c>rawConfig</c> for framework shape-adapter
    /// agents) — a <c>!= null</c> gate, not truthiness, so <c>Temperature = 0.0</c>
    /// and <c>MaxTokens = 0</c> are honored.
    /// </summary>
    internal void ApplyToPayload(JsonObject payload)
    {
        var key = payload.ContainsKey("agentConfig") ? "agentConfig" : "rawConfig";
        if (payload[key] is not JsonObject root) return;

        if (Model is not null) root["model"] = Model;
        if (Temperature is not null) root["temperature"] = Temperature.Value;
        if (MaxTokens is not null) root["maxTokens"] = MaxTokens.Value;
        if (ReasoningEffort is not null) root["reasoningEffort"] = ReasoningEffort;
        if (ThinkingBudgetTokens is not null)
        {
            root["thinkingConfig"] = new JsonObject
            {
                ["enabled"] = true,
                ["budgetTokens"] = ThinkingBudgetTokens.Value,
            };
        }
    }
}
