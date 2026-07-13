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
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;

namespace Conductor.AI;

/// <summary>
/// OCG (Open Context Graph) backed long-term memory for agents.
///
/// <para>Backs the agentspan <see cref="MemoryStore"/> abstraction with an OCG
/// instance so an agent's memories persist in OCG and ride OCG's feedback-aware
/// ranking. Implements the synchronous <see cref="MemoryStore"/> interface over
/// the OCG BFF:</para>
/// <list type="bullet">
///   <item><c>Add</c>     -> <c>POST   /api/v1/memories</c></item>
///   <item><c>Search</c>  -> <c>POST   /api/v1/memories/search</c> (feedback-blended ranking)</item>
///   <item><c>Delete</c>  -> <c>DELETE /api/v1/memories/{key}</c></item>
///   <item><c>ListAll</c> -> <c>GET    /api/v1/memories</c></item>
/// </list>
///
/// <para>Design notes: the OCG bearer <c>token</c> is held <b>client-side</b> here
/// (e.g. from <c>OCG_TOKEN</c>), unlike the server-resolved retrieval tools. Agents
/// only ever <b>create and read</b> memories — good/bad feedback is human-only and
/// delivered out-of-band via an agent's <see cref="Agent.FeedbackSink"/>; the signed
/// capability URLs are never surfaced to the agent's LLM.</para>
///
/// <para>When an agent's <see cref="SemanticMemory"/> is backed by this store, the
/// serializer emits a <c>longTermMemory</c> config so the server-side compiler
/// inlines retrieval (pre-loop) + distill/save/feedback (post-loop) steps — see
/// <see cref="AgentConfigSerializer"/>. The <c>credential</c> emitted there is a
/// server-resolvable secret NAME (e.g. <c>OCG_PUBLIC_KEY</c>), never the raw token.</para>
/// </summary>
public sealed class OCGMemoryStore : MemoryStore, IDisposable
{
    private readonly HttpClient _client;
    private readonly bool _ownsClient;

    /// <summary>Base URL of the OCG instance (trailing slash stripped).</summary>
    public string BaseUrl { get; }

    /// <summary>Agent owner key, e.g. <c>"agent:support"</c>.</summary>
    public string Agent { get; }

    /// <summary>Optional user owner, e.g. <c>"user:alice"</c>.</summary>
    public string? User { get; }

    /// <summary>
    /// Server-resolvable credential NAME (default <c>"OCG_PUBLIC_KEY"</c>) for the OCG
    /// bearer token. Used by the COMPILED/deployed path — the server resolves this via
    /// a <c>#{NAME}</c> HTTP-header placeholder. Distinct from the raw client token.
    /// </summary>
    public string Credential { get; }

    /// <summary>Memory scope for writes (default <c>"user"</c>).</summary>
    public string Scope { get; }

    /// <param name="url">Base URL of the OCG instance (required).</param>
    /// <param name="agent">Agent owner key, e.g. <c>"agent:support"</c> (required).</param>
    /// <param name="user">Optional user owner, e.g. <c>"user:alice"</c>.</param>
    /// <param name="token">OCG bearer token, held client-side (e.g. from <c>OCG_TOKEN</c>).
    /// Used by the client-side path. Ignored when <paramref name="client"/> is supplied.</param>
    /// <param name="credential">Server-resolvable credential NAME (default <c>"OCG_PUBLIC_KEY"</c>).</param>
    /// <param name="scope">Memory scope for writes (default <c>"user"</c>).</param>
    /// <param name="timeoutSeconds">Per-request timeout in seconds.</param>
    /// <param name="client">Optional pre-built <see cref="HttpClient"/> (mainly for tests).</param>
    public OCGMemoryStore(
        string url,
        string agent,
        string? user = null,
        string? token = null,
        string credential = "OCG_PUBLIC_KEY",
        string scope = "user",
        double timeoutSeconds = 10.0,
        HttpClient? client = null)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("OCGMemoryStore requires a non-blank OCG instance url", nameof(url));
        if (string.IsNullOrWhiteSpace(agent))
            throw new ArgumentException("OCGMemoryStore requires a non-blank agent owner", nameof(agent));

        BaseUrl = url.Trim().TrimEnd('/');
        Agent = agent;
        User = user;
        Credential = credential;
        Scope = scope;

