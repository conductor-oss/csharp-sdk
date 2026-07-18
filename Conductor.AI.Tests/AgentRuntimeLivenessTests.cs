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
// R11/T16 — a liveness monitor is only worth attaching for stateful runs (the
// ones whose tool tasks are routed to this process's own worker via a per-run
// domain); non-stateful runs have no single owning worker to go stale, and
// AgentConfig.LivenessEnabled=false must opt a caller out entirely.

using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Conductor.AI;
using Conductor.Client;
using RestSharp;
using Xunit;

namespace Conductor.AI.Tests;

public sealed class AgentRuntimeLivenessTests
{
    private sealed class RoutingHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage?> _route;
        public RoutingHandler(Func<HttpRequestMessage, HttpResponseMessage?> route) => _route = route;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
            => Task.FromResult(_route(request) ?? new HttpResponseMessage(HttpStatusCode.NoContent));
    }

    private static HttpResponseMessage Json(HttpStatusCode status, string body) => new(status)
    {
        Content = new StringContent(body, Encoding.UTF8, "application/json"),
    };

    private static Configuration BuildStubbedConfiguration(Func<HttpRequestMessage, HttpResponseMessage?> route)
    {
        var configuration = new Configuration { BasePath = "http://server/api" };
        configuration.ApiClient.RestClient = new RestClient(new RestClientOptions("http://server/api")
        {
            ConfigureMessageHandler = _ => new RoutingHandler(route),
        });
        return configuration;
    }

    private static HttpResponseMessage? RouteStart(HttpRequestMessage req) =>
        req.RequestUri!.AbsolutePath == "/api/agent/start"
            ? Json(HttpStatusCode.OK, "{\"executionId\":\"exec-1\"}")
            : null;

    [Fact]
    public async Task StatefulRun_LivenessEnabled_AttachesMonitor()
    {
        var config = BuildStubbedConfiguration(RouteStart);
        await using var runtime = new AgentRuntime(config, new AgentConfig
        {
            WorkerPollIntervalMs = 60_000,
            LivenessCheckIntervalSeconds = 3600,
        });
        var agent = new Agent("agent_one") { Model = "openai/gpt-4o", Stateful = true };

        var handle = await runtime.StartAsync(agent, "hi");

        Assert.True(handle.HasLivenessMonitor, "a stateful run with LivenessEnabled must get a monitor");
    }

    [Fact]
    public async Task StatefulRun_LivenessDisabled_NoMonitorAttached()
    {
        var config = BuildStubbedConfiguration(RouteStart);
        await using var runtime = new AgentRuntime(config, new AgentConfig
        {
            WorkerPollIntervalMs = 60_000,
            LivenessEnabled = false,
        });
        var agent = new Agent("agent_one") { Model = "openai/gpt-4o", Stateful = true };

        var handle = await runtime.StartAsync(agent, "hi");

        Assert.False(handle.HasLivenessMonitor, "LivenessEnabled=false must opt out of the monitor");
    }

    [Fact]
    public async Task NonStatefulRun_NoMonitorAttached_EvenWithLivenessEnabled()
    {
        var config = BuildStubbedConfiguration(RouteStart);
        await using var runtime = new AgentRuntime(config, new AgentConfig { WorkerPollIntervalMs = 60_000 });
        var agent = new Agent("agent_one") { Model = "openai/gpt-4o" }; // Stateful defaults to false

        var handle = await runtime.StartAsync(agent, "hi");

        Assert.False(handle.HasLivenessMonitor, "non-stateful runs have no single owning worker to go stale");
    }
}
