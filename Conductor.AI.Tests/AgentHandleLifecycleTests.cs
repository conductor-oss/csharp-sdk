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
// DD4/C11/C12 — AgentHandle.PauseAsync/UnpauseAsync/SignalAsync delegation wire
// shapes: pause/resume hit the shared WorkflowResourceApi-equivalent endpoints
// (PUT /workflow/{id}/pause|resume), signal hits POST /agent/{id}/signal with
// the message wrapper. All stubbed — no live server needed.

using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Conductor.AI;
using Conductor.Client;
using RestSharp;
using Xunit;

namespace Conductor.AI.Tests;

public sealed class AgentHandleLifecycleTests
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

    private static (AgentHandle Handle, StubHandler Handler) BuildHandle(
        Func<HttpRequestMessage, (HttpStatusCode, string)> respond)
    {
        var handler = new StubHandler(respond);
        var configuration = new Configuration { BasePath = "http://server/api" };
        configuration.ApiClient.RestClient = new RestClient(new RestClientOptions("http://server/api")
        {
            ConfigureMessageHandler = _ => handler,
        });
        var client = new OrkesAgentClient(configuration);
        return (new AgentHandle("exec-1", client), handler);
    }

    [Fact]
    public async Task PauseAsync_PutsToWorkflowPauseEndpoint()
    {
        var (handle, handler) = BuildHandle(_ => (HttpStatusCode.OK, ""));

        await handle.PauseAsync();

        var req = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Put, req.Method);
        Assert.Equal("/api/workflow/exec-1/pause", req.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task UnpauseAsync_PutsToWorkflowResumeEndpoint()
    {
        var (handle, handler) = BuildHandle(_ => (HttpStatusCode.OK, ""));

        await handle.UnpauseAsync();

        var req = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Put, req.Method);
        Assert.Equal("/api/workflow/exec-1/resume", req.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task SignalAsync_PostsMessageWrapperToSignalEndpoint()
    {
        string? capturedBody = null;
        var (handle, handler) = BuildHandle(req =>
        {
            capturedBody = req.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return (HttpStatusCode.OK, "{}");
        });

        await handle.SignalAsync("go");

        var req = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.Equal("/api/agent/exec-1/signal", req.RequestUri!.AbsolutePath);
        Assert.Contains("\"message\":\"go\"", capturedBody);
    }
}