        if (client is not null)
        {
            _client = client;
            _ownsClient = false;
        }
        else
        {
            _client = new HttpClient { Timeout = TimeSpan.FromSeconds(timeoutSeconds) };
            if (!string.IsNullOrEmpty(token))
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _ownsClient = true;
        }
    }

    // ── MemoryStore interface ───────────────────────────────────────────

    /// <inheritdoc />
    public override string Add(MemoryEntry entry)
    {
        var key = FirstNonEmpty(
            entry.Id,
            entry.Metadata.TryGetValue("key", out var k) ? k?.ToString() : null)
            ?? HashKey(entry.Content);

        var tags = new JsonArray();
        if (entry.Metadata.TryGetValue("tags", out var t) && t is not string
            && t is System.Collections.IEnumerable seq)
        {
            foreach (var item in seq) tags.Add(item?.ToString() ?? "");
        }

        var description = entry.Content.Length > 200 ? entry.Content[..200] : entry.Content;
        var body = new JsonObject
        {
            ["key"] = key,
            ["agent"] = Agent,
            ["value"] = entry.Content,
            ["description"] = description,
            ["scope"] = Scope,
            ["source"] = "agent_inferred",
            ["tags"] = tags,
        };
        if (User is not null) body["user"] = User;

        Request(HttpMethod.Post, "/api/v1/memories", body);
        return key;
    }

    /// <inheritdoc />
    public override List<MemoryEntry> Search(string query, int topK = 5)
    {
        var body = new JsonObject
        {
            ["query"] = query,
            ["agent"] = Agent,
            ["limit"] = topK,
            ["include_shared"] = true,
        };
        if (User is not null) body["user"] = User;

        var resp = Request(HttpMethod.Post, "/api/v1/memories/search", body);
        var memories = resp?["memories"]?.AsArray();
        var outList = new List<MemoryEntry>();
        if (memories is null) return outList;

        foreach (var m in memories)
        {
            if (m is null) continue;
            outList.Add(new MemoryEntry
            {
                Id = m["key"]?.GetValue<string>() ?? "",
                Content = WithSignal(m["value_preview"]?.GetValue<string>() ?? "", m),
                Metadata = new Dictionary<string, object>
                {
                    ["relevance_score"] = ReadDouble(m, "relevance_score"),
                    ["good_count"] = ReadInt(m, "good_count"),
                    ["bad_count"] = ReadInt(m, "bad_count"),
                },
            });
        }
        return outList;
    }

    /// <inheritdoc />
    public override bool Delete(string id)
    {
        var path = $"/api/v1/memories/{Uri.EscapeDataString(id)}" + Query(("agent", Agent), ("user", User));
        try
        {
            Request(HttpMethod.Delete, path);
        }
        catch (AgentApiException)
        {
            return false;
        }
        return true;
    }

    /// <inheritdoc />
    public override void Clear()
    {
        // No bulk-clear endpoint — fan out over the listed keys. Guard usage:
        // this deletes every memory for the configured agent/user.
        foreach (var e in ListAll()) Delete(e.Id);
    }

    /// <inheritdoc />
    public override List<MemoryEntry> ListAll()
    {
        var path = "/api/v1/memories" + Query(("agent", Agent), ("limit", "200"), ("user", User));
        var resp = Request(HttpMethod.Get, path);
        var memories = resp?["memories"]?.AsArray();
        var outList = new List<MemoryEntry>();
        if (memories is null) return outList;
        foreach (var m in memories)
        {
            if (m is null) continue;
            outList.Add(new MemoryEntry
            {
                Id = m["key"]?.GetValue<string>() ?? "",
                Content = m["value_preview"]?.GetValue<string>() ?? "",
            });
        }
        return outList;
    }

    // ── Capability feedback links (human-only, out-of-band) ─────────────

    /// <summary>
    /// Mint signed good/bad capability URLs for a memory. The URLs require no OCG
    /// login — a human (e.g. a support engineer) clicks them to vote. Requires the
    /// OCG instance to have a feedback-link secret configured (else OCG returns 501).
    /// </summary>
    public FeedbackLinks GetFeedbackLinks(string key)
    {
        var path = $"/api/v1/memories/{Uri.EscapeDataString(key)}/feedback-links"
                   + Query(("agent", Agent), ("user", User));
        var resp = Request(HttpMethod.Post, path);
        return new FeedbackLinks(
            GoodUrl: resp?["good_url"]?.GetValue<string>(),
            BadUrl: resp?["bad_url"]?.GetValue<string>(),
            ExpiresAt: resp?["expires_at"]?.GetValue<string>());
    }

    // ── HTTP plumbing ───────────────────────────────────────────────────

    private JsonNode? Request(HttpMethod method, string path, JsonNode? body = null)
    {
        var url = BaseUrl + path;
        using var req = new HttpRequestMessage(method, url);
        if (body is not null)
            req.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");

        HttpResponseMessage resp;
        try
        {
            resp = _client.SendAsync(req).GetAwaiter().GetResult();
        }
        catch (HttpRequestException exc) // network/timeout
        {
            throw new AgentApiException(0, exc.Message, url);
        }

        using (resp)
        {
            var text = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            if ((int)resp.StatusCode >= 400)
                throw new AgentApiException((int)resp.StatusCode, text, text);
            return string.IsNullOrEmpty(text) ? null : JsonNode.Parse(text);
        }
    }

    // ── helpers ─────────────────────────────────────────────────────────

    /// <summary>
    /// Fold the human good/bad signal into a search result's content so the injected
    /// prompt context shows the agent when a memory was marked bad and why.
    /// </summary>
    private static string WithSignal(string content, JsonNode m)
    {
        int good = ReadInt(m, "good_count");
        int bad = ReadInt(m, "bad_count");
        if (good == 0 && bad == 0) return content;

        content += $"  [good {good} / bad {bad}]";
        var notes = m["feedback_notes"]?.AsArray();
        if (notes is not null)
        {
            foreach (var note in notes)
            {
                if (note is null) continue;
                if (note["verdict"]?.GetValue<string>() == "bad")
                {
                    var reason = note["reason"]?.GetValue<string>();
                    if (!string.IsNullOrEmpty(reason))
                        content += $" (bad: \"{reason}\")";
                }
            }
        }
        return content;
    }

    private static string HashKey(string content)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return "mem-" + Convert.ToHexString(hash)[..16].ToLowerInvariant();
    }

    private static string? FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(v => !string.IsNullOrEmpty(v));

    private static int ReadInt(JsonNode node, string key)
    {
        var v = node[key];
        if (v is null) return 0;
        try { return v.GetValue<int>(); }
        catch { try { return (int)v.GetValue<double>(); } catch { return 0; } }
    }

    private static double ReadDouble(JsonNode node, string key)
    {
        var v = node[key];
        if (v is null) return 0.0;
        try { return v.GetValue<double>(); }
        catch { return 0.0; }
    }

    private static string Query(params (string Key, string? Value)[] pairs)
    {
        var parts = pairs
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value!)}")
            .ToList();
        return parts.Count == 0 ? "" : "?" + string.Join("&", parts);
    }

    public void Dispose()
    {
        if (_ownsClient) _client.Dispose();
    }
}

