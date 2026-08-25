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
        => Route(request, statusBody, executionBody, "{}");

    private static (HttpStatusCode, string) Route(
        HttpRequestMessage request, string statusBody, string executionBody, string workflowBody)
    {
        var path = request.RequestUri!.AbsolutePath;
        if (path.EndsWith("/status")) return (HttpStatusCode.OK, statusBody);
        if (path.Contains("/execution/")) return (HttpStatusCode.OK, executionBody);
        return (HttpStatusCode.OK, workflowBody);
    }

    // ── pendingTool: HITL callers read response_schema off it ────────────
    //
    // AgentHandle and AgentRuntime each had a hand-written status mapper, and the
    // handle's omitted pendingTool entirely. Every HITL example that reads the
    // pending tool's response_schema off the handle (09b, 09c) therefore saw null
    // and silently stalled instead of prompting — the server was sending the field
    // all along. Both paths now share AgentStatus.FromNode; assert BOTH so a
    // future re-split cannot regress one of them unnoticed.

    private const string WaitingStatusBody = """
        {"executionId":"e1","status":"RUNNING","isComplete":false,"isRunning":true,
         "isWaiting":true,
         "pendingTool":{"taskRefName":"writer_09b_approval_human__1",
           "response_schema":{"type":"object",
             "properties":{"approved":{"type":"boolean","description":"Approve or reject"}},
             "required":["approved"]}}}
        """;

    [Fact]
    public async Task RuntimeGetStatusAsync_PopulatesPendingTool()
    {
        var config = BuildConfig(req => RouteStatusAndExecution(req, WaitingStatusBody));

        using var runtime = new AgentRuntime(config);
        var status = await runtime.GetStatusAsync("e1");

        Assert.NotNull(status.PendingTool);
        Assert.True(status.PendingTool!.ContainsKey("response_schema"));
    }

    [Fact]
    public async Task HandleGetStatusAsync_PopulatesPendingTool()
    {
        var config = BuildConfig(req => RouteStatusAndExecution(req, WaitingStatusBody));

        var handle = new AgentHandle("e1", new OrkesAgentClient(config));
        var status = await handle.GetStatusAsync();

        Assert.NotNull(status.PendingTool);
        Assert.True(status.PendingTool!.ContainsKey("response_schema"));
        Assert.True(status.IsWaiting);
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

    // ── Java parity: Error only read on non-Completed status ─────────────

    [Fact]
    public async Task WaitAsync_CompletedWithStrayReasonForIncompletion_ErrorIgnored()
    {
        // Defensive gate matching Java's `if (status != COMPLETED)` — even if the
        // server ever sent a stray reasonForIncompletion on a completed run, it
        // must not leak into Error.
        var config = BuildConfig(req => RouteStatusAndExecution(req, """
            {"executionId":"e1","status":"COMPLETED","isComplete":true,"isRunning":false,
             "isWaiting":false,"reasonForIncompletion":"should be ignored","output":{"result":"hello"}}
            """));

        var handle = new AgentHandle("e1", new OrkesAgentClient(config));
        var result = await handle.WaitAsync();

        Assert.Null(result.Error);
    }

    // ── Java parity: ToolCalls aggregated from workflow tasks ────────────

    [Fact]
    public async Task WaitAsync_ExtractsToolCallsFromWorkflowTasks()
    {
        var config = BuildConfig(req => Route(
            req,
            statusBody: """
                {"executionId":"e1","status":"COMPLETED","isComplete":true,"isRunning":false,"isWaiting":false}
                """,
            executionBody: "{}",
            workflowBody: """
                {"tasks":[
                    {"taskType":"echo","referenceTaskName":"call_echo_1",
                     "inputData":{"query":"hello","_internal":"drop me","ctx":"drop me too"},
                     "outputData":{"result":"echoed: hello"}},
                    {"taskType":"LLM_CHAT_COMPLETE","referenceTaskName":"llm_1",
                     "inputData":{},"outputData":{"promptTokens":10}}
                ]}
                """));

        var handle = new AgentHandle("e1", new OrkesAgentClient(config));
        var result = await handle.WaitAsync();

        var toolCall = Assert.Single(result.ToolCalls!);
        Assert.Equal("echo", toolCall["name"]);
        var args = Assert.IsType<Dictionary<string, object>>(toolCall["args"]);
        Assert.Equal(["query"], args.Keys);
        Assert.Equal("echoed: hello", toolCall["result"]!.ToString());
    }

    [Fact]
    public async Task WaitAsync_NoMatchingTasks_ToolCallsStaysNull()
    {
        var config = BuildConfig(req => Route(
            req,
            statusBody: """
                {"executionId":"e1","status":"COMPLETED","isComplete":true,"isRunning":false,"isWaiting":false}
                """,
            executionBody: "{}",
            workflowBody: """{"tasks":[]}"""));

        var handle = new AgentHandle("e1", new OrkesAgentClient(config));
        var result = await handle.WaitAsync();

        Assert.Null(result.ToolCalls);
    }
}
