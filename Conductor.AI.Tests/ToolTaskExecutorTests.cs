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
//
// Spec R6 — credential dispatch is fail-closed via Task.RuntimeMetadata
// (wire-delivered), never a fetch call or ambient env read.

using System.Text.Json;
using Conductor.AI;
using Conductor.Client.Models;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Conductor.AI.Tests;

public sealed class ToolTaskExecutorTests
{
    private static Conductor.Client.Models.Task StubTask(
        Dictionary<string, object>? inputData = null, Dictionary<string, string>? runtimeMetadata = null) =>
        new(inputData: inputData ?? new(), taskId: "task-1", workflowInstanceId: "wf-1")
        {
            RuntimeMetadata = runtimeMetadata,
        };

    [Fact]
    public async Task Handler_StringResult_WrappedAsPrimitiveResult()
    {
        var executor = new ToolTaskExecutor("t", (_, _) => Task.FromResult<object?>("hi"));

        var result = await executor.ExecuteAsync(StubTask(), CancellationToken.None);

        Assert.Equal(TaskResult.StatusEnum.COMPLETED, result.Status);
        Assert.Equal("hi", result.OutputData["result"]);
    }

    [Fact]
    public async Task Handler_DictResult_PassedThroughUnwrapped()
    {
        var executor = new ToolTaskExecutor("t",
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
            ["__agentspan_ctx__"] = JsonSerializer.Deserialize<JsonElement>("{}"),
            ["_agent_state"] = JsonSerializer.Deserialize<JsonElement>("{\"counter\":1}"),
        };
        var executor = new ToolTaskExecutor("t",
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
        var executor = new ToolTaskExecutor("t", (args, _) =>
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
        var executor = new ToolTaskExecutor("t",
            (_, _) => throw new TerminalToolException("bad config"));

        var result = await executor.ExecuteAsync(StubTask(), CancellationToken.None);

        Assert.Equal(TaskResult.StatusEnum.FAILEDWITHTERMINALERROR, result.Status);
        Assert.Equal("bad config", result.ReasonForIncompletion);
    }

    [Fact]
    public async Task GenericException_MapsToFailed_NotTerminal()
    {
        var executor = new ToolTaskExecutor("t",
            (_, _) => throw new InvalidOperationException("boom"));

        var result = await executor.ExecuteAsync(StubTask(), CancellationToken.None);

        Assert.Equal(TaskResult.StatusEnum.FAILED, result.Status);
    }

    // ── Spec R6: fail-closed runtimeMetadata credential dispatch ────────

    [Fact]
    public async Task DeclaredCredential_MissingFromRuntimeMetadata_MapsToFailedWithTerminalError()
    {
        var executor = new ToolTaskExecutor("t",
            (_, _) => Task.FromResult<object?>("unused"),
            credentialNames: new[] { "API_KEY" });

        // No RuntimeMetadata on the task at all — older/incapable server.
        var result = await executor.ExecuteAsync(StubTask(), CancellationToken.None);

        Assert.Equal(TaskResult.StatusEnum.FAILEDWITHTERMINALERROR, result.Status);
        Assert.Contains("API_KEY", result.ReasonForIncompletion);
    }

    [Fact]
    public async Task DeclaredCredential_PartiallyDelivered_StillFailsClosed()
    {
        var executor = new ToolTaskExecutor("t",
            (_, _) => Task.FromResult<object?>("unused"),
            credentialNames: new[] { "API_KEY", "OTHER_KEY" });

        var result = await executor.ExecuteAsync(
            StubTask(runtimeMetadata: new Dictionary<string, string> { ["API_KEY"] = "secret" }),
            CancellationToken.None);

        Assert.Equal(TaskResult.StatusEnum.FAILEDWITHTERMINALERROR, result.Status);
        Assert.Contains("OTHER_KEY", result.ReasonForIncompletion);
        Assert.DoesNotContain("API_KEY", result.ReasonForIncompletion);
    }

    [Fact]
    public async Task DeclaredCredential_Delivered_VisibleToHandlerViaCredentialScope()
    {
        string? seenValue = null;
        var executor = new ToolTaskExecutor("t", (_, _) =>
        {
            seenValue = ToolContext.GetCredential("API_KEY");
            return Task.FromResult<object?>("ok");
        }, credentialNames: new[] { "API_KEY" });

        var result = await executor.ExecuteAsync(
            StubTask(runtimeMetadata: new Dictionary<string, string> { ["API_KEY"] = "secret-value" }),
            CancellationToken.None);

        Assert.Equal(TaskResult.StatusEnum.COMPLETED, result.Status);
        Assert.Equal("secret-value", seenValue);
    }

    [Fact]
    public async Task AmbientEnvNeverReadAsFallback()
    {
        Environment.SetEnvironmentVariable("API_KEY", "from-ambient-env-should-be-ignored");
        try
        {
            var executor = new ToolTaskExecutor("t",
                (_, _) => Task.FromResult<object?>("unused"),
                credentialNames: new[] { "API_KEY" });

            // Server didn't deliver it via runtimeMetadata — must fail closed
            // even though the same-named env var happens to be set.
            var result = await executor.ExecuteAsync(StubTask(), CancellationToken.None);

            Assert.Equal(TaskResult.StatusEnum.FAILEDWITHTERMINALERROR, result.Status);
        }
        finally
        {
            Environment.SetEnvironmentVariable("API_KEY", null);
        }
    }
}
