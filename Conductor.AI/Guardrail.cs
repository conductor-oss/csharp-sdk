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
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Conductor.AI;

// ── GuardrailAttribute ─────────────────────────────────────

/// <summary>Mark a method as an Agentspan guardrail worker.</summary>
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
        var patternList = patterns.ToList();
        var compiled = patternList.Select(p => new Regex(p, RegexOptions.Compiled)).ToList();
        var guardrailName = name ?? "regex_guardrail";

        var def = new GuardrailDef
        {
            Name = guardrailName,
            Position = position,
            OnFail = onFail,
            MaxRetries = maxRetries,
            GuardrailType = "regex",
            Patterns = patternList,
            Mode = mode,
            Message = message,
            Handler = content =>
            {
                bool matched = compiled.Any(rx => rx.IsMatch(content));

                if (mode == "block" && matched)
                {
                    var msg = message ?? "Content matched a blocked pattern.";
                    return Task.FromResult(new GuardrailResult(false, msg));
                }
                if (mode == "allow" && !matched)
                {
                    var msg = message ?? "Content did not match any allowed pattern.";
                    return Task.FromResult(new GuardrailResult(false, msg));
                }
                return Task.FromResult(new GuardrailResult(true));
            },
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
/// Requires OPENAI_API_KEY in the environment (or set the apiKey parameter).
/// </summary>
public static class LLMGuardrail
{
    private static readonly System.Net.Http.HttpClient _http = new();

    public static GuardrailDef Create(
        string model,
        string policy,
        string? name = null,
        int? maxTokens = null,
        Position position = Position.Output,
        OnFail onFail = OnFail.Raise,
        int maxRetries = 3,
        string? apiKey = null)
    {
        var guardrailName = name ?? "llm_guardrail";

        var def = new GuardrailDef
        {
            Name = guardrailName,
            Position = position,
            OnFail = onFail,
            MaxRetries = maxRetries,
            GuardrailType = "llm",
            Model = model,
            Policy = policy,
            MaxTokens = maxTokens,
            Handler = async content =>
            {
                try
                {
                    var key = apiKey ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "";
                    var prompt =
                        "You are a content safety evaluator. Evaluate the following content against this policy:\n\n" +
                        $"POLICY: {policy}\n\n" +
                        $"CONTENT: {content}\n\n" +
                        "Respond with ONLY a JSON object: {\"passed\": true/false, \"reason\": \"...\"}";

                    // Parse provider/model format
                    var modelName = model.Contains('/') ? model.Split('/', 2)[1] : model;
                    var provider = model.Contains('/') ? model.Split('/', 2)[0] : "openai";

                    string apiUrl = provider switch
                    {
                        "anthropic" => "https://api.anthropic.com/v1/messages",
                        _ => "https://api.openai.com/v1/chat/completions",
                    };

                    var requestBody = new
                    {
                        model = modelName,
                        messages = new[] { new { role = "user", content = prompt } },
                        max_tokens = maxTokens ?? 300,
                        temperature = 0,
                    };

                    var json = System.Text.Json.JsonSerializer.Serialize(requestBody);
                    using var req = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, apiUrl);
                    req.Headers.Add("Authorization", $"Bearer {key}");
                    req.Content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    using var resp = await _http.SendAsync(req);
                    var body = await resp.Content.ReadAsStringAsync();
                    var node = System.Text.Json.Nodes.JsonNode.Parse(body);
                    var text = node?["choices"]?[0]?["message"]?["content"]?.GetValue<string>() ?? "";

                    // Parse JSON response from LLM
                    try
                    {
                        var resultNode = System.Text.Json.Nodes.JsonNode.Parse(text);
                        var passed = resultNode?["passed"]?.GetValue<bool>() ?? false;
                        var reason = resultNode?["reason"]?.GetValue<string>() ?? "";
                        return new GuardrailResult(passed, reason);
                    }
                    catch
                    {
                        // If LLM didn't return valid JSON, be conservative and fail
                        return new GuardrailResult(false, $"LLM guardrail returned unparseable response: {text[..Math.Min(200, text.Length)]}");
                    }
                }
                catch (Exception ex)
                {
                    return new GuardrailResult(false, $"LLM guardrail evaluation error: {ex.Message}");
                }
            },
        };
        GuardrailDef.Validate(def);
        return def;
    }
}
