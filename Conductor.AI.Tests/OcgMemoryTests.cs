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
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Conductor.AI;
using Xunit;

namespace Conductor.AI.Tests;

/// <summary>
/// Unit tests for OCG-backed long-term memory: the <see cref="OCGMemoryStore"/> HTTP
/// adapter and the <c>longTermMemory</c> / <c>feedbackSink</c> serializer emission.
/// Modeled on Python's test_ocg_memory_store.py + test_config_serializer.py additions.
/// </summary>
public class OcgMemoryStoreTests
{
    // A minimal HttpMessageHandler that routes every request through a func — the C#
    // analogue of httpx.MockTransport used in the Python tests.
    private sealed class MockHandler(Func<HttpRequestMessage, HttpResponseMessage> fn) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
            => Task.FromResult(fn(request));
    }

    private static OCGMemoryStore StoreWith(Func<HttpRequestMessage, HttpResponseMessage> handler)
    {
        var client = new HttpClient(new MockHandler(handler));
        return new OCGMemoryStore(url: "https://ocg.test", agent: "agent:a", user: "user:bob", client: client);
    }

    private static HttpResponseMessage Json(HttpStatusCode code, string json)
        => new(code) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    [Fact]
    public void Add_posts_value_field_and_no_confidence()
    {
        string? capturedUrl = null;
        JsonNode? capturedBody = null;

        var store = StoreWith(req =>
        {
            capturedUrl = req.RequestUri!.ToString();
            capturedBody = JsonNode.Parse(req.Content!.ReadAsStringAsync().GetAwaiter().GetResult());
            return Json(HttpStatusCode.OK, "{\"key\":\"k1\"}");
        });

        var key = store.Add(new MemoryEntry
        {
            Content = "alice prefers email",
            Metadata = new Dictionary<string, object> { ["key"] = "pref" },
        });

        Assert.Equal("pref", key);
        Assert.EndsWith("/api/v1/memories", capturedUrl);
        var body = capturedBody!.AsObject();
        Assert.Equal("alice prefers email", body["value"]!.GetValue<string>()); // field is "value", NOT "string_value"
        Assert.False(body.ContainsKey("string_value"));
        Assert.False(body.ContainsKey("confidence")); // confidence was removed from the API
        Assert.Equal("agent:a", body["agent"]!.GetValue<string>());
        Assert.Equal("user:bob", body["user"]!.GetValue<string>());
        Assert.Equal("agent_inferred", body["source"]!.GetValue<string>());
    }

    [Fact]
    public void Search_folds_good_bad_signal_into_content()
    {
        var store = StoreWith(req =>
        {
            Assert.EndsWith("/api/v1/memories/search", req.RequestUri!.ToString());
            return Json(HttpStatusCode.OK, """
                {
                  "memories": [
                    {
                      "key": "m1",
                      "value_preview": "use us-east-1",
                      "good_count": 2,
                      "bad_count": 1,
                      "relevance_score": 0.9,
                      "feedback_notes": [{"verdict": "bad", "reason": "stale region"}]
                    }
                  ]
                }
                """);
        });

        var entries = store.Search("which region", topK: 5);
        Assert.Single(entries);
        Assert.Contains("[good 2 / bad 1]", entries[0].Content);
        Assert.Contains("bad: \"stale region\"", entries[0].Content);
    }

    [Fact]
    public void FeedbackLinks_hits_mint_route()
    {
        var store = StoreWith(req =>
        {
            var path = req.RequestUri!.ToString().Split('?')[0];
            Assert.EndsWith("/api/v1/memories/k1/feedback-links", path);
            return Json(HttpStatusCode.OK, """
                {
                  "good_url": "https://ocg.test/api/v1/feedback/GOOD",
                  "bad_url": "https://ocg.test/api/v1/feedback/BAD",
                  "expires_at": "2026-09-01T00:00:00Z"
                }
                """);
        });

        var links = store.GetFeedbackLinks("k1");
        Assert.EndsWith("/feedback/GOOD", links.GoodUrl);
        Assert.EndsWith("/feedback/BAD", links.BadUrl);
        Assert.Equal("2026-09-01T00:00:00Z", links.ExpiresAt);
    }

    [Fact]
    public void Non_2xx_raises()
    {
        var store = StoreWith(_ => Json(HttpStatusCode.InternalServerError, "boom"));
        Assert.Throws<AgentApiException>(() =>
            store.Add(new MemoryEntry { Content = "x", Metadata = new() { ["key"] = "k" } }));
    }

    [Fact]
    public void Blank_url_or_agent_rejected()
    {
        Assert.Throws<ArgumentException>(() => new OCGMemoryStore(url: "  ", agent: "agent:a"));
        Assert.Throws<ArgumentException>(() => new OCGMemoryStore(url: "https://ocg.test", agent: ""));
    }
}

