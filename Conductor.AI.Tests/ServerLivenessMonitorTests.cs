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
// R11/T16 — stateful runs route tool tasks to this process's own worker via a
// per-run domain; if that worker dies, the server-side task sits at
// pollCount=0 forever and a blocking WaitAsync would hang indefinitely. The
// liveness monitor polls the workflow's tasks and flags a stall once a
// pending (SCHEDULED/IN_PROGRESS) task has gone unpolled past the configured
// threshold. All stubbed — no live server needed.

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

public sealed class ServerLivenessMonitorTests
{
    private sealed class StubHandler : HttpMessageHandler
    {
        private int _callCount;
        public int CallCount => _callCount;
        private readonly Func<HttpRequestMessage, (HttpStatusCode Status, string Body)> _respond;
        public StubHandler(Func<HttpRequestMessage, (HttpStatusCode, string)> respond) => _respond = respond;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Interlocked.Increment(ref _callCount);
            var (status, body) = _respond(request);
            return Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            });
        }
    }

    private static (ServerLivenessMonitor Monitor, StubHandler Handler) BuildMonitor(
        Func<HttpRequestMessage, (HttpStatusCode, string)> respond,
        double stallSeconds = 0.3,
        double checkIntervalSeconds = 0.05)
    {
        var handler = new StubHandler(respond);
        var configuration = new Configuration { BasePath = "http://server/api" };
        configuration.ApiClient.RestClient = new RestClient(new RestClientOptions("http://server/api")
        {
            ConfigureMessageHandler = _ => handler,
        });
        var monitor = new ServerLivenessMonitor(configuration, "exec-1", stallSeconds, checkIntervalSeconds);
        return (monitor, handler);
    }

    private static async Task WaitUntilAsync(Func<bool> predicate, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            if (predicate()) return;
            await Task.Delay(20);
        }
        Assert.True(predicate(), "condition was not met before the timeout");
    }

    [Fact]
    public async Task UnpolledPendingTask_PastStallThreshold_FlagsStall()
    {
        var (monitor, _) = BuildMonitor(_ => (HttpStatusCode.OK, """
            { "status": "RUNNING", "tasks": [
                { "status": "SCHEDULED", "pollCount": 0, "scheduledTime": 1, "referenceTaskName": "my_tool_ref" }
            ] }
            """));

        await WaitUntilAsync(() => monitor.StalledTaskRef is not null, TimeSpan.FromSeconds(2));

        Assert.Equal("my_tool_ref", monitor.StalledTaskRef);
        await monitor.DisposeAsync();
    }

    [Fact]
    public async Task PendingTask_AlreadyPolled_NeverFlagsStall()
    {
        var (monitor, handler) = BuildMonitor(_ => (HttpStatusCode.OK, """
            { "status": "RUNNING", "tasks": [
                { "status": "IN_PROGRESS", "pollCount": 3, "scheduledTime": 1, "referenceTaskName": "my_tool_ref" }
            ] }
            """));

        await WaitUntilAsync(() => handler.CallCount >= 3, TimeSpan.FromSeconds(2));

        Assert.Null(monitor.StalledTaskRef);
        await monitor.DisposeAsync();
    }

    [Fact]
    public async Task TerminalWorkflowStatus_StopsPollingAndNeverFlagsStall()
    {
        var (monitor, handler) = BuildMonitor(_ => (HttpStatusCode.OK, """{ "status": "COMPLETED" }"""));

        await WaitUntilAsync(() => handler.CallCount >= 1, TimeSpan.FromSeconds(2));
        var countAfterTerminal = handler.CallCount;
        await Task.Delay(300); // several more check intervals worth of time

        Assert.Equal(countAfterTerminal, handler.CallCount);
        Assert.Null(monitor.StalledTaskRef);
        await monitor.DisposeAsync();
    }

    [Fact]
    public async Task DisposeAsync_StopsTheBackgroundLoop()
    {
        var (monitor, handler) = BuildMonitor(_ => (HttpStatusCode.OK, """{ "status": "RUNNING", "tasks": [] }"""));

        await WaitUntilAsync(() => handler.CallCount >= 1, TimeSpan.FromSeconds(2));
        await monitor.DisposeAsync();

        var countAfterDispose = handler.CallCount;
        await Task.Delay(300);

        Assert.Equal(countAfterDispose, handler.CallCount);
    }
}
