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
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Conductor.AI.Scheduling;
using Conductor.Client;
using RestSharp;

namespace Conductor.AI;

/// <summary>
/// Default <see cref="IAgentClient"/> implementation. Every non-streaming call
/// routes through the shared <see cref="Client.ApiClient.ExecuteAsync{T}"/> built
/// from one <see cref="Configuration"/> — the same code path the generated
/// resource APIs (<see cref="Conductor.Api.WorkflowResourceApi"/> etc.) use. This
/// inherits X-Authorization header injection, <c>TokenHandler</c> mint/cache, and
/// one-shot 401 refresh-retry for free; this class adds no token logic of its own.
///
/// <para>SSE cannot ride the generic call method — <see cref="StreamEventsAsync"/>
/// uses a dedicated streaming <see cref="HttpClient"/>, sourcing its auth header
/// fresh from <see cref="Configuration.AccessToken"/> on every (re)connect.</para>
///
/// <para><see cref="Dispose"/>/<see cref="DisposeAsync"/> only release the
/// SSE transport this class privately owns — the shared
/// <see cref="Configuration"/>/<see cref="Client.ApiClient"/> is owned by the
/// caller and is never disposed here.</para>
/// </summary>
public sealed class OrkesAgentClient : IAgentClient
{
    private readonly Configuration _configuration;
    private readonly HttpClient _sseClient;
    private Schedules? _schedules;

    public OrkesAgentClient(Configuration configuration) : this(configuration, sseHandler: null)
    {
    }