/// <summary>
/// Signed good/bad capability URLs for a memory, minted via
/// <see cref="OCGMemoryStore.GetFeedbackLinks"/>. Delivered to a human out-of-band;
/// never shown to the agent's LLM.
/// </summary>
public sealed record FeedbackLinks(string? GoodUrl, string? BadUrl, string? ExpiresAt);

/// <summary>Structured output for the conversation summarizer agent.</summary>
public sealed class MemorySummary
{
    /// <summary>One short paragraph: what happened / what was learned.</summary>
    public string Summary { get; init; } = "";
    /// <summary>Durable, reusable facts about the user or task (no chit-chat).</summary>
    public List<string> Facts { get; init; } = [];
    /// <summary>Short topical tags.</summary>
    public List<string> Tags { get; init; } = [];
}

/// <summary>
/// Handed to an agent's <see cref="Agent.FeedbackSink"/> after a conversation memory
/// is saved. Carries the distilled summary plus the signed capability URLs a human can
/// click to mark the memory good/bad. The integrator routes these out-of-band (e.g.
/// posts them into a Zendesk ticket). These URLs are never shown to the agent's LLM.
/// </summary>
public sealed class FeedbackEvent
{
    public string MemoryKey { get; init; } = "";
    public string Summary { get; init; } = "";
    public List<string> Facts { get; init; } = [];
    public List<string> Tags { get; init; } = [];
    public string? GoodUrl { get; init; }
    public string? BadUrl { get; init; }
    public string? ExpiresAt { get; init; }
    public string? Agent { get; init; }
    public string? User { get; init; }
    public string? SessionId { get; init; }
}

/// <summary>
/// Conversation summarization helpers (Claude-style distillation). The server-side
/// compiler builds an equivalent summarizer from the agent's <c>summaryModel</c>; this
/// mirror is exposed for cross-SDK parity and manual summarization.
/// </summary>
public static class OCGMemory
{
    public const string MemorySummarizerInstructions =
        "You distill a conversation into a durable memory. Read the transcript and " +
        "extract only reusable, durable facts about the user, their preferences, and " +
        "the task — the kind of thing worth remembering for next time. Ignore greetings, " +
        "filler, and one-off details. Write a one-paragraph summary, a short list of " +
        "facts, and a few topical tags. Be concise and concrete.";

    /// <summary>
    /// Build the internal agent that summarizes a conversation into a memory. It uses
    /// <see cref="MemorySummary"/> structured output and is intentionally created
    /// WITHOUT <see cref="Agent.SemanticMemory"/> so the post-run save hook skips it
    /// (no recursion).
    /// </summary>
    public static Agent BuildMemorySummarizer(string model, string name = "__memory_summarizer")
        => new(name)
        {
            Model = model,
            Instructions = MemorySummarizerInstructions,
            OutputType = typeof(MemorySummary),
            MaxTurns = 1,
        };
}
