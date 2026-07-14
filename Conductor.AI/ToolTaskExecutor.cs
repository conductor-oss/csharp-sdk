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
using System.Text.Json;
using System.Text.Json.Nodes;
using Conductor.Client.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Task = Conductor.Client.Models.Task;

namespace Conductor.AI;

/// <summary>
/// Loop-agnostic tool-task execution semantics: input conversion (Newtonsoft ↔
/// System.Text.Json), <see cref="ToolContext"/> extraction, credential
/// resolution + scoped env injection, <c>_state_updates</c> piggyback,
/// primitive-wrapping, and terminal-error mapping. Ported verbatim from the
/// pre-Worker-SDK <c>WorkerPollLoop.ExecuteAsync</c> — the only change is the
/// caller: this returns a <see cref="TaskResult"/> instead of calling
/// <c>UpdateTaskAsync</c> itself, since <see cref="Conductor.Client.Worker.WorkflowTaskExecutor"/>
/// now owns polling, batching, and the update-with-retry-backoff loop
/// (guide §25.1 — tools are ordinary Conductor workers).
/// </summary>
internal sealed class ToolTaskExecutor
{
    private readonly IAgentClient _http;
    private readonly string _taskName;
    private readonly Func<Dictionary<string, JsonElement>, ToolContext?, System.Threading.Tasks.Task<object?>> _handler;
    private readonly string[] _credentialNames;
    private readonly ILogger _logger;

    internal ToolTaskExecutor(
        IAgentClient http,
        string taskName,
        Func<Dictionary<string, JsonElement>, ToolContext?, System.Threading.Tasks.Task<object?>> handler,
        string[]? credentialNames = null,
        ILogger? logger = null)
    {
        _http = http;
        _taskName = taskName;
        _handler = handler;
        _credentialNames = credentialNames ?? [];
        _logger = logger ?? NullLogger.Instance;
    }

