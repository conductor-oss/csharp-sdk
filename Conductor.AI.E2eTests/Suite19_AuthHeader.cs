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
// Suite 19 — Shared token authority: OrkesAgentClient must send the SAME
// X-Authorization header the rest of the SDK sends — sourced from
// Configuration.AccessToken (TokenHandler mint/cache), not a bespoke client-side
// JWT mint. This is the single-token-authority contract (spec R2/R5): the agent
// client and the worker plane sharing one Configuration must never mint twice.
// No server needed — /token and downstream requests are both stubbed with an
// in-memory handler. Deterministic; fail-first validated.

using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Conductor.Client;
using Conductor.Client.Authentication;
using RestSharp;
using Xunit;

namespace Conductor.AI.E2eTests;

public sealed class Suite19_AuthHeader
{
    // A base64url JWT payload with a far-future exp so the cache holds.
    private const long FarFutureExp = 4102444800; // 2100-01-01
    private static string FakeJwt(long exp)
    {
        string B64Url(string s)
        {
            var b = Convert.ToBase64String(Encoding.UTF8.GetBytes(s));
            return b.TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
        return $"{B64Url("{\"alg\":\"HS256\"}")}.{B64Url($"{{\"exp\":{exp}}}")}.sig";
    }

    /// <summary>
    /// Routes <c>POST /token</c> to a stubbed mint response (counting calls) and
    /// captures the X-Authorization header seen on every other request. SSE
    /// requests (Accept: text/event-stream) get a minimal single-event body so
    /// <see cref="OrkesAgentClient.StreamEventsAsync"/> completes without a reconnect.
    /// </summary>
    private sealed class RoutingHandler : HttpMessageHandler
    {
        public readonly List<string?> SeenAuth = [];
        public int MintCount;
        private readonly string _jwt;

        public RoutingHandler(string jwt) => _jwt = jwt;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/token"))
            {
                Interlocked.Increment(ref MintCount);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent($"{{\"token\":\"{_jwt}\"}}", Encoding.UTF8, "application/json"),
                });
            }

            SeenAuth.Add(request.Headers.TryGetValues("X-Authorization", out var v)
                ? string.Join(",", v) : null);

            if (request.Headers.Accept.Any(a => a.MediaType == "text/event-stream"))
            {
                const string sse = "event: done\ndata: {\"status\":\"COMPLETED\"}\n\n";
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(sse, Encoding.UTF8, "text/event-stream"),
                });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json"),
            });
        }
    }

    private static Configuration BuildConfiguration(string? key, string? secret, RoutingHandler handler)
    {
        var configuration = new Configuration { BasePath = "http://server/api" };
        configuration.ApiClient.RestClient = new RestClient(new RestClientOptions("http://server/api")
        {
            ConfigureMessageHandler = _ => handler,
        });
        if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(secret))
            configuration.AuthenticationSettings = new OrkesAuthenticationSettings(key, secret);
        return configuration;
    }

    // ── 19.1  key+secret → minted JWT in X-Authorization, cached ──────────

    [Fact]
    public async Task KeySecret_MintsJwt_SendsXAuthorization_AndCaches()
    {
        var jwt = FakeJwt(FarFutureExp);
        var handler = new RoutingHandler(jwt);
        var configuration = BuildConfiguration("my-key", "my-secret", handler);
        using var client = new OrkesAgentClient(configuration);

        await client.GetStatusAsync("exec-1");
        await client.GetStatusAsync("exec-1");

        Assert.Equal(2, handler.SeenAuth.Count);
        Assert.All(handler.SeenAuth, h => Assert.Equal(jwt, h));
        Assert.Equal(1, handler.MintCount); // minted once, reused from TokenHandler's cache
    }

    // ── 19.2  no credentials → no auth header (OSS anonymous) ─────────────

    [Fact]
    public async Task NoCreds_NoAuthHeader()
    {
        var handler = new RoutingHandler("unused");
        var configuration = BuildConfiguration(null, null, handler);
        using var client = new OrkesAgentClient(configuration);

        await client.GetStatusAsync("exec-1");

        Assert.Single(handler.SeenAuth);
        Assert.Null(handler.SeenAuth[0]);
        Assert.Equal(0, handler.MintCount);
    }

    // ── 19.3  SSE sources its header from the same Configuration.AccessToken ──
    // (no second mint — single token authority across the non-streaming and
    // streaming call paths).

    [Fact]
    public async Task Sse_ReusesToken_NoSecondMint()
    {
        var jwt = FakeJwt(FarFutureExp);
        var handler = new RoutingHandler(jwt);
        var configuration = BuildConfiguration("my-key", "my-secret", handler);
        using var client = new OrkesAgentClient(configuration, sseHandler: handler);

        // Non-streaming call mints the token once...
        await client.GetStatusAsync("exec-1");

        // ...and the SSE stream reuses it — no second POST /token.
        var events = new List<AgentEvent>();
        await foreach (var ev in client.StreamEventsAsync("exec-1"))
            events.Add(ev);

        Assert.Single(events);
        Assert.Equal(EventType.Done, events[0].Type);
        Assert.Equal(1, handler.MintCount);
    }
}
