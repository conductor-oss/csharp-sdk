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
// The server's /agent/{id}/status response has no "error"/"reason" field — only
// "reasonForIncompletion". AgentResult.Error and AgentStatus.Reason used to read
// the wrong key and were always null on failed/terminated runs.

using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Conductor.Client;
using RestSharp;
using Xunit;

namespace Conductor.AI.Tests;

public sealed class AgentStatusMappingTests
{
    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, (HttpStatusCode Status, string Body)> _respond;
        public StubHandler(Func<HttpRequestMessage, (HttpStatusCode, string)> respond) => _respond = respond;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var (status, body) = _respond(request);
            return Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            });
        }
    }

    private static Configuration BuildConfig(Func<HttpRequestMessage, (HttpStatusCode, string)> respond)
    {
        var configuration = new Configuration { BasePath = "http://server/api" };
        configuration.ApiClient.RestClient = new RestClient(new RestClientOptions("http://server/api")
        {
            ConfigureMessageHandler = _ => new StubHandler(respond),
        });
        return configuration;
    }

    private static (HttpStatusCode, string) RouteStatusAndExecution(
        HttpRequestMessage request, string statusBody, string executionBody = "{}")
    {
        var path = request.RequestUri!.AbsolutePath;
        return path.EndsWith("/status")
            ? (HttpStatusCode.OK, statusBody)
            : (HttpStatusCode.OK, executionBody);
    }

    // ── AgentRuntime.GetStatusAsync ──────────────────────────────────────

    [Fact]
    public async Task GetStatusAsync_Failed_SurfacesReasonForIncompletion()
    {
        var config = BuildConfig(req => RouteStatusAndExecution(req, """
            {"executionId":"e1","status":"FAILED","isComplete":true,"isRunning":false,
             "isWaiting":false,"reasonForIncompletion":"Guardrail 'content_safety' failed: unsafe content"}
            """));

        using var runtime = new AgentRuntime(config);
        var status = await runtime.GetStatusAsync("e1");

        Assert.Equal("Guardrail 'content_safety' failed: unsafe content", status.Reason);
    }

    [Fact]
    public async Task GetStatusAsync_Completed_ReasonStaysNull()
    {
        var config = BuildConfig(req => RouteStatusAndExecution(req, """
            {"executionId":"e1","status":"COMPLETED","isComplete":true,"isRunning":false,"isWaiting":false}
            """));

        using var runtime = new AgentRuntime(config);
        var status = await runtime.GetStatusAsync("e1");

        Assert.Null(status.Reason);
    }

    [Fact]
    public async Task GetStatusAsync_PopulatesOutput()
    {
        var config = BuildConfig(req => RouteStatusAndExecution(req, """
            {"executionId":"e1","status":"COMPLETED","isComplete":true,"isRunning":false,
             "isWaiting":false,"output":{"result":"hello"}}
            """));

        using var runtime = new AgentRuntime(config);
        var status = await runtime.GetStatusAsync("e1");

        Assert.NotNull(status.Output);
    }

    // ── AgentHandle.WaitAsync (the RunAsync/PrintResult path) ────────────

    [Fact]
    public async Task WaitAsync_Failed_SurfacesReasonForIncompletionAsError()
    {
        var config = BuildConfig(req => RouteStatusAndExecution(req, """
            {"executionId":"e1","status":"FAILED","isComplete":true,"isRunning":false,
             "isWaiting":false,"reasonForIncompletion":"Guardrail 'content_safety' failed: unsafe content"}
            """));

        var handle = new AgentHandle("e1", new OrkesAgentClient(config));
        var result = await handle.WaitAsync();

        Assert.Equal(Status.Failed, result.Status);
        Assert.Equal("Guardrail 'content_safety' failed: unsafe content", result.Error);
    }

    [Fact]
    public async Task WaitAsync_Completed_ErrorStaysNull()
    {
        var config = BuildConfig(req => RouteStatusAndExecution(req, """
            {"executionId":"e1","status":"COMPLETED","isComplete":true,"isRunning":false,
             "isWaiting":false,"output":{"result":"hello"}}
            """));

        var handle = new AgentHandle("e1", new OrkesAgentClient(config));
        var result = await handle.WaitAsync();

        Assert.Equal(Status.Completed, result.Status);
        Assert.Null(result.Error);
    }
}
