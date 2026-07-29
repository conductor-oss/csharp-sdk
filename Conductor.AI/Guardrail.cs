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
using System.Reflection;

namespace Conductor.AI;

// ── GuardrailAttribute ─────────────────────────────────────

/// <summary>Mark a method as an Conductor guardrail worker.</summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class GuardrailAttribute : Attribute
{
    public string? Name { get; set; }
    public Position Position { get; set; } = Position.Output;
    public OnFail OnFail { get; set; } = OnFail.Raise;
    public int MaxRetries { get; set; } = 3;

    public GuardrailAttribute() { }
    public GuardrailAttribute(string name) { Name = name; }
}

// ── GuardrailDef ────────────────────────────────────────────

/// <summary>A compiled guardrail — name, position, on_fail, and the backing handler.</summary>
public sealed class GuardrailDef
{
    public string Name { get; init; } = "";
    public Position Position { get; init; } = Position.Output;
    public OnFail OnFail { get; init; } = OnFail.Raise;
    public int MaxRetries { get; init; } = 3;

    /// <summary>
    /// Wire <c>guardrailType</c> — "custom" (default), "regex", "llm", or "external".
    /// Mirrors Python's <c>_serialize_guardrail</c>.
    /// </summary>
    internal string GuardrailType { get; init; } = "custom";

    /// <summary>
    /// <c>true</c> if this guardrail references an external worker running
    /// elsewhere (no local handler). Mirrors Python <c>Guardrail.external</c>.
    /// </summary>
    public bool External => Handler is null && GuardrailType == "external";

    // Handler receives the content string and returns a GuardrailResult.
    internal Func<string, Task<GuardrailResult>>? Handler { get; init; }

    // ── Server-evaluated guardrail data (guardrailType "regex") ──
    public IReadOnlyList<string>? Patterns { get; init; }
    public string? Mode { get; init; }
    public string? Message { get; init; }

    // ── Server-evaluated guardrail data (guardrailType "llm") ──
    public string? Model { get; init; }
    public string? Policy { get; init; }
    public int? MaxTokens { get; init; }

    /// <summary>
    /// Non-blank name, non-negative maxRetries, <c>on_fail=human</c> only for output
    /// position (input runs client-side, can't pause a workflow), type-specific data
    /// for regex/llm/custom. Mirrors Java's <c>GuardrailDef.Builder.build()</c>.
    /// </summary>
    internal static void Validate(GuardrailDef def)
    {
        if (string.IsNullOrWhiteSpace(def.Name))
            throw new ArgumentException("GuardrailDef requires a non-blank name.");
        if (def.MaxRetries < 0)
            throw new ArgumentException($"GuardrailDef '{def.Name}': maxRetries must be non-negative.");
        if (def.OnFail == OnFail.Human && def.Position == Position.Input)
            throw new ArgumentException(
                "on_fail='human' is only valid for position='output' " +
                "(input guardrails are client-side and cannot pause a workflow)");

        switch (def.GuardrailType)
        {
            case "regex":
                if (def.Patterns is null || def.Patterns.Count == 0)
                    throw new ArgumentException(
                        $"GuardrailDef '{def.Name}': regex guardrails require at least one pattern.");
                if (def.Mode != "block" && def.Mode != "allow")
                    throw new ArgumentException(
                        $"GuardrailDef '{def.Name}': mode must be 'block' or 'allow', got '{def.Mode}'.");
                break;
            case "llm":
                if (string.IsNullOrWhiteSpace(def.Model))
                    throw new ArgumentException($"GuardrailDef '{def.Name}': llm guardrails require a model.");
                if (string.IsNullOrWhiteSpace(def.Policy))
                    throw new ArgumentException($"GuardrailDef '{def.Name}': llm guardrails require a policy.");
                break;
            case "custom":
                if (def.Handler is null)
                    throw new ArgumentException(
                        $"GuardrailDef '{def.Name}': custom guardrails require a handler. " +
                        "Use Guardrail.External(...) for a guardrail backed by a remote worker.");
                break;
        }
    }
}

// ── Guardrail (external reference factory) ─────────────────

