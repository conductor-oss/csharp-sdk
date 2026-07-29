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
// T5/T9 — OrkesAgentClient error mapping (404 → AgentNotFoundException, 5xx →
// AgentApiException, enrichment reads swallow to null) and the new SignalAsync/
// ListExecutionsAsync wire shapes. All stubbed — no live server needed.

using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Conductor.AI;
using Conductor.Client;
using RestSharp;
using Xunit;

namespace Conductor.AI.Tests;

public sealed class OrkesAgentClientTests
{
    private sealed class StubHandler : HttpMessageHandler
    {
        public readonly List<HttpRequestMessage> Requests = new();
        private readonly Func<HttpRequestMessage, (HttpStatusCode Status, string Body)> _respond;
        public StubHandler(Func<HttpRequestMessage, (HttpStatusCode, string)> respond) => _respond = respond;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Requests.Add(request);
            var (status, body) = _respond(request);
            return Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            });
        }
    }

    private static (OrkesAgentClient Client, StubHandler Handler) BuildClient(
        Func<HttpRequestMessage, (HttpStatusCode, string)> respond)
    {
        var handler = new StubHandler(respond);
        var configuration = new Configuration { BasePath = "http://server/api" };
        configuration.ApiClient.RestClient = new RestClient(new RestClientOptions("http://server/api")
        {
            ConfigureMessageHandler = _ => handler,
        });
        return (new OrkesAgentClient(configuration), handler);
    }

    // ── T5 error mapping ────────────────────────────────────────────────

    [Fact]
    public async Task GetStatusAsync_404_ThrowsAgentNotFoundException()
    {
        var (client, _) = BuildClient(_ => (HttpStatusCode.NotFound, "not found"));

        await Assert.ThrowsAsync<AgentNotFoundException>(() => client.GetStatusAsync("missing-exec"));
    }

    [Fact]
    public async Task DeployAsync_500_ThrowsAgentApiException()
    {
        var (client, _) = BuildClient(_ => (HttpStatusCode.InternalServerError, "boom"));

        var ex = await Assert.ThrowsAsync<AgentApiException>(
            () => client.DeployAsync(new JsonObject { ["name"] = "a" }));
        Assert.Equal(500, ex.StatusCode);
    }

    [Fact]
    public async Task GetExecutionAsync_OnFailure_ReturnsNull()
    {
        var (client, _) = BuildClient(_ => (HttpStatusCode.NotFound, "not found"));

        var result = await client.GetExecutionAsync("missing-exec");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetWorkflowAsync_OnFailure_ReturnsNull()
    {
        var (client, _) = BuildClient(_ => (HttpStatusCode.InternalServerError, "boom"));

        var result = await client.GetWorkflowAsync("exec-1");

        Assert.Null(result);
    }

    // ── SignalAsync / ListExecutionsAsync wire shapes ──────────────────

    [Fact]
    public async Task SignalAsync_PostsMessageWrapperToSignalEndpoint()
    {
        string? capturedBody = null;
        var (client, handler) = BuildClient(req =>
        {
            capturedBody = req.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return (HttpStatusCode.OK, "{}");
        });

        await client.SignalAsync("exec-1", "resume-now");

        var req = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.Equal("/api/agent/exec-1/signal", req.RequestUri!.AbsolutePath);
        Assert.Contains("\"message\":\"resume-now\"", capturedBody);
    }

    [Fact]
    public async Task ListExecutionsAsync_SendsQueryParamsAndParsesResponse()
    {
        var (client, handler) = BuildClient(req => (HttpStatusCode.OK, "{\"executions\":[{\"executionId\":\"e1\"}]}"));

        var result = await client.ListExecutionsAsync(new Dictionary<string, string> { ["status"] = "RUNNING" });

        var req = Assert.Single(handler.Requests);
        Assert.Equal("/api/agent/executions", req.RequestUri!.AbsolutePath);
        Assert.Contains("status=RUNNING", req.RequestUri!.Query);
        Assert.Equal("e1", result?["executions"]?[0]?["executionId"]?.GetValue<string>());
    }

    [Fact]
    public async Task StartAsync_PostsToAgentStart_ReturnsExecutionId()
    {
        var (client, handler) = BuildClient(_ => (HttpStatusCode.OK, "{\"executionId\":\"exec-42\"}"));

        var executionId = await client.StartAsync(new JsonObject { ["agentConfig"] = new JsonObject() });

        Assert.Equal("exec-42", executionId);
        var req = Assert.Single(handler.Requests);
        Assert.Equal("/api/agent/start", req.RequestUri!.AbsolutePath);
    }
}
