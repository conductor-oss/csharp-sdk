/*
 * Copyright 2026 Conductor Authors.
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
using System.Net.Http;
using System.Text;
using Conductor.AI.Scheduling;
using Xunit;

namespace Conductor.AI.Tests;

public class SchedulesVerbFallbackTests
{
    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, (HttpStatusCode, string)> _respond;
        public readonly List<string> Requests = new();
        public StubHandler(Func<HttpRequestMessage, (HttpStatusCode, string)> respond) => _respond = respond;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Requests.Add($"{request.Method} {request.RequestUri!.PathAndQuery}");
            var (code, body) = _respond(request);
            return Task.FromResult(new HttpResponseMessage(code)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            });
        }
    }

    [Fact]
    public async Task PauseAsync_SendsPutFirst()
    {
        var handler = new StubHandler(_ => (HttpStatusCode.OK, "{}"));
        var schedules = new Schedules(new HttpClient(handler), "http://localhost:8080");

        await schedules.PauseAsync("digest-daily");

        var request = Assert.Single(handler.Requests);
        Assert.Equal("PUT /scheduler/schedules/digest-daily/pause", request);
    }

    [Fact]
    public async Task PauseAsync_FallsBackToGetOn405()
    {
        var handler = new StubHandler(req => req.Method == HttpMethod.Put
            ? (HttpStatusCode.MethodNotAllowed, "")
            : (HttpStatusCode.OK, "{}"));
        var schedules = new Schedules(new HttpClient(handler), "http://localhost:8080");

        await schedules.PauseAsync("digest-daily");

        Assert.Equal(2, handler.Requests.Count);
        Assert.Equal("PUT /scheduler/schedules/digest-daily/pause", handler.Requests[0]);
        Assert.Equal("GET /scheduler/schedules/digest-daily/pause", handler.Requests[1]);
    }

    [Fact]
    public async Task ResumeAsync_FallsBackToGetOn405Independently_EachCallRetriesPutFirst()
    {
        var handler = new StubHandler(req => req.Method == HttpMethod.Put
            ? (HttpStatusCode.MethodNotAllowed, "")
            : (HttpStatusCode.OK, "{}"));
        var schedules = new Schedules(new HttpClient(handler), "http://localhost:8080");

        await schedules.PauseAsync("digest-daily");
        await schedules.ResumeAsync("digest-daily");

        Assert.Equal(4, handler.Requests.Count);
        Assert.Equal("PUT /scheduler/schedules/digest-daily/resume", handler.Requests[2]);
        Assert.Equal("GET /scheduler/schedules/digest-daily/resume", handler.Requests[3]);
    }

    [Fact]
    public async Task PauseAsync_ReasonSurvivesTheGetFallback()
    {
        var handler = new StubHandler(req => req.Method == HttpMethod.Put
            ? (HttpStatusCode.MethodNotAllowed, "")
            : (HttpStatusCode.OK, "{}"));
        var schedules = new Schedules(new HttpClient(handler), "http://localhost:8080");

        await schedules.PauseAsync("digest-daily", reason: "maintenance");

        Assert.Contains(handler.Requests, r => r == "GET /scheduler/schedules/digest-daily/pause?reason=maintenance");
    }

    [Fact]
    public async Task PauseAsync_RethrowsOn403WithoutFallingBackToGet()
    {
        var handler = new StubHandler(_ => (HttpStatusCode.Forbidden, ""));
        var schedules = new Schedules(new HttpClient(handler), "http://localhost:8080");

        await Assert.ThrowsAsync<ScheduleException>(() => schedules.PauseAsync("digest-daily"));

        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task ResumeAsync_FallsBackToGetOn405()
    {
        var handler = new StubHandler(req => req.Method == HttpMethod.Put
            ? (HttpStatusCode.MethodNotAllowed, "")
            : (HttpStatusCode.OK, "{}"));
        var schedules = new Schedules(new HttpClient(handler), "http://localhost:8080");

        await schedules.ResumeAsync("digest-daily");

        Assert.Equal(2, handler.Requests.Count);
        Assert.Equal("PUT /scheduler/schedules/digest-daily/resume", handler.Requests[0]);
        Assert.Equal("GET /scheduler/schedules/digest-daily/resume", handler.Requests[1]);
    }
}