    /// <summary>Test-only seam — injects a stub transport for the SSE client.</summary>
    internal OrkesAgentClient(Configuration configuration, HttpMessageHandler? sseHandler)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _sseClient = sseHandler is null
            ? new HttpClient { Timeout = TimeSpan.FromMinutes(10) }
            : new HttpClient(sseHandler) { Timeout = TimeSpan.FromMinutes(10) };
    }

    /// <summary>Cron-schedule lifecycle API (save/list/pause/resume/delete/runNow/preview/reconcile).</summary>
    public Schedules Schedules => _schedules ??= new Schedules(_configuration);

    // ── Run / start / deploy / schedule (agent-level, control-plane) ──────────

    public async Task<AgentResult> RunAsync(
        Agent agent, string prompt, string? sessionId = null,
        IEnumerable<string>? media = null, Plans.Plan? plan = null,
        RunSettings? runSettings = null, CancellationToken ct = default)
    {
        var handle = await StartAsync(agent, prompt, sessionId, media, plan, runSettings, ct);
        return await handle.WaitAsync(ct);
    }

    public async Task<AgentHandle> StartAsync(
        Agent agent, string prompt, string? sessionId = null,
        IEnumerable<string>? media = null, Plans.Plan? plan = null,
        RunSettings? runSettings = null, CancellationToken ct = default)
    {
        var payload = AgentConfigSerializer.Serialize(agent, prompt, sessionId ?? "", media);
        runSettings?.ApplyToPayload(payload);
        if (plan is not null) payload["static_plan"] = plan.ToJson();
        var executionId = await StartAsync(payload, ct);
        return new AgentHandle(executionId, this);
    }

    public async Task<DeploymentInfo[]> DeployAsync(params Agent[] agents)
    {
        var results = new DeploymentInfo[agents.Length];
        for (int i = 0; i < agents.Length; i++)
        {
            var cfg = AgentConfigSerializer.SerializeAgent(agents[i]);
            var registeredName = await DeployAsync(cfg);
            results[i] = new DeploymentInfo(RegisteredName: registeredName, AgentName: agents[i].Name);
        }
        return results;
    }

    public async Task<DeploymentInfo> ScheduleAsync(
        Agent agent, IEnumerable<Schedule> schedules, CancellationToken ct = default)
    {
        var info = (await DeployAsync(agent))[0];
        await Schedules.ReconcileAsync(agent.Name, schedules, ct);
        return info;
    }

    // ── Agent API ───────────────────────────────────────────

    public async Task<string> StartAsync(JsonObject payload, CancellationToken ct = default)
    {
        var node = await ExecuteJsonAsync(Method.Post, "/agent/start", payload, ct);
        return node?["executionId"]?.GetValue<string>()
            ?? throw new AgentApiException(200, "No executionId in start response");
    }

    /// <summary>Deploy (register) an agent on the server without starting execution.</summary>
    public async Task<string> DeployAsync(JsonObject agentConfig, CancellationToken ct = default)
    {
        var node = await ExecuteJsonAsync(Method.Post, "/agent/deploy", FrameworkAwarePayload(agentConfig), ct);
        return node?["agentName"]?.GetValue<string>() ?? "";
    }

    /// <summary>Compile an agent to a Conductor WorkflowDef without executing it.</summary>
    public async Task<JsonNode?> CompileAsync(JsonObject agentConfig, CancellationToken ct = default)
        => await ExecuteJsonAsync(Method.Post, "/agent/compile", FrameworkAwarePayload(agentConfig), ct);

    public async Task<JsonNode?> GetStatusAsync(string executionId, CancellationToken ct = default)
        => await ExecuteJsonAsync(Method.Get, $"/agent/{Uri.EscapeDataString(executionId)}/status", null, ct);

    /// <summary>Fetch the full execution record (includes tokenUsage, finishReason). Enrichment read — null on any failure.</summary>
    public async Task<JsonNode?> GetExecutionAsync(string executionId, CancellationToken ct = default)
        => await ExecuteJsonAsync(
            Method.Get, $"/agent/execution/{Uri.EscapeDataString(executionId)}", null, ct, nullOnFailure: true);

    public async Task<JsonNode?> ListExecutionsAsync(
        IReadOnlyDictionary<string, string>? queryParams = null, CancellationToken ct = default)
    {
        var query = new List<KeyValuePair<string, string>>();
        if (queryParams is not null)
            foreach (var (key, value) in queryParams)
                query.Add(new KeyValuePair<string, string>(key, value));
        return await ExecuteJsonAsync(Method.Get, "/agent/executions", null, ct, queryParams: query);
    }

    public async Task RespondAsync(string executionId, object body, CancellationToken ct = default)
        => await ExecuteJsonAsync(
            Method.Post, $"/agent/{Uri.EscapeDataString(executionId)}/respond", ToJsonNode(body), ct);

    /// <summary>Push a message into a running agent's Workflow Message Queue.</summary>
    public async Task SendWorkflowMessageAsync(string executionId, object message, CancellationToken ct = default)
    {
        var payload = message is string s
            ? new JsonObject { ["message"] = s }
            : ToJsonNode(message) as JsonObject ?? new JsonObject { ["message"] = ToJsonNode(message) };
        await ExecuteJsonAsync(
            Method.Post, $"/workflow/{Uri.EscapeDataString(executionId)}/messages", payload, ct);
    }

    /// <summary>Send a signal message to a running agent execution (<c>POST /agent/{id}/signal</c>).</summary>
    public async Task SignalAsync(string executionId, object message, CancellationToken ct = default)
    {
        var payload = new JsonObject { ["message"] = ToJsonNode(message) };
        await ExecuteJsonAsync(Method.Post, $"/agent/{Uri.EscapeDataString(executionId)}/signal", payload, ct);
    }

    /// <summary>Gracefully stop an agent — sets _stop_requested and unblocks WMQ waits.</summary>
    public async Task StopAgentAsync(string executionId, CancellationToken ct = default)
    {
        // Signal the agent to stop. Best-effort — ignore failures (agent may have already completed).
        try { await ExecuteJsonAsync(Method.Post, $"/agent/{Uri.EscapeDataString(executionId)}/stop", new JsonObject(), ct); }
        catch { /* ignore */ }

        // Also unblock any blocking PULL_WORKFLOW_MESSAGES wait.
        try
        {
            await SendWorkflowMessageAsync(executionId, new Dictionary<string, object> { ["_signal"] = "stop" }, ct);
        }
        catch { /* ignore — WMQ may not be enabled */ }
    }

    /// <summary>Immediately cancel an agent execution (TERMINATED status).</summary>
    public async Task CancelAgentAsync(string executionId, string reason = "", CancellationToken ct = default)
    {
        var query = string.IsNullOrEmpty(reason)
            ? new List<KeyValuePair<string, string>>()
            : new List<KeyValuePair<string, string>> { new("reason", reason) };
        try
        {
            await ExecuteJsonAsync(
                Method.Delete, $"/workflow/{Uri.EscapeDataString(executionId)}", null, ct, queryParams: query);
        }
        catch (Exception ex)
        {
            // Best-effort, but a failed cancel means a still-running (billable) execution.
            System.Diagnostics.Trace.TraceWarning(
                $"CancelAgentAsync({executionId}) failed: {ex.Message}; execution may still be running.");
        }
    }

    /// <summary>Pause a running workflow execution — tasks stop being scheduled.</summary>
    public async Task PauseAgentAsync(string executionId, CancellationToken ct = default)
        => await ExecuteJsonAsync(Method.Put, $"/workflow/{Uri.EscapeDataString(executionId)}/pause", null, ct);

    /// <summary>Resume ("unpause") a previously paused workflow execution.</summary>
    public async Task UnpauseAgentAsync(string executionId, CancellationToken ct = default)
        => await ExecuteJsonAsync(Method.Put, $"/workflow/{Uri.EscapeDataString(executionId)}/resume", null, ct);

    /// <summary>Fetch the workflow definition (without tasks) — e.g. to read taskToDomain. Enrichment read — null on any failure.</summary>
    public async Task<JsonNode?> GetWorkflowAsync(string executionId, CancellationToken ct = default)
        => await ExecuteJsonAsync(
            Method.Get, $"/workflow/{Uri.EscapeDataString(executionId)}", null, ct,
            queryParams: new List<KeyValuePair<string, string>> { new("includeTasks", "false") },
            nullOnFailure: true);

    /// <summary>Fetch the workflow with its tasks — used to aggregate tool calls from call_* tasks. Enrichment read — null on any failure.</summary>
    public async Task<JsonNode?> GetWorkflowWithTasksAsync(string executionId, CancellationToken ct = default)
        => await ExecuteJsonAsync(
            Method.Get, $"/workflow/{Uri.EscapeDataString(executionId)}", null, ct,
            queryParams: new List<KeyValuePair<string, string>> { new("includeTasks", "true") },
            nullOnFailure: true);

    // ── Run by name ──────────────────────────────────────────

    /// <summary>Start a pre-deployed workflow by name (no agentConfig payload).</summary>
    public async Task<string> StartWorkflowByNameAsync(
        string workflowName, string prompt, string sessionId = "", CancellationToken ct = default)
    {
        var input = new JsonObject
        {
            ["prompt"] = prompt,
            ["media"] = new JsonArray(),
            ["session_id"] = sessionId ?? "",
            ["context"] = new JsonObject(),
        };
        var payload = new JsonObject { ["name"] = workflowName, ["input"] = input };

        // This endpoint returns the execution id as a bare (non-JSON) string —
        // route through the raw call directly rather than ExecuteJsonAsync,
        // which always attempts JsonNode.Parse on the body.
        var (statusCode, body) = await AgentApiCall.InvokeAsync(_configuration, Method.Post, "/workflow", payload, ct);
        if (statusCode is < 200 or >= 300)
        {
            if (statusCode == 404) throw new AgentNotFoundException("/workflow");
            throw new AgentApiException(statusCode, body ?? "", body);
        }
        return (body ?? "").Trim('"', ' ', '\n', '\r');
    }

    // ── SSE streaming ───────────────────────────────────────

    private const int MaxReconnectAttempts = 5;

    public async IAsyncEnumerable<AgentEvent> StreamEventsAsync(
        string executionId, string? lastEventId = null,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var basePath = _configuration.BasePath.TrimEnd('/');
        var url = $"{basePath}/agent/stream/{Uri.EscapeDataString(executionId)}";
        string? eventId = lastEventId;
        int reconnectAttempt = 0;

        while (!ct.IsCancellationRequested)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.ParseAdd("text/event-stream");
            var token = _configuration.AccessToken;
            if (!string.IsNullOrEmpty(token))
                request.Headers.TryAddWithoutValidation("X-Authorization", token);
            if (!string.IsNullOrEmpty(eventId))
                request.Headers.TryAddWithoutValidation("Last-Event-ID", eventId);

            HttpResponseMessage resp;
            try
            {
                resp = await _sseClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                if (eventId is null) throw new SSEUnavailableException($"SSE connect failed: {ex.Message}", ex);
                if (++reconnectAttempt > MaxReconnectAttempts)
                    throw new SSEUnavailableException($"SSE reconnect failed after {MaxReconnectAttempts} attempts: {ex.Message}", ex);
                await Task.Delay(TimeSpan.FromSeconds(Math.Min(reconnectAttempt, 5)), ct);
                continue;
            }

            if (!resp.IsSuccessStatusCode)
            {
                if (eventId is null)
                    throw new SSEUnavailableException($"SSE connect failed: HTTP {(int)resp.StatusCode}");
                if (++reconnectAttempt > MaxReconnectAttempts)
                    throw new SSEUnavailableException($"SSE reconnect failed after {MaxReconnectAttempts} attempts: HTTP {(int)resp.StatusCode}");
                await Task.Delay(TimeSpan.FromSeconds(Math.Min(reconnectAttempt, 5)), ct);
                continue;
            }

            reconnectAttempt = 0;
            var sawAnyEvent = false;
            var streamEndedNaturally = false;

            await using (var stream = await resp.Content.ReadAsStreamAsync(ct))
            using (var reader = new StreamReader(stream))
            {
                string? eventType = null;
                var dataLines = new StringBuilder();

                while (!ct.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync(ct);
                    if (line is null) break; // end of stream — reconnect below unless we already yielded "done"

                    if (line.StartsWith(':')) continue; // heartbeat

                    if (line.StartsWith("event:"))
                    {
                        eventType = line["event:".Length..].Trim();
                    }
                    else if (line.StartsWith("id:"))
                    {
                        eventId = line["id:".Length..].Trim();
                    }
                    else if (line.StartsWith("data:"))
                    {
                        if (dataLines.Length > 0) dataLines.Append('\n');
                        dataLines.Append(line["data:".Length..].TrimStart());
                    }
                    else if (line.Length == 0 && dataLines.Length > 0)
                    {
                        var ev = ParseEvent(eventType, dataLines.ToString());
                        eventType = null;
                        dataLines.Clear();

                        if (ev is not null)
                        {
                            sawAnyEvent = true;
                            yield return ev;
                            if (ev.Type == EventType.Done)
                            {
                                streamEndedNaturally = true;
                                break;
                            }
                        }
                    }
                }
            }

            if (streamEndedNaturally || ct.IsCancellationRequested) yield break;

            // Mid-stream drop — reconnect with Last-Event-ID if we have one, else give up.
            if (eventId is null)
            {
                if (!sawAnyEvent) throw new SSEUnavailableException("SSE stream ended before any event was received.");
                yield break;
            }
            if (++reconnectAttempt > MaxReconnectAttempts)
                throw new SSEUnavailableException($"SSE reconnect failed after {MaxReconnectAttempts} attempts: stream kept dropping.");
            await Task.Delay(TimeSpan.FromSeconds(Math.Min(reconnectAttempt, 5)), ct);
        }
    }

    private static AgentEvent? ParseEvent(string? eventType, string data)
    {
        JsonNode? node = null;
        try { node = JsonNode.Parse(data); } catch { /* skip malformed */ }

        return eventType switch
        {
            "thinking" => new AgentEvent
            {
                Type = EventType.Thinking,
                Content = node?["content"]?.GetValue<string>(),
            },
            "tool_call" => new AgentEvent
            {
                Type = EventType.ToolCall,
                ToolName = node?["toolName"]?.GetValue<string>(),
            },
            "tool_result" => new AgentEvent
            {
                Type = EventType.ToolResult,
                ToolName = node?["toolName"]?.GetValue<string>(),
            },
            "guardrail_pass" => new AgentEvent
            {
                Type = EventType.GuardrailPass,
                GuardrailName = node?["guardrailName"]?.GetValue<string>(),
            },
            "guardrail_fail" => new AgentEvent
            {
                Type = EventType.GuardrailFail,
                GuardrailName = node?["guardrailName"]?.GetValue<string>(),
                Content = node?["message"]?.GetValue<string>(),
            },
            "waiting" => new AgentEvent { Type = EventType.Waiting },
            "handoff" => new AgentEvent
            {
                Type = EventType.Handoff,
                Target = node?["target"]?.GetValue<string>(),
            },
            "done" => new AgentEvent
            {
                Type = EventType.Done,
                Status = node?["output"]?["finishReason"]?.GetValue<string>()
                       ?? node?["status"]?.GetValue<string>(),
                Content = ExtractOutputText(node?["output"]),
            },
            "error" => new AgentEvent
            {
                Type = EventType.Error,
                Content = node?["message"]?.GetValue<string>(),
            },
            _ => null,
        };
    }

    private static string? ExtractOutputText(JsonNode? output)
    {
        if (output is null) return null;
        if (output is JsonObject obj && obj.TryGetPropertyValue("result", out var r))
            return r?.GetValue<string>();
        if (output is JsonValue v)
        {
            try { return v.GetValue<string>(); } catch { return null; }
        }
        return null;
    }

    // ── Shared call path (single token authority) ────────────

    /// <summary>
    /// Every non-streaming op funnels through here → <see cref="Client.ApiClient.ExecuteAsync{T}"/>
    /// on the shared <see cref="Configuration"/> — same auth injection + 401
    /// refresh-retry as every generated resource API. One response handler:
    /// non-2xx → <see cref="AgentApiException"/> (or <see cref="AgentNotFoundException"/>
    /// on 404), unless <paramref name="nullOnFailure"/> (the two enrichment reads).
    /// </summary>
    private async Task<JsonNode?> ExecuteJsonAsync(
        Method method, string path, JsonNode? body, CancellationToken ct,
        List<KeyValuePair<string, string>>? queryParams = null, bool nullOnFailure = false)
    {
        (int StatusCode, string? Body) result;
        try
        {
            result = await AgentApiCall.InvokeAsync(_configuration, method, path, body, ct, queryParams);
        }
        catch (Exception) when (nullOnFailure)
        {
            return null;
        }

        if (result.StatusCode is >= 200 and < 300)
            return string.IsNullOrWhiteSpace(result.Body) ? null : JsonNode.Parse(result.Body);

        if (nullOnFailure) return null;

        if (result.StatusCode == 404)
            throw new AgentNotFoundException(path);

        throw new AgentApiException(result.StatusCode, result.Body ?? "", result.Body);
    }

    private static JsonNode? ToJsonNode(object value) => value switch
    {
        JsonNode node => node,
        string s => JsonValue.Create(s),
        _ => JsonSerializer.SerializeToNode(value, AgentspanJson.Options),
    };

    private static JsonObject FrameworkAwarePayload(JsonObject agentConfig)
    {
        var framework = agentConfig["_framework"]?.GetValue<string>();
        if (!string.IsNullOrEmpty(framework))
        {
            return new JsonObject
            {
                ["framework"] = framework,
                ["rawConfig"] = agentConfig.DeepClone(),
            };
        }

        return new JsonObject { ["agentConfig"] = agentConfig };
    }

    // ── Disposal ─────────────────────────────────────────────
    // Only the SSE transport is privately owned — Configuration/ApiClient are
    // shared with the caller and are never disposed here.

    public void Dispose() => _sseClient.Dispose();

    public ValueTask DisposeAsync()
    {
        _sseClient.Dispose();
        return ValueTask.CompletedTask;
    }
}
