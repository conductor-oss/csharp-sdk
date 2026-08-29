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
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Conductor.AI;

// ── Enums ──────────────────────────────────────────────────

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EventType
{
    [JsonPropertyName("thinking")] Thinking,
    [JsonPropertyName("tool_call")] ToolCall,
    [JsonPropertyName("tool_result")] ToolResult,
    [JsonPropertyName("guardrail_pass")] GuardrailPass,
    [JsonPropertyName("guardrail_fail")] GuardrailFail,
    [JsonPropertyName("waiting")] Waiting,
    [JsonPropertyName("handoff")] Handoff,
    [JsonPropertyName("message")] Message,
    [JsonPropertyName("error")] Error,
    [JsonPropertyName("done")] Done,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Status
{
    [JsonPropertyName("completed")] Completed,
    [JsonPropertyName("failed")] Failed,
    [JsonPropertyName("terminated")] Terminated,
    [JsonPropertyName("timed_out")] TimedOut,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FinishReason
{
    [JsonPropertyName("stop")] Stop,
    [JsonPropertyName("length")] Length,
    [JsonPropertyName("tool_calls")] ToolCalls,
    [JsonPropertyName("error")] Error,
    [JsonPropertyName("cancelled")] Cancelled,
    [JsonPropertyName("timeout")] Timeout,
    [JsonPropertyName("guardrail")] Guardrail,
    [JsonPropertyName("rejected")] Rejected,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OnFail
{
    [JsonPropertyName("retry")] Retry,
    [JsonPropertyName("raise")] Raise,
    [JsonPropertyName("fix")] Fix,
    [JsonPropertyName("human")] Human,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Position
{
    [JsonPropertyName("input")] Input,
    [JsonPropertyName("output")] Output,
}

// ── Value records ──────────────────────────────────────────

public record TokenUsage(
    [property: JsonPropertyName("promptTokens")] int PromptTokens,
    [property: JsonPropertyName("completionTokens")] int CompletionTokens,
    [property: JsonPropertyName("totalTokens")] int TotalTokens
);

public record DeploymentInfo(
    [property: JsonPropertyName("registeredName")] string RegisteredName,
    [property: JsonPropertyName("agentName")] string AgentName
);

public record CredentialFile(
    string EnvVar,
    string? RelativePath = null,
    string? Content = null
);

public record CodeExecutionConfig(
    bool Enabled = true,
    List<string>? AllowedLanguages = null,
    List<string>? AllowedCommands = null,
    int Timeout = 30
);

public record CliConfig(
    bool Enabled = true,
    List<string>? AllowedCommands = null,
    int Timeout = 30,
    bool AllowShell = false,
    string? WorkingDir = null
);

public record ExecutionResult(
    string Output,
    string? Error = null,
    int ExitCode = 0,
    bool TimedOut = false
)
{
    [JsonIgnore] public bool Success => ExitCode == 0 && !TimedOut;
}

public record GuardrailResult(
    bool Passed,
    string? Message = null,
    string? FixedOutput = null
);

// ── AgentEvent ─────────────────────────────────────────────

public record AgentEvent
{
    [JsonPropertyName("type")] public EventType Type { get; init; }
    [JsonPropertyName("content")] public string? Content { get; init; }
    [JsonPropertyName("toolName")] public string? ToolName { get; init; }
    [JsonPropertyName("args")] public Dictionary<string, object>? Args { get; init; }
    [JsonPropertyName("result")] public object? Result { get; init; }
    [JsonPropertyName("target")] public string? Target { get; init; }
    [JsonPropertyName("output")] public object? Output { get; init; }
    [JsonPropertyName("executionId")] public string? ExecutionId { get; init; }
    [JsonPropertyName("guardrailName")] public string? GuardrailName { get; init; }
    [JsonPropertyName("timestamp")] public long? Timestamp { get; init; }
    [JsonPropertyName("status")] public string? Status { get; init; }
}

// ── AgentResult ────────────────────────────────────────────

public record AgentResult
{
    [JsonPropertyName("executionId")] public string ExecutionId { get; init; } = "";
    [JsonPropertyName("correlationId")] public string? CorrelationId { get; init; }
    [JsonPropertyName("output")] public Dictionary<string, object>? Output { get; init; }
    [JsonPropertyName("messages")] public List<Dictionary<string, object>>? Messages { get; init; }
    [JsonPropertyName("toolCalls")] public List<Dictionary<string, object>>? ToolCalls { get; init; }
    [JsonPropertyName("status")] public Status Status { get; init; }
    [JsonPropertyName("finishReason")] public FinishReason? FinishReason { get; init; }
    [JsonPropertyName("error")] public string? Error { get; init; }
    [JsonPropertyName("tokenUsage")] public TokenUsage? TokenUsage { get; init; }
    [JsonPropertyName("metadata")] public Dictionary<string, object>? Metadata { get; init; }
    [JsonPropertyName("events")] public List<AgentEvent>? Events { get; init; }
    [JsonPropertyName("subResults")] public Dictionary<string, object>? SubResults { get; init; }

    // Convenience properties
    [JsonIgnore] public bool IsSuccess => Status == Status.Completed;
    [JsonIgnore] public bool IsFailed => Status == Status.Failed;
    [JsonIgnore] public bool IsRejected => FinishReason == Conductor.AI.FinishReason.Rejected;

    /// <summary>Print a formatted summary of the result, mirroring Python's print_result().</summary>
    public void PrintResult()
    {
        const int width = 50;
        var border = new string('═', width);
        Console.WriteLine($"\n╒{border}╕");
        Console.WriteLine($"│ {"Agent Output".PadRight(width - 1)}│");
        Console.WriteLine($"╘{border}╛");
        Console.WriteLine();

        if (IsFailed && Error is not null)
        {
            Console.WriteLine($"ERROR: {Error}");
            Console.WriteLine();
        }
        else if (Output is not null)
        {
            if (Output.TryGetValue("result", out var result) && result is not null)
            {
                Console.WriteLine(result);
                Console.WriteLine();
            }
            else
            {
                foreach (var (key, value) in Output)
                {
                    Console.WriteLine($"--- {key} ---");
                    Console.WriteLine(value);
                    Console.WriteLine();
                }
            }
        }

        if (TokenUsage is not null)
            Console.WriteLine($"Tokens: {TokenUsage.TotalTokens} total ({TokenUsage.PromptTokens} prompt, {TokenUsage.CompletionTokens} completion)");
        else
            Console.WriteLine("Tokens: —");

        if (FinishReason.HasValue)
            Console.WriteLine($"Finish reason: FinishReason.{FinishReason.Value}");

        if (!string.IsNullOrEmpty(ExecutionId))
            Console.WriteLine($"Execution ID: {ExecutionId}");

        Console.WriteLine();
    }
}

// ── AgentStatus ────────────────────────────────────────────

public record AgentStatus
{
    [JsonPropertyName("executionId")] public string ExecutionId { get; init; } = "";
    [JsonPropertyName("isComplete")] public bool IsComplete { get; init; }
    [JsonPropertyName("isRunning")] public bool IsRunning { get; init; }
    [JsonPropertyName("isWaiting")] public bool IsWaiting { get; init; }
    [JsonPropertyName("output")] public object? Output { get; init; }
    [JsonPropertyName("status")] public string? StatusValue { get; init; }
    [JsonPropertyName("reason")] public string? Reason { get; init; }
    [JsonPropertyName("currentTask")] public string? CurrentTask { get; init; }
    [JsonPropertyName("pendingTool")] public Dictionary<string, object>? PendingTool { get; init; }
    [JsonPropertyName("tokenUsage")] public TokenUsage? TokenUsage { get; init; }
}

// ── AgentHandle ────────────────────────────────────────────

public sealed class AgentHandle
{
    /// <summary>Overall WaitAsync deadline (Java parity) — bounds a run that never reaches a terminal state.</summary>
    private static readonly TimeSpan DefaultWaitTimeout = TimeSpan.FromMinutes(10);

    /// <summary>Consecutive transient GetStatus failures tolerated before WaitAsync gives up and rethrows.</summary>
    private const int MaxConsecutiveStatusErrors = 3;

    private readonly string _executionId;
    private readonly IAgentClient _http;
    private readonly string? _runId;
    private readonly bool _streamingEnabled;
    private readonly ServerLivenessMonitor? _livenessMonitor;

    internal AgentHandle(
        string executionId, IAgentClient http, string? runId = null,
        bool streamingEnabled = true, ServerLivenessMonitor? livenessMonitor = null)
    {
        _executionId = executionId;
        _http = http;
        _runId = runId;
        _streamingEnabled = streamingEnabled;
        _livenessMonitor = livenessMonitor;
    }

    public string ExecutionId => _executionId;

    /// <summary>Test-only seam — whether a liveness monitor (spec R11) is attached to this handle.</summary>
    internal bool HasLivenessMonitor => _livenessMonitor is not null;

    /// <summary>
    /// The domain UUID used for domain-based routing (stateful agents).
    /// Set when the agent was started with <see cref="Agent.Stateful"/> = true,
    /// or when resuming an existing execution via <see cref="AgentRuntime.ResumeAsync"/>.
    /// </summary>
    public string? RunId => _runId;

    /// <summary>
    /// Poll until the agent completes, then return the result. Bounded by a
    /// 10-minute overall deadline and tolerates up to
    /// <see cref="MaxConsecutiveStatusErrors"/> consecutive transient status-poll
    /// failures before giving up. For stateful runs with liveness monitoring
    /// enabled (<see cref="AgentConfig.LivenessEnabled"/>), throws
    /// <see cref="WorkerStallException"/> as soon as the monitor detects an
    /// unpolled task instead of waiting out the full deadline.
    /// </summary>
    public async Task<AgentResult> WaitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var deadline = DateTime.UtcNow + DefaultWaitTimeout;
            var consecutiveErrors = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_livenessMonitor?.StalledTaskRef is { } stalledRef)
                    throw new WorkerStallException(stalledRef, _executionId);

                if (DateTime.UtcNow >= deadline)
                    throw new TimeoutException(
                        $"Timed out after {DefaultWaitTimeout.TotalMinutes:0}m waiting for execution '{_executionId}' to complete.");

                JsonNode? status;
                try
                {
                    status = await _http.GetStatusAsync(_executionId, cancellationToken);
                    consecutiveErrors = 0;
                }
                catch when (++consecutiveErrors <= MaxConsecutiveStatusErrors)
                {
                    await Task.Delay(500, cancellationToken);
                    continue;
                }

                var s = status?["status"]?.GetValue<string>() ?? "";
                if (s is "COMPLETED" or "FAILED" or "TERMINATED" or "TIMED_OUT")
                {
                    // Fetch full execution record for token usage and finish reason
                    var execution = await _http.GetExecutionAsync(_executionId, cancellationToken);
                    // Walk the workflow's tasks to recover the tool calls and the
                    // events the run produced (enrichment read — null is fine,
                    // and yields a result carrying neither).
                    var workflowWithTasks = await _http.GetWorkflowWithTasksAsync(_executionId, cancellationToken);
                    return BuildResult(status!, s, execution, workflowWithTasks);
                }
                await Task.Delay(500, cancellationToken);
            }
            throw new OperationCanceledException();
        }
        finally
        {
            if (_livenessMonitor is not null)
                await _livenessMonitor.DisposeAsync();
        }
    }

    /// <summary>
    /// Stream events as they arrive over SSE. When streaming is disabled by
    /// config (<see cref="AgentConfig.StreamingEnabled"/> = false) or the server
    /// rejects the SSE connection (<see cref="SSEUnavailableException"/>), degrades
    /// to status polling and yields a single terminal <see cref="EventType.Done"/>
    /// event once the execution completes instead of ending silently.
    /// </summary>
    public async IAsyncEnumerable<AgentEvent> StreamAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_streamingEnabled)
        {
            await using var enumerator = _http.StreamEventsAsync(_executionId, lastEventId: null, ct: cancellationToken)
                .GetAsyncEnumerator(cancellationToken);
            while (true)
            {
                bool moved;
                try { moved = await enumerator.MoveNextAsync(); }
                catch (SSEUnavailableException) { break; } // fall through to the polling fallback below
                if (!moved) yield break; // natural end of a real SSE stream (a Done event was already yielded)
                yield return enumerator.Current;
            }
        }

        // Streaming disabled, or the SSE connection failed — degrade to status
        // polling and synthesize a terminal Done event from the final result.
        var result = await WaitAsync(cancellationToken);
        yield return new AgentEvent
        {
            Type = EventType.Done,
            ExecutionId = result.ExecutionId,
            Status = result.FinishReason?.ToString(),
            Content = result.Output is { } output && output.TryGetValue("result", out var r) ? r?.ToString() : null,
        };
    }

    /// <summary>Check the current status without blocking.</summary>
    public async Task<AgentStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var node = await _http.GetStatusAsync(_executionId, cancellationToken);
        if (node is null) return new AgentStatus { ExecutionId = _executionId };

        var statusValue = node["status"]?.GetValue<string>();
        return new AgentStatus
        {
            ExecutionId = node["executionId"]?.GetValue<string>() ?? _executionId,
            IsComplete = node["isComplete"]?.GetValue<bool>() ?? false,
            IsRunning = node["isRunning"]?.GetValue<bool>() ?? false,
            IsWaiting = node["isWaiting"]?.GetValue<bool>() ?? false,
            Output = node["output"] is JsonObject outObj
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(outObj.ToJsonString(), ConductorAgentJson.Options)
                : null,
            StatusValue = statusValue,
            Reason = statusValue != "COMPLETED" ? node["reasonForIncompletion"]?.GetValue<string>() : null,
            CurrentTask = node["currentTask"]?.GetValue<string>(),
        };
    }

    public async Task RespondAsync(object response, CancellationToken cancellationToken = default)
        => await _http.RespondAsync(_executionId, response, cancellationToken);

    public async Task ApproveAsync(CancellationToken cancellationToken = default)
        => await _http.RespondAsync(_executionId, new { approved = true }, cancellationToken);

    /// <summary>Approve the waiting HITL task with a comment reason.</summary>
    public async Task ApproveAsync(string comment, CancellationToken cancellationToken = default)
        => await _http.RespondAsync(_executionId, new { approved = true, reason = comment }, cancellationToken);

    public async Task RejectAsync(string? reason = null, CancellationToken cancellationToken = default)
        => await _http.RespondAsync(_executionId, new { approved = false, reason }, cancellationToken);

    // ── Event-targeted HITL ──────────────────────────────────
    // Under HANDOFF/SEQUENTIAL/PARALLEL strategies the HUMAN task lives in a
    // sub-execution. Pass the WAITING event so the response targets that event's
    // executionId rather than the root. Mirrors Java's AgentStream.approve(event).

    /// <summary>Approve the HITL task that emitted the given WAITING event (targets its sub-execution).</summary>
    public async Task ApproveAsync(AgentEvent waitingEvent, string? comment = null, CancellationToken cancellationToken = default)
        => await _http.RespondAsync(EventExecId(waitingEvent),
            comment is null ? new { approved = true } : new { approved = true, reason = comment }, cancellationToken);

    /// <summary>Reject the HITL task that emitted the given WAITING event (targets its sub-execution).</summary>
    public async Task RejectAsync(AgentEvent waitingEvent, string reason, CancellationToken cancellationToken = default)
        => await _http.RespondAsync(EventExecId(waitingEvent), new { approved = false, reason }, cancellationToken);

    /// <summary>Send an arbitrary structured response to the execution that emitted the given event.</summary>
    public async Task RespondAsync(AgentEvent waitingEvent, object response, CancellationToken cancellationToken = default)
        => await _http.RespondAsync(EventExecId(waitingEvent), response, cancellationToken);

    private string EventExecId(AgentEvent e) => e.ExecutionId ?? _executionId;

    // ── Waiting helpers ──────────────────────────────────────

    /// <summary>True if the execution is currently paused for human input. Swallows transient errors.</summary>
    public async Task<bool> IsWaitingAsync(CancellationToken cancellationToken = default)
    {
        try { return (await GetStatusAsync(cancellationToken)).IsWaiting; }
        catch { return false; }
    }

    /// <summary>
    /// Poll until the execution pauses for human input (returns true) or reaches a
    /// terminal state (returns false). Returns false on timeout.
    /// </summary>
    public async Task<bool> WaitUntilWaitingAsync(
        TimeSpan timeout, TimeSpan? pollInterval = null, CancellationToken cancellationToken = default)
    {
        var poll = pollInterval ?? TimeSpan.FromMilliseconds(500);
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline && !cancellationToken.IsCancellationRequested)
        {
            var status = await GetStatusAsync(cancellationToken);
            if (status.IsWaiting) return true;
            if (status.IsComplete) return false;
            if (status.StatusValue is "COMPLETED" or "FAILED" or "TERMINATED" or "TIMED_OUT") return false;
            await Task.Delay(poll, cancellationToken);
        }
        return false;
    }

    /// <summary>
    /// Gracefully stop the agent execution. Sets _stop_requested to true — the
    /// agent's loop exits after the current iteration completes. Status → COMPLETED.
    /// Also unblocks any blocking WaitForMessage calls.
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken = default)
        => await _http.StopAgentAsync(_executionId, cancellationToken);

    /// <summary>Gracefully stop the agent execution (synchronous).</summary>
    public void Stop() => StopAsync().GetAwaiter().GetResult();

    /// <summary>
    /// Immediately cancel the agent execution. Status → TERMINATED.
    /// For graceful stop, use <see cref="StopAsync"/> instead.
    /// </summary>
    public async Task CancelAsync(string reason = "", CancellationToken cancellationToken = default)
        => await _http.CancelAgentAsync(_executionId, reason, cancellationToken);

    /// <summary>Immediately cancel the agent execution (synchronous).</summary>
    public void Cancel(string reason = "") => CancelAsync(reason).GetAwaiter().GetResult();

    /// <summary>Pause the execution — tasks stop being scheduled until <see cref="UnpauseAsync"/>.</summary>
    public async Task PauseAsync(CancellationToken cancellationToken = default)
        => await _http.PauseAgentAsync(_executionId, cancellationToken);

    /// <summary>Pause the execution (synchronous).</summary>
    public void Pause() => PauseAsync().GetAwaiter().GetResult();

    /// <summary>
    /// Resume a paused execution. Not to be confused with
    /// <see cref="AgentRuntime.Resume(string, Agent)"/>, which re-attaches a
    /// runtime to an existing execution and re-registers its tool workers.
    /// </summary>
    public async Task UnpauseAsync(CancellationToken cancellationToken = default)
        => await _http.UnpauseAgentAsync(_executionId, cancellationToken);

    /// <summary>Resume a paused execution (synchronous).</summary>
    public void Unpause() => UnpauseAsync().GetAwaiter().GetResult();

    /// <summary>Send a signal message to this execution (<c>POST /agent/{id}/signal</c>).</summary>
    public async Task SignalAsync(object message, CancellationToken cancellationToken = default)
        => await _http.SignalAsync(_executionId, message, cancellationToken);

    /// <summary>Send a signal message to this execution (synchronous).</summary>
    public void Signal(object message) => SignalAsync(message).GetAwaiter().GetResult();

    private static AgentResult BuildResult(
        JsonNode status, string statusStr, JsonNode? execution = null, JsonNode? workflowWithTasks = null)
    {
        var output = status["output"];
        var parsedStatus = statusStr switch
        {
            "COMPLETED" => Status.Completed,
            "FAILED" => Status.Failed,
            "TERMINATED" => Status.Terminated,
            "TIMED_OUT" => Status.TimedOut,
            _ => Status.Completed,
        };

        Dictionary<string, object>? outputDict = null;
        if (output is JsonObject obj)
        {
            outputDict = JsonSerializer.Deserialize<Dictionary<string, object>>(
                obj.ToJsonString(), ConductorAgentJson.Options);
        }
        else if (output is not null)
        {
            outputDict = new Dictionary<string, object> { ["result"] = output.ToString() };
        }

        // Extract token usage from the full execution record
        TokenUsage? tokenUsage = null;
        if (execution?["tokenUsage"] is JsonObject tuObj)
        {
            tokenUsage = new TokenUsage(
                PromptTokens: tuObj["promptTokens"]?.GetValue<int>() ?? 0,
                CompletionTokens: tuObj["completionTokens"]?.GetValue<int>() ?? 0,
                TotalTokens: tuObj["totalTokens"]?.GetValue<int>() ?? 0
            );
        }

        // Extract finish reason from output (server puts it in output.finishReason)
        FinishReason? finishReason = null;
        var frStr = output?["finishReason"]?.GetValue<string>()?.ToUpperInvariant();
        finishReason = frStr switch
        {
            "STOP" => Conductor.AI.FinishReason.Stop,
            "LENGTH" => Conductor.AI.FinishReason.Length,
            "TOOL_CALL" or "TOOL_CALLS" => Conductor.AI.FinishReason.ToolCalls,
            "ERROR" => Conductor.AI.FinishReason.Error,
            "GUARDRAIL" => Conductor.AI.FinishReason.Guardrail,
            "REJECTED" => Conductor.AI.FinishReason.Rejected,
            _ => null,
        };

        var executionId = status["executionId"]?.GetValue<string>() ?? "";
        var error = parsedStatus != Status.Completed
            ? status["reasonForIncompletion"]?.GetValue<string>()
            : null;

        var (toolCalls, events) = ExtractToolActivity(workflowWithTasks, executionId);
        events.Add(parsedStatus == Status.Completed
            ? new AgentEvent { Type = EventType.Done, ExecutionId = executionId, Output = outputDict }
            : new AgentEvent { Type = EventType.Error, ExecutionId = executionId, Content = error });

        return new AgentResult
        {
            ExecutionId = executionId,
            Status = parsedStatus,
            Output = outputDict,
            Error = error,
            ToolCalls = toolCalls,
            TokenUsage = tokenUsage,
            FinishReason = finishReason,
            Events = events,
        };
    }

    // ── Tool-call extraction ─────────────────────────────────────────
    //
    // A tool task is identified by its Conductor task type, allowlisted off the
    // server's ToolCompiler.TYPE_MAP — for the types the agent layer also uses
    // for its own structure, and for the worker kind, corroborated by a dispatch
    // marker. It is never identified by its reference task name: the server
    // seeds that from the provider's tool-call id (OpenAI's `call_...`,
    // Anthropic's `toolu_...`, else a UUID) and appends the fork index and loop
    // iteration, so a prefix test only ever matches one provider.
    //
    // The tool's real name is likewise never the task type. Conductor sets an
    // executed SIMPLE task's type to the task's own name, which is the tool
    // name for a worker tool and is why that one kind used to read correctly;
    // every other kind carries its system task type there instead.

    /// <summary>
    /// Task types only a tool compiles to, from the server's
    /// <c>ToolCompiler.TYPE_MAP</c> — plus <c>GENERATE_PDF</c>, which that map
    /// omits although the server compiles a <c>generate_pdf</c> tool to it.
    /// <c>SIMPLE</c> is absent deliberately: a worker tool's executed task type
    /// is the tool's own name, so that kind is recognised by the dispatch
    /// markers below rather than by type.
    /// </summary>
    private static readonly HashSet<string> ToolTaskTypes = new(StringComparer.Ordinal)
    {
        "HTTP", "CALL_MCP_TOOL",
        "GENERATE_IMAGE", "GENERATE_AUDIO", "GENERATE_VIDEO", "GENERATE_PDF",
        "LLM_INDEX_TEXT", "LLM_SEARCH_INDEX", "PULL_WORKFLOW_MESSAGES",
    };

    /// <summary>
    /// Task types a tool compiles to that the agent layer also uses for its own
    /// structure — <c>SUB_WORKFLOW</c> for a sub-agent, a strategy workflow, a
    /// router and a plan execution; <c>HUMAN</c> for a plan's approval step.
    /// The type alone therefore proves nothing, and a dispatch marker is
    /// required as well.
    /// </summary>
    private static readonly HashSet<string> AmbiguousToolTaskTypes = new(StringComparer.Ordinal)
    {
        "SUB_WORKFLOW", "HUMAN",
    };

    /// <summary>
    /// Input keys the server's tool-dispatch script injects.
    /// <c>_agent_tool_name</c> is set for every tool kind on the static
    /// dispatch path; <c>_agent_state</c> is set for worker tools on both the
    /// static and the dynamic-tools path.
    /// </summary>
    private const string AgentToolNameKey = "_agent_tool_name";
    private const string AgentStateKey = "_agent_state";

    /// <summary>The dispatch method name — the tool name on the dynamic-tools path.</summary>
    private const string MethodKey = "method";

    /// <summary>
    /// Whether a task is an LLM-dispatched tool call: an unambiguous tool task
    /// type, or a type that needs corroborating evidence that the LLM dispatched
    /// it.
    ///
    /// Two cases need that evidence. The worker case, because a worker tool's
    /// task type is the tool's own name and so cannot be allowlisted; what marks
    /// it is Conductor setting an executed SIMPLE task's type to the task's own
    /// name, which is also its <c>taskDefName</c> — a signature no system task
    /// shares. And the ambiguous types above, which the agent layer emits for
    /// its own structure far more often than for a tool.
    ///
    /// Neither half of the test suffices alone: a multi-agent
    /// <c>SET_VARIABLE</c> task carries <c>_agent_state</c> without being a tool
    /// call, and a multi-agent handoff is a <c>SUB_WORKFLOW</c> without being
    /// one either.
    ///
    /// Accepted cost: the dynamic-tools dispatch path sets no marker on a
    /// non-worker tool, so an agent-as-tool or human tool dispatched that way is
    /// missed. That is the right way to be wrong — the alternative reports a
    /// fabricated tool call for every handoff in every multi-agent run, and a
    /// tool call that did not happen is worse than one that is absent.
    /// </summary>
    private static bool IsToolTask(JsonNode task)
    {
        var taskType = task["taskType"]?.GetValue<string>();
        if (taskType is null) return false;
        if (ToolTaskTypes.Contains(taskType)) return true;

        var needsMarker = AmbiguousToolTaskTypes.Contains(taskType)
            || taskType == task["taskDefName"]?.GetValue<string>();
        return needsMarker
            && task["inputData"] is JsonObject inputData
            && (inputData.ContainsKey(AgentToolNameKey) || inputData.ContainsKey(AgentStateKey));
    }

    /// <summary>
    /// Resolve a tool task's tool name: <c>inputData._agent_tool_name</c>, then
    /// <c>inputData.method</c> (the dynamic-tools dispatch path sets no
    /// <c>_agent_tool_name</c>), then <c>taskDefName</c>.
    /// </summary>
    private static string ResolveToolName(JsonNode task)
    {
        if (task["inputData"] is JsonObject inputData)
        {
            if (StringValue(inputData, AgentToolNameKey) is { } toolName) return toolName;
            if (StringValue(inputData, MethodKey) is { } method) return method;
        }
        return task["taskDefName"]?.GetValue<string>() ?? "";
    }

    private static string? StringValue(JsonObject obj, string key)
        => obj.TryGetPropertyValue(key, out var node) && node is JsonValue value
            && value.TryGetValue<string>(out var s) && !string.IsNullOrEmpty(s)
            ? s
            : null;

    /// <summary>The tool's arguments — the task's input with the runtime's own keys stripped.</summary>
    private static Dictionary<string, object> ToolArgs(JsonObject inputData)
    {
        var cleaned = new Dictionary<string, object>();
        foreach (var kv in inputData)
        {
            var k = kv.Key;
            if (k.StartsWith('_') || k == MethodKey || k is "evaluatorType" or "expression" or "ctx"
                or "workerTag" or "agentConfig")
                continue;
            cleaned[k] = JsonSerializer.Deserialize<object>(kv.Value?.ToJsonString() ?? "null", ConductorAgentJson.Options)!;
        }
        return cleaned;
    }

    /// <summary>
    /// The tool's result: the task's <c>result</c> output, or its whole output
    /// when there is no such key — an HTTP tool answers under <c>response</c>,
    /// so keying only on <c>result</c> would report no result at all for it.
    /// Matches the fallback the server's <c>AgentEventListener</c> applies.
    /// </summary>
    private static object? ToolResult(JsonObject outputData)
    {
        var node = outputData.TryGetPropertyValue("result", out var resultNode) && resultNode is not null
            ? resultNode
            : outputData;
        return JsonSerializer.Deserialize<object>(node.ToJsonString(), ConductorAgentJson.Options);
    }

    /// <summary>
    /// Walk the workflow's tasks once and recover, per LLM-dispatched tool call,
    /// both the <see cref="AgentResult.ToolCalls"/> entry (name, arguments,
    /// result) and the <see cref="EventType.ToolCall"/> /
    /// <see cref="EventType.ToolResult"/> event pair — two views of the same
    /// call, so they are built together and cannot drift apart.
    ///
    /// The returned event list carries the tool events only; the caller appends
    /// the terminal event. It is never null, so enumerating
    /// <see cref="AgentResult.Events"/> never throws. Tool calls stay null when
    /// there were none, as they always have.
    ///
    /// This is a reconstruction from completed tasks, not the stream
    /// <see cref="StreamAsync"/> delivers live: the server emits events for
    /// thinking steps, failed tasks, handoffs and guardrails, none of which
    /// survives into the terminal record. Use <see cref="StreamAsync"/> when the
    /// events themselves are the point.
    /// </summary>
    private static (List<Dictionary<string, object>>? ToolCalls, List<AgentEvent> Events) ExtractToolActivity(
        JsonNode? workflowWithTasks, string executionId)
    {
        List<Dictionary<string, object>>? toolCalls = null;
        var events = new List<AgentEvent>();

        if (workflowWithTasks?["tasks"] is not JsonArray tasks) return (toolCalls, events);

        foreach (var task in tasks)
        {
            if (task is null || task["outputData"] is not JsonObject outputData || !IsToolTask(task))
                continue;

            var name = ResolveToolName(task);
            var args = task["inputData"] is JsonObject inputData ? ToolArgs(inputData) : null;
            var result = ToolResult(outputData);

            var tc = new Dictionary<string, object> { ["name"] = name };
            if (args is not null) tc["args"] = args;
            if (result is not null) tc["result"] = result;
            (toolCalls ??= new List<Dictionary<string, object>>()).Add(tc);

            events.Add(new AgentEvent
            {
                Type = EventType.ToolCall,
                ExecutionId = executionId,
                ToolName = name,
                Args = args,
                Timestamp = task["startTime"]?.GetValue<long>(),
            });
            events.Add(new AgentEvent
            {
                Type = EventType.ToolResult,
                ExecutionId = executionId,
                ToolName = name,
                Result = result,
                Timestamp = task["endTime"]?.GetValue<long>(),
            });
        }
        return (toolCalls, events);
    }
}
