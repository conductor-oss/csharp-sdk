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
// T14 — spec R9 verb contract: `serve` = deploy + serve (one POST /agent/deploy
// per served agent, before workers start), `blocking:false` returns once workers
// are registered and polling. Plus AgentConfig.AutoStartWorkers gating (run/start
// skip worker registration when disabled) and the try/finally worker-shutdown
// guarantee when a run throws mid-wait. All stubbed via RestClient's
// ConfigureMessageHandler seam — no live server needed.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Conductor.AI;
using Conductor.Client;
using RestSharp;
using Xunit;

namespace Conductor.AI.Tests;

public sealed class AgentRuntimeServeTests
{
    private sealed class RoutingHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage?> _route;
        private readonly List<string> _requests = new();
        public IReadOnlyList<string> Requests => _requests;

        public RoutingHandler(Func<HttpRequestMessage, HttpResponseMessage?> route) => _route = route;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            lock (_requests) _requests.Add($"{request.Method} {request.RequestUri!.AbsolutePath}");
            return Task.FromResult(_route(request) ?? new HttpResponseMessage(HttpStatusCode.NoContent));
        }
    }

    private static HttpResponseMessage Json(HttpStatusCode status, string body) => new(status)
    {
        Content = new StringContent(body, Encoding.UTF8, "application/json"),
    };

    private static (Configuration Configuration, RoutingHandler Handler) BuildStubbedConfiguration(
        Func<HttpRequestMessage, HttpResponseMessage?> route)
    {
        var handler = new RoutingHandler(route);
        var configuration = new Configuration { BasePath = "http://server/api" };
        configuration.ApiClient.RestClient = new RestClient(new RestClientOptions("http://server/api")
        {
            ConfigureMessageHandler = _ => handler,
        });
        return (configuration, handler);
    }

    private static Agent MinimalAgent(string name) => new(name) { Model = "openai/gpt-4o" };

    // ── serve = deploy + serve (R9) ─────────────────────────────────────

    [Fact]
    public async Task NonBlockingServe_DeploysAgent_RegistersWorkers_AndReturnsPromptly()
    {
        var (config, handler) = BuildStubbedConfiguration(req =>
            req.RequestUri!.AbsolutePath == "/api/agent/deploy"
                ? Json(HttpStatusCode.OK, "{\"agentName\":\"registered\"}")
                : null);

        await using var runtime = new AgentRuntime(config, new AgentConfig { WorkerPollIntervalMs = 60_000 });

        await runtime.ServeAsync(blocking: false, agents: new[] { MinimalAgent("agent_one") });

        Assert.Equal(1, handler.Requests.Count(r => r == "POST /api/agent/deploy"));
        Assert.True(runtime.HasActiveWorkers, "serve must register + start workers even in non-blocking mode");
    }

    [Fact]
    public async Task NonBlockingServe_DeploysEveryServedAgentOnce()
    {
        var (config, handler) = BuildStubbedConfiguration(req =>
            req.RequestUri!.AbsolutePath == "/api/agent/deploy"
                ? Json(HttpStatusCode.OK, "{\"agentName\":\"registered\"}")
                : null);

        await using var runtime = new AgentRuntime(config, new AgentConfig { WorkerPollIntervalMs = 60_000 });

        await runtime.ServeAsync(blocking: false,
            agents: new[] { MinimalAgent("agent_one"), MinimalAgent("agent_two") });

        Assert.Equal(2, handler.Requests.Count(r => r == "POST /api/agent/deploy"));
    }

    // ── AgentConfig.AutoStartWorkers gates run/start registration ───────

    [Fact]
    public async Task AutoStartWorkersFalse_SkipsWorkerRegistration_ButStartStillSucceeds()
    {
        var (config, _) = BuildStubbedConfiguration(req =>
            req.RequestUri!.AbsolutePath == "/api/agent/start"
                ? Json(HttpStatusCode.OK, "{\"executionId\":\"exec-1\"}")
                : null);

        await using var runtime = new AgentRuntime(config, new AgentConfig { AutoStartWorkers = false });

        var handle = await runtime.StartAsync(MinimalAgent("agent_one"), "hi");

        Assert.Equal("exec-1", handle.ExecutionId);
        Assert.False(runtime.HasActiveWorkers, "AutoStartWorkers=false must skip worker registration for run/start");
    }

    [Fact]
    public async Task AutoStartWorkersTrue_RegistersWorkers()
    {
        var (config, _) = BuildStubbedConfiguration(req =>
            req.RequestUri!.AbsolutePath == "/api/agent/start"
                ? Json(HttpStatusCode.OK, "{\"executionId\":\"exec-1\"}")
                : null);

        await using var runtime = new AgentRuntime(config, new AgentConfig { WorkerPollIntervalMs = 60_000 });

        await runtime.StartAsync(MinimalAgent("agent_one"), "hi");

        Assert.True(runtime.HasActiveWorkers);
    }

    // ── try/finally worker shutdown on a throwing WaitAsync ─────────────

    [Fact]
    public async Task RunAsync_WaitAsyncThrows_StillStopsWorkers()
    {
        var (config, _) = BuildStubbedConfiguration(req =>
        {
            var path = req.RequestUri!.AbsolutePath;
            if (path == "/api/agent/start") return Json(HttpStatusCode.OK, "{\"executionId\":\"exec-1\"}");
            if (path.EndsWith("/status")) return Json(HttpStatusCode.InternalServerError, "boom");
            return null;
        });

        await using var runtime = new AgentRuntime(config, new AgentConfig { WorkerPollIntervalMs = 60_000 });

        await Assert.ThrowsAsync<AgentApiException>(() => runtime.RunAsync(MinimalAgent("agent_one"), "hi"));

        Assert.False(runtime.HasActiveWorkers, "the finally block must stop workers even though WaitAsync threw");
    }
}
