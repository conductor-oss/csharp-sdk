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
// Guide §25.1 / DD6 — ToolTaskExecutor carries the loop-agnostic execution
// semantics extracted from the pre-Worker-SDK WorkerPollLoop: primitive
// wrapping, _state_updates piggyback, terminal-error mapping, and internal-key
// stripping. It now returns a TaskResult instead of updating the task itself
// (Conductor.Client.Worker.WorkflowTaskExecutor owns polling/update/retry).

using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Conductor.AI;
using Conductor.Client;
using Conductor.Client.Models;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Conductor.AI.Tests;

public sealed class ToolTaskExecutorTests
{
    private static Conductor.Client.Models.Task StubTask(Dictionary<string, object>? inputData = null) =>
        new(inputData: inputData ?? new(), taskId: "task-1", workflowInstanceId: "wf-1");

    [Fact]
    public async Task Handler_StringResult_WrappedAsPrimitiveResult()
    {
        var executor = new ToolTaskExecutor(http: null!, "t", (_, _) => Task.FromResult<object?>("hi"));

        var result = await executor.ExecuteAsync(StubTask(), CancellationToken.None);

        Assert.Equal(TaskResult.StatusEnum.COMPLETED, result.Status);
        Assert.Equal("hi", result.OutputData["result"]);
    }

    [Fact]
    public async Task Handler_DictResult_PassedThroughUnwrapped()
    {
        var executor = new ToolTaskExecutor(http: null!, "t",
            (_, _) => Task.FromResult<object?>(new Dictionary<string, object> { ["foo"] = "bar" }));

        var result = await executor.ExecuteAsync(StubTask(), CancellationToken.None);

        Assert.Equal(TaskResult.StatusEnum.COMPLETED, result.Status);
        Assert.Equal("bar", result.OutputData["foo"]);
    }

    [Fact]
    public async Task StatePiggyback_AddsStateUpdatesKey()
    {
        var inputData = new Dictionary<string, object>
        {
            ["__agentspan_ctx__"] = JsonSerializer.Deserialize<JsonElement>("{\"execution_token\":\"tok\"}"),
            ["_agent_state"] = JsonSerializer.Deserialize<JsonElement>("{\"counter\":1}"),
        };
        var executor = new ToolTaskExecutor(http: null!, "t",
            (_, ctx) => Task.FromResult<object?>(new Dictionary<string, object> { ["ok"] = true }));

        var result = await executor.ExecuteAsync(StubTask(inputData), CancellationToken.None);

        Assert.Equal(TaskResult.StatusEnum.COMPLETED, result.Status);
        Assert.True(result.OutputData.ContainsKey("_state_updates"));
    }

    [Fact]
    public async Task InternalKeys_StrippedFromHandlerInput()
    {
        Dictionary<string, JsonElement>? seen = null;
        var inputData = new Dictionary<string, object>
        {
            ["visible"] = "yes",
            ["method"] = "POST",
            ["__agentspan_ctx__"] = JsonSerializer.Deserialize<JsonElement>("{}"),
        };
        var executor = new ToolTaskExecutor(http: null!, "t", (args, _) =>
        {
            seen = args;
            return Task.FromResult<object?>("ok");
        });

        await executor.ExecuteAsync(StubTask(inputData), CancellationToken.None);

        Assert.NotNull(seen);
        Assert.True(seen!.ContainsKey("visible"));
        Assert.False(seen.ContainsKey("method"));
        Assert.False(seen.ContainsKey("__agentspan_ctx__"));
    }

    [Fact]
    public async Task TerminalToolException_MapsToFailedWithTerminalError()
    {
        var executor = new ToolTaskExecutor(http: null!, "t",
            (_, _) => throw new TerminalToolException("bad config"));

        var result = await executor.ExecuteAsync(StubTask(), CancellationToken.None);

        Assert.Equal(TaskResult.StatusEnum.FAILEDWITHTERMINALERROR, result.Status);
        Assert.Equal("bad config", result.ReasonForIncompletion);
    }

    [Fact]
    public async Task GenericException_MapsToFailed_NotTerminal()
    {
        var executor = new ToolTaskExecutor(http: null!, "t",
            (_, _) => throw new InvalidOperationException("boom"));

        var result = await executor.ExecuteAsync(StubTask(), CancellationToken.None);

        Assert.Equal(TaskResult.StatusEnum.FAILED, result.Status);
    }

    [Fact]
    public async Task CredentialResolutionFailure_MapsToFailedWithTerminalError()
    {
        // Stub the OrkesAgentClient's SSE-shared HttpClient (used by the pull-path
        // ResolveCredentialsAsync) to return a 200 body missing the requested name,
        // which ResolveCredentialsAsync maps to CredentialNotFoundException.
        var handler = new StubHandler(_ => (HttpStatusCode.OK, "{}"));
        var configuration = new Configuration { BasePath = "http://server/api" };
        var client = new OrkesAgentClient(configuration, handler);

        var executor = new ToolTaskExecutor(client, "t",
            (_, _) => Task.FromResult<object?>("unused"),
            credentialNames: new[] { "API_KEY" });

        var task = StubTask(new Dictionary<string, object>
        {
            ["__agentspan_ctx__"] = JsonSerializer.Deserialize<JsonElement>("{\"execution_token\":\"tok\"}"),
        });

        var result = await executor.ExecuteAsync(task, CancellationToken.None);

        Assert.Equal(TaskResult.StatusEnum.FAILEDWITHTERMINALERROR, result.Status);
        Assert.Contains("Credential resolution failed", result.ReasonForIncompletion);
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, (HttpStatusCode, string)> _respond;
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
}