/// <summary>
/// Serializer emission of <c>longTermMemory</c> + <c>feedbackSink</c> — the piece the
/// server relies on to activate OCG-backed memory on the compiled/deployed path.
/// Mirrors Python's test_config_serializer.py additions.
/// </summary>
public class LongTermMemorySerializerTests
{
    private static JsonObject SerializeAgent(Agent agent) => AgentConfigSerializer.SerializeAgent(agent);

    [Fact]
    public void Serialize_long_term_memory()
    {
        var store = new OCGMemoryStore(
            url: "https://ocg.example.com/",
            agent: "agent:ce-ticket-resolution",
            user: "user:alice",
            scope: "agent");
        var sm = new SemanticMemory(store: store, maxResults: 7);

        var agent = new Agent("ce_agent")
        {
            Model = "openai/gpt-4o",
            Instructions = "Resolve tickets.",
            SemanticMemory = sm,
            MemorySummaryModel = "openai/gpt-4o-mini",
            FeedbackSink = _ => { },
        };
        var cfg = SerializeAgent(agent);

        var ltm = cfg["longTermMemory"]!;
        Assert.Equal("https://ocg.example.com", ltm["ocgUrl"]!.GetValue<string>()); // trailing slash stripped
        Assert.Equal("OCG_PUBLIC_KEY", ltm["credential"]!.GetValue<string>());      // server-resolvable name, not token
        Assert.Equal("agent:ce-ticket-resolution", ltm["agent"]!.GetValue<string>());
        Assert.Equal("user:alice", ltm["user"]!.GetValue<string>());
        Assert.Equal("agent", ltm["scope"]!.GetValue<string>());
        Assert.Equal(7, ltm["maxResults"]!.GetValue<int>());
        Assert.Equal("openai/gpt-4o-mini", ltm["summaryModel"]!.GetValue<string>());

        Assert.Equal("ce_agent_feedback_sink", cfg["feedbackSink"]!["taskName"]!.GetValue<string>());
    }

    [Fact]
    public void Serialize_long_term_memory_absent()
    {
        var agent = new Agent("plain") { Model = "openai/gpt-4o", Instructions = "Hi." };
        var cfg = SerializeAgent(agent);

        Assert.Null(cfg["longTermMemory"]);
        Assert.Null(cfg["feedbackSink"]);
    }

    [Fact]
    public void Serialize_long_term_memory_summary_model_fallback()
    {
        var store = new OCGMemoryStore(url: "https://ocg.example.com", agent: "agent:x");
        var sm = new SemanticMemory(store: store, maxResults: 5);
        var agent = new Agent("a") { Model = "anthropic/claude", SemanticMemory = sm };
        var cfg = SerializeAgent(agent);

        Assert.Equal("anthropic/claude", cfg["longTermMemory"]!["summaryModel"]!.GetValue<string>());
        Assert.Null(cfg["feedbackSink"]); // no feedback sink -> no feedbackSink emitted
    }

    [Fact]
    public void Non_ocg_store_does_not_compile()
    {
        // A non-OCG store (plain InMemoryStore) has no base url to call server-side,
        // so it must NOT emit longTermMemory.
        var sm = new SemanticMemory(store: new InMemoryStore(), maxResults: 5);
        var agent = new Agent("a") { Model = "openai/gpt-4o", SemanticMemory = sm };
        var cfg = SerializeAgent(agent);

        Assert.Null(cfg["longTermMemory"]);
    }
}