    public async System.Threading.Tasks.Task<TaskResult> ExecuteAsync(Task task, CancellationToken ct)
    {
        try
        {
            var inputData = ConvertInputData(task.InputData);
            var toolCtx = ExtractToolContext(inputData);

            // Strip internal keys from the handler-visible input
            var handlerInput = inputData
                .Where(kv => !string.Equals(kv.Key, "__agentspan_ctx__", StringComparison.OrdinalIgnoreCase)
                          && !string.Equals(kv.Key, "_agent_state", StringComparison.OrdinalIgnoreCase)
                          && !string.Equals(kv.Key, "method", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);

            // Resolve and inject credentials via the centralized helper so the
            // mutation + invocation + restoration is atomic under a single
            // process-wide lock. See docs/design/secret-injection-contract.md.
            // Tier-2 (env-injection) path; tier-1 (explicit-key) lands when the
            // user-facing API exposes a `credentials` parameter to agent factories.
            Dictionary<string, string> resolvedCredentials = new();
            if (_credentialNames.Length > 0)
            {
                var creds = await _http.ResolveCredentialsAsync(
                    toolCtx?.ExecutionToken, _credentialNames, ct);
                foreach (var (k, v) in creds)
                    resolvedCredentials[k] = v;
            }

            // Tier-1 (explicit accessor): populate the ambient credential scope so
            // tool code can read resolved credentials via ToolContext.GetCredential /
            // Secrets.Get without relying on process-env injection. Tier-2 (env
            // injection) below remains for framework-passthrough tools.
            object? result;
            using (CredentialScope.Begin(resolvedCredentials))
            {
                result = await CredentialInjection.InjectViaEnvAsync<object?>(
                    resolvedCredentials,
                    () => _handler(handlerInput, toolCtx),
                    ct);
            }

            // Wrap primitives — Conductor expects outputData as an object
            object outputData = result switch
            {
                null => new { result = (object?)null },
                string s => new { result = s },
                int i => new { result = i },
                long l => new { result = l },
                double d => new { result = d },
                bool b => new { result = b },
                _ => result,
            };

            // Include state updates so the server can persist shared state
            if (toolCtx?.State is { Count: > 0 } state)
            {
                if (outputData is Dictionary<string, object> outDict)
                    outDict["_state_updates"] = state;
                else if (outputData is Dictionary<string, object?> outDictN)
                    outDictN["_state_updates"] = state;
                else
                {
                    var wrapper = new Dictionary<string, object?> { ["_state_updates"] = state };
                    var resultJson = System.Text.Json.JsonSerializer.Serialize(outputData, AgentspanJson.Options);
                    var resultNode = JsonNode.Parse(resultJson);
                    if (resultNode is JsonObject obj)
                        foreach (var kv in obj)
                            wrapper[kv.Key] = kv.Value?.DeepClone();
                    else
                        wrapper["result"] = outputData;
                    outputData = wrapper;
                }
            }

            return new TaskResult(workflowInstanceId: task.WorkflowInstanceId, taskId: task.TaskId)
            {
                Status = TaskResult.StatusEnum.COMPLETED,
                OutputData = ToNewtonsoftDict(outputData),
            };
        }
        catch (TerminalToolException ex)
        {
            return new TaskResult(workflowInstanceId: task.WorkflowInstanceId, taskId: task.TaskId)
            {
                Status = TaskResult.StatusEnum.FAILEDWITHTERMINALERROR,
                ReasonForIncompletion = ex.Message,
            };
        }
        catch (Exception ex) when (
            ex is CredentialNotFoundException
               or CredentialAuthException
               or CredentialRateLimitException
               or CredentialServiceException)
        {
            // Credential failures are configuration issues — non-retryable.
            // Marking as terminal so the workflow surfaces the cause immediately
            // instead of burning retries on a broken config.
            _logger.LogError(ex, "Credential resolution failed for {TaskName}", _taskName);
            return new TaskResult(workflowInstanceId: task.WorkflowInstanceId, taskId: task.TaskId)
            {
                Status = TaskResult.StatusEnum.FAILEDWITHTERMINALERROR,
                ReasonForIncompletion = $"Credential resolution failed: {ex.Message}",
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Worker execution error for {TaskName}", _taskName);
            return new TaskResult(workflowInstanceId: task.WorkflowInstanceId, taskId: task.TaskId)
            {
                Status = TaskResult.StatusEnum.FAILED,
                ReasonForIncompletion = ex.Message,
            };
        }
    }

    // ── JSON bridges (Newtonsoft ↔ System.Text.Json) ──────────

    /// <summary>Convert conductor-csharp's Newtonsoft-deserialized inputData to STJ JsonElements.</summary>
    private static Dictionary<string, JsonElement> ConvertInputData(Dictionary<string, object>? inputData)
    {
        if (inputData is null || inputData.Count == 0)
            return new Dictionary<string, JsonElement>();

        var json = JsonConvert.SerializeObject(inputData);
        using var doc = System.Text.Json.JsonSerializer.Deserialize<JsonDocument>(json)!;
        var result = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        foreach (var prop in doc.RootElement.EnumerateObject())
            result[prop.Name] = prop.Value.Clone();
        return result;
    }

    /// <summary>Convert STJ-serializable output to a Newtonsoft-compatible dict for TaskResult.OutputData.</summary>
    private static Dictionary<string, object> ToNewtonsoftDict(object outputData)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(outputData, AgentspanJson.Options);
        return JsonConvert.DeserializeObject<Dictionary<string, object>>(json)
            ?? new Dictionary<string, object>();
    }

    private static ToolContext? ExtractToolContext(Dictionary<string, JsonElement> inputData)
    {
        ToolContext? ctx = null;
        if (inputData.TryGetValue("__agentspan_ctx__", out var ctxEl))
        {
            try { ctx = System.Text.Json.JsonSerializer.Deserialize<ToolContext>(ctxEl.GetRawText(), AgentspanJson.Options); }
            catch { }
        }

        Dictionary<string, object>? state = null;
        if (inputData.TryGetValue("_agent_state", out var agentStateEl) &&
            agentStateEl.ValueKind == JsonValueKind.Object)
        {
            state = new Dictionary<string, object>();
            foreach (var prop in agentStateEl.EnumerateObject())
                state[prop.Name] = prop.Value.Clone();
        }

        if (ctx is null && state is null) return null;
        return (ctx ?? new ToolContext()) with { State = state ?? ctx?.State };
    }
}