/// <summary>
/// Factory for referenced-by-name (external) guardrails — a guardrail that runs
/// as a Conductor worker task elsewhere, with no local handler. Mirrors Python
/// <c>Guardrail(name=...)</c> and TS <c>guardrail.external()</c>.
/// </summary>
public static class Guardrail
{
    /// <summary>
    /// Create an external guardrail definition (no local handler). The task is
    /// dispatched by name to a remote worker. Emits <c>guardrailType:"external"</c>.
    /// </summary>
    /// <param name="name">The Conductor task name of the external guardrail worker.</param>
    /// <param name="position">Where the guardrail runs — input or output (default output).</param>
    /// <param name="onFail">What to do when the guardrail fails (default raise).</param>
    /// <param name="maxRetries">Max retry attempts for on_fail=retry (default 3).</param>
    public static GuardrailDef External(
        string name,
        Position position = Position.Output,
        OnFail onFail = OnFail.Raise,
        int maxRetries = 3)
    {
        var def = new GuardrailDef
        {
            Name = name,
            Position = position,
            OnFail = onFail,
            MaxRetries = maxRetries,
            GuardrailType = "external",
            Handler = null,
        };
        GuardrailDef.Validate(def);
        return def;
    }
}

// ── GuardrailRegistry ──────────────────────────────────────

/// <summary>Build <see cref="GuardrailDef"/> instances from class instances using reflection.</summary>
public static class GuardrailRegistry
{
    public static List<GuardrailDef> FromInstance(object instance)
    {
        var type = instance.GetType();
        var defs = new List<GuardrailDef>();

        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
        {
            var attr = method.GetCustomAttribute<GuardrailAttribute>();
            if (attr is null) continue;

            var name = attr.Name ?? ToolRegistry.ToSnakeCase(method.Name);
            var def = new GuardrailDef
            {
                Name = name,
                Position = attr.Position,
                OnFail = attr.OnFail,
                MaxRetries = attr.MaxRetries,
                Handler = BuildHandler(instance, method),
            };
            GuardrailDef.Validate(def);
            defs.Add(def);
        }
        return defs;
    }

    private static Func<string, Task<GuardrailResult>> BuildHandler(object instance, MethodInfo method)
    {
        return async (content) =>
        {
            var parameters = method.GetParameters();
            var args = new object?[parameters.Length];
            if (parameters.Length > 0) args[0] = content;

            var result = method.Invoke(instance, args);
            if (result is Task<GuardrailResult> taskResult) return await taskResult;
            if (result is GuardrailResult gr) return gr;
            return new GuardrailResult(true);
        };
    }
}

// ── RegexGuardrail ─────────────────────────────────────────

/// <summary>
/// A guardrail that validates content against regex patterns.
/// Block mode (default): fails if any pattern matches.
/// Allow mode: fails if NO pattern matches.
///
/// <para>Serialized as <c>guardrailType: "regex"</c> — the Conductor server evaluates the
/// patterns (ECMAScript/GraalJS dialect). No worker process is needed.</para>
/// </summary>
public static class RegexGuardrail
{
    public static GuardrailDef Create(
        IEnumerable<string> patterns,
        string mode = "block",
        string? name = null,
        string? message = null,
        Position position = Position.Output,
        OnFail onFail = OnFail.Raise,
        int maxRetries = 3)
    {
        var def = new GuardrailDef
        {
            Name = name ?? "regex_guardrail",
            Position = position,
            OnFail = onFail,
            MaxRetries = maxRetries,
            GuardrailType = "regex",
            Patterns = patterns.ToList(),
            Mode = mode,
            Message = message,
        };
        GuardrailDef.Validate(def);
        return def;
    }

    /// <summary>Convenience overload accepting a single pattern string.</summary>
    public static GuardrailDef Create(
        string pattern,
        string mode = "block",
        string? name = null,
        string? message = null,
        Position position = Position.Output,
        OnFail onFail = OnFail.Raise,
        int maxRetries = 3)
        => Create([pattern], mode, name, message, position, onFail, maxRetries);
}

// ── LLMGuardrail ───────────────────────────────────────────

/// <summary>
/// A guardrail that uses an LLM to evaluate content against a safety policy.
/// The model receives the policy + content and must respond with JSON:
///   {"passed": true/false, "reason": "..."}
///
/// <para>Serialized as <c>guardrailType: "llm"</c> — the Conductor server calls the
/// specified model via its own configured LLM providers. No worker process, HTTP client,
/// or API key is needed on the client side.</para>
/// </summary>
public static class LLMGuardrail
{
    public static GuardrailDef Create(
        string model,
        string policy,
        string? name = null,
        int? maxTokens = null,
        Position position = Position.Output,
        OnFail onFail = OnFail.Raise,
        int maxRetries = 3)
    {
        var def = new GuardrailDef
        {
            Name = name ?? "llm_guardrail",
            Position = position,
            OnFail = onFail,
            MaxRetries = maxRetries,
            GuardrailType = "llm",
            Model = model,
            Policy = policy,
            MaxTokens = maxTokens,
        };
        GuardrailDef.Validate(def);
        return def;
    }
}
