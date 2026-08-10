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
using System.Text.Json;

namespace Conductor.AI;

/// <summary>
/// Combined handler for a scope's local (custom) guardrails — one worker per agent/tool,
/// evaluates in declaration order, first failure wins. Mirrors Java's
/// <c>GuardrailHandlerFactory</c>.
/// </summary>
internal static class GuardrailHandlerFactory
{
    /// <summary>
    /// Handler exceptions propagate to the worker runner (failed task), matching Java —
    /// not converted into a guardrail result.
    /// </summary>
    public static Func<Dictionary<string, JsonElement>, ToolContext?, Task<object?>> Create(
        IReadOnlyList<GuardrailDef> guardrails)
    {
        var local = guardrails.ToList();
        return async (args, _ctx) =>
        {
            string content = args.TryGetValue("content", out var contentEl)
                ? (contentEl.ValueKind == JsonValueKind.String
                    ? contentEl.GetString() ?? ""
                    : contentEl.GetRawText())
                : "";

            int iteration = args.TryGetValue("iteration", out var iterEl) &&
                            iterEl.ValueKind == JsonValueKind.Number
                ? iterEl.GetInt32()
                : 0;

            foreach (var guardrail in local)
            {
                var result = await guardrail.Handler!(content);

                if (!result.Passed)
                {
                    var effectiveOnFail = guardrail.OnFail;
                    if (effectiveOnFail == OnFail.Retry && iteration >= guardrail.MaxRetries)
                        effectiveOnFail = OnFail.Raise;
                    if (effectiveOnFail == OnFail.Fix && result.FixedOutput is null)
                        effectiveOnFail = OnFail.Raise;

                    return (object)new Dictionary<string, object?>
                    {
                        ["passed"] = false,
                        ["message"] = result.Message ?? "",
                        ["on_fail"] = effectiveOnFail.ToString().ToLowerInvariant(),
                        ["fixed_output"] = result.FixedOutput,
                        ["guardrail_name"] = guardrail.Name,
                        ["should_continue"] = effectiveOnFail == OnFail.Retry,
                    };
                }
            }

            return (object)new Dictionary<string, object?>
            {
                ["passed"] = true,
                ["message"] = "",
                ["on_fail"] = "pass",
                ["fixed_output"] = null,
                ["guardrail_name"] = "",
                ["should_continue"] = false,
            };
        };
    }
}
