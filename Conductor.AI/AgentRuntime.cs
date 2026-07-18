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
using System.Text.Json.Nodes;
using Conductor.AI.Scheduling;
using Conductor.Client;
using Conductor.Client.Authentication;

namespace Conductor.AI;

/// <summary>
/// Main entry point for running Agentspan agents.
/// </summary>
/// <example>
/// <code>
/// await using var runtime = new AgentRuntime();
/// var result = await runtime.RunAsync(agent, "Hello!");
/// result.PrintResult();
/// </code>
/// </example>
public sealed class AgentRuntime : IAsyncDisposable, IDisposable
{
    private readonly IAgentClient _http;
    private readonly Configuration _conductorConfig;
    private readonly AgentConfig _agentConfig;
    private WorkerManager? _workers;

    /// <summary>
    /// Env lookup seam — tests override this to assert precedence without
    /// mutating process environment variables. Production default reads real
    /// process env.
    /// </summary>
    internal static Func<string, string?> EnvLookup = Environment.GetEnvironmentVariable;

    /// <summary>
    /// The control-plane <see cref="IAgentClient"/> backing this runtime — exposes
    /// control-plane <c>run</c>/<c>start</c>/<c>deploy</c>/<c>schedule</c> directly
    /// (without local tool-worker orchestration, which the runtime owns). Shares
    /// its <see cref="Configuration"/> — and therefore its token cache — with this
    /// runtime's worker plane.
    /// </summary>
    public IAgentClient Client => _http;

    /// <summary>Cron-schedule lifecycle API (delegates to <see cref="Client"/>).</summary>
    public Schedules Schedules => _http.Schedules;

    /// <summary>
    /// Build a runtime on an explicit (or env-resolved) <see cref="Configuration"/> —
    /// the same connection/auth object used across the SDK, so the agent client and
    /// the worker plane share one token authority. When <paramref name="configuration"/>
    /// is null, resolves from <c>CONDUCTOR_SERVER_URL</c>/<c>CONDUCTOR_AUTH_KEY</c>/
    /// <c>CONDUCTOR_AUTH_SECRET</c>, falling back to the legacy <c>AGENTSPAN_*</c>
    /// names, defaulting to <c>http://localhost:8080/api</c> with no auth.
    /// <paramref name="settings"/> is behavior-only tuning (worker polling,
    /// auto-start, streaming, liveness) — it defaults to <see cref="AgentConfig.FromEnv"/>
    /// and can never carry connection/auth (spec R4).
    /// </summary>
    public AgentRuntime(Configuration? configuration = null, AgentConfig? settings = null)
    {
        _conductorConfig = configuration ?? BuildConfiguration(null, null, null);
        _http = new OrkesAgentClient(_conductorConfig);
        _agentConfig = settings ?? AgentConfig.FromEnv();
    }

    /// <summary>
    /// Legacy sugar overload — folds <see cref="AgentRuntimeOptions"/> into a
    /// <see cref="Configuration"/> at construction time; the options object is
    /// never stored. Prefer the <see cref="Configuration"/> overload directly.
    /// </summary>
    public AgentRuntime(AgentRuntimeOptions options)
        : this(BuildConfiguration(options?.ServerUrl, options?.AuthKey, options?.AuthSecret))
    {
    }

    /// <summary>Worker poll interval in ms (env <c>AGENTSPAN_WORKER_POLL_INTERVAL</c>, default 100).</summary>
    public int WorkerPollIntervalMs => _agentConfig.WorkerPollIntervalMs;

    /// <summary>Worker thread count per task type (env <c>AGENTSPAN_WORKER_THREADS</c>, default 1).</summary>
    public int WorkerThreadCount => _agentConfig.WorkerThreadCount;

    /// <summary>
    /// Resolves the shared <see cref="Configuration"/> from explicit values (options
    /// overload) or environment, in <c>CONDUCTOR_*</c> → <c>AGENTSPAN_*</c> → default
    /// order. AuthenticationSettings is left null for OSS Conductor (no token exchange
    /// needed). For Orkes Cloud, set the key+secret pair and the SDK uses
    /// <see cref="OrkesAuthenticationSettings"/> to obtain a JWT automatically.
    /// </summary>
    /// <summary>Internal for T6 env-precedence tests — resolves the same way the public ctors do.</summary>
    internal static Configuration BuildConfiguration(string? serverUrl, string? authKey, string? authSecret)
    {
        var resolvedUrl = serverUrl
            ?? EnvLookup("CONDUCTOR_SERVER_URL")
            ?? EnvLookup("AGENTSPAN_SERVER_URL")
            ?? "http://localhost:8080/api";
        var resolvedKey = authKey ?? EnvLookup("CONDUCTOR_AUTH_KEY") ?? EnvLookup("AGENTSPAN_AUTH_KEY");
        var resolvedSecret = authSecret ?? EnvLookup("CONDUCTOR_AUTH_SECRET") ?? EnvLookup("AGENTSPAN_AUTH_SECRET");

        var config = new Configuration { BasePath = resolvedUrl };
        if (!string.IsNullOrEmpty(resolvedKey) && !string.IsNullOrEmpty(resolvedSecret))
            config.AuthenticationSettings = new OrkesAuthenticationSettings(resolvedKey, resolvedSecret);
        return config;
    }

    private WorkerManager NewWorkerManager()
        => new(_conductorConfig, _agentConfig.WorkerPollIntervalMs, _agentConfig.WorkerThreadCount);

    // ── Deploy / Serve ────────────────────────────────────────

    /// <summary>
    /// Compile and register an agent's workflow on the server without executing it.
    /// This is a CI/CD step: push agent definitions without starting workers.
    /// </summary>
    public DeploymentInfo[] Deploy(params Agent[] agents) => DeployAsync(agents).GetAwaiter().GetResult();

    /// <summary>
    /// Compile and register agents on the server without executing them.
    /// Returns one <see cref="DeploymentInfo"/> per agent.
    /// </summary>
    public async Task<DeploymentInfo[]> DeployAsync(params Agent[] agents)
    {
        var results = new DeploymentInfo[agents.Length];
        for (int i = 0; i < agents.Length; i++)
        {
            var agentConfig = AgentConfigSerializer.SerializeAgent(agents[i]);
            var registeredName = await _http.DeployAsync(agentConfig);
            results[i] = new DeploymentInfo(RegisteredName: registeredName, AgentName: agents[i].Name);
        }
        return results;
    }

    /// <summary>
    /// Deploy a single agent and reconcile its cron schedules declaratively.
    ///
    /// <para><c>schedules</c> semantics:
    /// <list type="bullet">
    ///   <item><c>null</c> → leave existing schedules untouched.</item>
    ///   <item>empty → purge all schedules for this agent.</item>
    ///   <item>non-empty → upsert these and prune any others for this agent.</item>
    /// </list></para>
    /// </summary>
    public async Task<DeploymentInfo> DeployAsync(Agent agent, IEnumerable<Schedule>? schedules)
    {
        var info = (await DeployAsync(new[] { agent }))[0];
        if (schedules is not null)
            await Schedules.ReconcileAsync(agent.Name, schedules);
        return info;
    }

    /// <summary>
    /// Deploy the agent, register its local tool workers, and block until
    /// <paramref name="ct"/> is cancelled. <c>serve</c> = deploy + serve (spec R9) —
    /// a bare <c>ServeAsync(agent)</c> is a complete, startable deployment; no
    /// prior <see cref="DeployAsync(Agent[])"/> or <see cref="RunAsync"/> call is required.
    /// </summary>
    public async Task ServeAsync(Agent agent, CancellationToken ct = default)
        => await ServeAsync(ct, agent);

    /// <summary>Deploy and serve multiple agents, blocking until cancelled.</summary>
    public async Task ServeAsync(CancellationToken ct = default, params Agent[] agents)
        => await ServeAsync(blocking: true, ct, agents);

    /// <summary>
    /// Deploy and serve one or more agents with an explicit blocking mode.
    /// <paramref name="blocking"/> = false returns as soon as every agent is
    /// deployed and its workers are registered and polling — for embedding the
    /// runtime in a host application that owns the process lifecycle. The caller
    /// is then responsible for eventually disposing the runtime.
    /// </summary>
    public async Task ServeAsync(bool blocking, CancellationToken ct = default, params Agent[] agents)
    {
        _workers ??= NewWorkerManager();
        foreach (var agent in agents)
        {
            // Deploy before registering this agent's workers — same ordering
            // `run`/`start` already establish; idempotent server-side.
            await DeployAsync(agent);
            _workers.RegisterAgentTools(agent);
        }
        await _workers.StartAsync();

        if (!blocking) return;

        try { await Task.Delay(Timeout.Infinite, ct); }
        catch (OperationCanceledException) { }
        finally { await StopWorkersAsync(); }
    }

    // ── Plan (dry-run compile) ────────────────────────────────

    /// <summary>
    /// Compile an agent to a Conductor WorkflowDef without executing it.
    /// Returns the raw server response including the workflow definition.
    /// Useful for inspecting, debugging, or CI/CD validation.
    /// </summary>
    public JsonNode? Plan(Agent agent) => PlanAsync(agent).GetAwaiter().GetResult();

    /// <summary>
    /// Compile an agent to a Conductor WorkflowDef without executing it.
    /// Returns the raw server response including the workflow definition.
    /// </summary>
    public async Task<JsonNode?> PlanAsync(Agent agent, CancellationToken ct = default)
    {
        var agentConfig = AgentConfigSerializer.SerializeAgent(agent);
        return await _http.CompileAsync(agentConfig, ct);
    }

    // ── Synchronous convenience wrappers ────────────────────

    /// <summary>Run an agent synchronously (blocks until done).</summary>
    public AgentResult Run(
        Agent agent, string prompt, string? sessionId = null,
        IEnumerable<string>? media = null, RunSettings? runSettings = null)
        => RunAsync(agent, prompt, sessionId, media: media, runSettings: runSettings).GetAwaiter().GetResult();

    /// <summary>Run a pre-deployed agent by workflow name (synchronous).</summary>
    public AgentResult Run(string workflowName, string prompt, string? sessionId = null)
        => RunByNameAsync(workflowName, prompt, sessionId).GetAwaiter().GetResult();

    /// <summary>Start an agent synchronously and return a handle.</summary>
    public AgentHandle Start(
        Agent agent, string prompt, string? sessionId = null,
        IEnumerable<string>? media = null, RunSettings? runSettings = null)
        => StartAsync(agent, prompt, sessionId, media: media, runSettings: runSettings).GetAwaiter().GetResult();

    /// <summary>Start a pre-deployed agent by workflow name (synchronous).</summary>
    public AgentHandle Start(string workflowName, string prompt, string? sessionId = null)
        => StartByNameAsync(workflowName, prompt, sessionId).GetAwaiter().GetResult();

    // ── Async API ────────────────────────────────────────────

    /// <summary>Run an agent and wait for the result.</summary>
    /// <param name="plan">
    /// Optional deterministic plan for <c>Strategy.PlanExecute</c> harnesses.
    /// When present, the SDK forwards it as <c>static_plan</c> on the start
    /// payload; the server's PAC extract_json picks it up as Case-0
    /// (highest priority) and discards the planner LLM's output.
    /// </param>
    public async Task<AgentResult> RunAsync(
        Agent agent, string prompt, string? sessionId = null,
        IEnumerable<string>? media = null, Plans.Plan? plan = null,
        RunSettings? runSettings = null, CancellationToken ct = default)
    {
        var handle = await StartInternalAsync(agent, prompt, sessionId, media, plan, runSettings, ct);
        try
        {
            return await handle.WaitAsync(ct);
        }
        finally
        {
            await StopWorkersAsync();
        }
    }

    /// <summary>Run a pre-deployed agent by workflow name and wait for the result.</summary>
    public async Task<AgentResult> RunByNameAsync(
        string workflowName, string prompt, string? sessionId = null, CancellationToken ct = default)
    {
        var handle = await StartByNameAsync(workflowName, prompt, sessionId, ct);
        return await handle.WaitAsync(ct);
    }

    /// <summary>Start an agent asynchronously and return a handle for streaming / HITL.</summary>
    public async Task<AgentHandle> StartAsync(
        Agent agent, string prompt, string? sessionId = null,
        IEnumerable<string>? media = null, Plans.Plan? plan = null,
        RunSettings? runSettings = null, CancellationToken ct = default)
    {
        return await StartInternalAsync(agent, prompt, sessionId, media, plan, runSettings, ct);
    }

    /// <summary>Start a pre-deployed agent by workflow name (no agentConfig payload).</summary>
    public async Task<AgentHandle> StartByNameAsync(
        string workflowName, string prompt, string? sessionId = null, CancellationToken ct = default)
    {
        var executionId = await _http.StartWorkflowByNameAsync(workflowName, prompt, sessionId ?? "", ct);
        return new AgentHandle(executionId, _http);
    }

    /// <summary>Stream events from an agent execution.</summary>
    public async IAsyncEnumerable<AgentEvent> StreamAsync(
        Agent agent, string prompt, string? sessionId = null,
        IEnumerable<string>? media = null, RunSettings? runSettings = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        var handle = await StartInternalAsync(agent, prompt, sessionId, media, plan: null, runSettings, ct);
        try
        {
            await foreach (var evt in handle.StreamAsync(ct))
                yield return evt;
        }
        finally
        {
            await StopWorkersAsync();
        }
    }

    // ── Resume ──────────────────────────────────────────────

    /// <summary>
    /// Re-attach to an existing agent execution and re-register workers.
    ///
    /// Fetches the workflow from the server, extracts the worker domain from
    /// its taskToDomain mapping (for stateful agents), and re-registers tool
    /// workers under that domain. Works across process restarts — the workflow
    /// is durable on the server.
    /// </summary>
    /// <param name="executionId">The execution ID from a previous StartAsync call.</param>
    /// <param name="agent">The same Agent definition that was originally executed.</param>
    public AgentHandle Resume(string executionId, Agent agent)
        => ResumeAsync(executionId, agent).GetAwaiter().GetResult();

    /// <summary>Async version of <see cref="Resume"/>.</summary>
    public async Task<AgentHandle> ResumeAsync(string executionId, Agent agent, CancellationToken ct = default)
    {
        var domain = await ExtractDomainAsync(executionId, ct);

        _workers ??= NewWorkerManager();
        _workers.RegisterAgentTools(agent, domain);
        await _workers.StartAsync();

        return new AgentHandle(executionId, _http, domain, streamingEnabled: _agentConfig.StreamingEnabled);
    }

    private async Task<string?> ExtractDomainAsync(string executionId, CancellationToken ct)
    {
        try
        {
            var wf = await _http.GetWorkflowAsync(executionId, ct);
            if (wf is null) return null;

            var taskToDomain = wf["taskToDomain"];
            if (taskToDomain is null) return null;

            var domains = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in taskToDomain.AsObject())
            {
                var v = kv.Value?.GetValue<string>();
                if (!string.IsNullOrEmpty(v))
                    domains[v] = domains.TryGetValue(v, out var c) ? c + 1 : 1;
            }

            return domains.Count == 0 ? null
                : domains.MaxBy(kv => kv.Value).Key;
        }
        catch { return null; }
    }

    // ── WMQ (Workflow Message Queue) ─────────────────────────

    /// <summary>
    /// Push a message into a running agent's Workflow Message Queue.
    /// The agent must have a <see cref="WaitForMessageTool"/> to receive messages.
    /// Requires conductor.workflow-message-queue.enabled=true on the server.
    /// </summary>
    /// <param name="executionId">The running workflow execution ID.</param>
    /// <param name="message">Any JSON-serializable object. Strings are wrapped as {"message": value}.</param>
    public async Task SendMessageAsync(string executionId, object message, CancellationToken ct = default)
        => await _http.SendWorkflowMessageAsync(executionId, message, ct);

    /// <summary>Push a message into a running agent's Workflow Message Queue (synchronous).</summary>
    public void SendMessage(string executionId, object message)
        => SendMessageAsync(executionId, message).GetAwaiter().GetResult();

    // ── Status / respond by execution ID ────────────────────

    /// <summary>Check the current status of an existing execution.</summary>
    public async Task<AgentStatus> GetStatusAsync(string executionId, CancellationToken ct = default)
    {
        var node = await _http.GetStatusAsync(executionId, ct);
        if (node is null) return new AgentStatus { ExecutionId = executionId };
        return new AgentStatus
        {
            ExecutionId = node["executionId"]?.GetValue<string>() ?? executionId,
            IsComplete = node["isComplete"]?.GetValue<bool>() ?? false,
            IsRunning = node["isRunning"]?.GetValue<bool>() ?? false,
            IsWaiting = node["isWaiting"]?.GetValue<bool>() ?? false,
            StatusValue = node["status"]?.GetValue<string>(),
            Reason = node["reason"]?.GetValue<string>(),
            CurrentTask = node["currentTask"]?.GetValue<string>(),
            PendingTool = node["pendingTool"] is not null
                ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(
                    node["pendingTool"]!.ToJsonString(), AgentspanJson.Options)
                : null,
        };
    }

    /// <summary>Check the current status of an existing execution (synchronous).</summary>
    public AgentStatus GetStatus(string executionId)
        => GetStatusAsync(executionId).GetAwaiter().GetResult();

    /// <summary>Respond to a waiting HITL approval or HumanTool question.</summary>
    public async Task RespondAsync(string executionId, object response, CancellationToken ct = default)
        => await _http.RespondAsync(executionId, response, ct);

    /// <summary>Respond to a waiting HITL approval or HumanTool question (synchronous).</summary>
    public void Respond(string executionId, object response)
        => RespondAsync(executionId, response).GetAwaiter().GetResult();

    // ── Event-targeted HITL (streaming) ──────────────────────
    // StreamAsync yields AgentEvents that carry the emitting executionId. Under
    // multi-agent strategies the HUMAN task lives in a sub-execution, so respond
    // to the WAITING event's executionId rather than the root. Mirrors Java's
    // AgentStream.approve(event)/reject(event).

    /// <summary>Approve the HITL task that emitted the given WAITING event.</summary>
    public async Task ApproveAsync(AgentEvent waitingEvent, string? comment = null, CancellationToken ct = default)
        => await _http.RespondAsync(EventExecId(waitingEvent),
            comment is null ? new { approved = true } : new { approved = true, reason = comment }, ct);

    /// <summary>Reject the HITL task that emitted the given WAITING event.</summary>
    public async Task RejectAsync(AgentEvent waitingEvent, string reason, CancellationToken ct = default)
        => await _http.RespondAsync(EventExecId(waitingEvent), new { approved = false, reason }, ct);

    /// <summary>Send an arbitrary structured response to the execution that emitted the given event.</summary>
    public async Task RespondAsync(AgentEvent waitingEvent, object response, CancellationToken ct = default)
        => await _http.RespondAsync(EventExecId(waitingEvent), response, ct);

    private static string EventExecId(AgentEvent e)
        => e.ExecutionId ?? throw new InvalidOperationException(
            "Event has no executionId to target — use the runtime's executionId-based RespondAsync instead.");

    // ── Internal ─────────────────────────────────────────────

    private async Task<AgentHandle> StartInternalAsync(
        Agent agent, string prompt, string? sessionId,
        IEnumerable<string>? media, Plans.Plan? plan, RunSettings? runSettings, CancellationToken ct)
    {
        // Generate a fresh per-execution domain UUID for stateful agents. The
        // server uses this as taskToDomain for every worker task in the run,
        // and we register local workers under the same domain so they poll the
        // per-execution queue. Without this, concurrent stateful runs share a
        // single domain queue and can dequeue each other's tasks.
        // Mirrors Python runtime._has_stateful_tools + run_id = uuid.uuid4().
        var runId = HasStatefulTools(agent) ? Guid.NewGuid().ToString("N") : null;

        if (_agentConfig.AutoStartWorkers)
        {
            // Fresh worker manager per run
            _workers ??= NewWorkerManager();
            _workers.RegisterAgentTools(agent, runId);
            await _workers.StartAsync();
        }

        var payload = AgentConfigSerializer.Serialize(agent, prompt, sessionId ?? "", media);
        runSettings?.ApplyToPayload(payload);
        if (runId is not null) payload["runId"] = runId;
        if (plan is not null)
        {
            // Server reads ${workflow.input.static_plan} as the Case-0 plan source
            // for Strategy.PlanExecute harnesses — wins over the planner LLM's output.
            payload["static_plan"] = plan.ToJson();
        }
        var executionId = await _http.StartAsync(payload, ct);

        // Stateful runs enqueue worker tasks under the per-execution domain — if
        // this process stops polling, nothing else ever will. Watch for that
        // stall so WaitAsync fails fast instead of hanging forever (spec R11).
        ServerLivenessMonitor? livenessMonitor = null;
        if (runId is not null && _agentConfig.LivenessEnabled)
        {
            livenessMonitor = new ServerLivenessMonitor(
                _conductorConfig, executionId,
                _agentConfig.LivenessStallSeconds, _agentConfig.LivenessCheckIntervalSeconds);
        }

        return new AgentHandle(
            executionId, _http, runId,
            streamingEnabled: _agentConfig.StreamingEnabled, livenessMonitor: livenessMonitor);
    }

    private static bool HasStatefulTools(Agent agent)
    {
        if (agent.Stateful) return true;
        foreach (var t in agent.Tools)
            if (t is not null && t.Stateful) return true;
        foreach (var sub in agent.Agents)
            if (HasStatefulTools(sub)) return true;
        if (agent.Router is not null && HasStatefulTools(agent.Router)) return true;
        return false;
    }

    private async Task StopWorkersAsync()
    {
        if (_workers is not null)
        {
            await _workers.DisposeAsync();
            _workers = null;
        }
    }

    /// <summary>Test-only seam — observe whether a worker manager is currently active.</summary>
    internal bool HasActiveWorkers => _workers is not null;

    // ── Disposal ─────────────────────────────────────────────

    public async ValueTask DisposeAsync()
    {
        await StopWorkersAsync();
        _http.Dispose();
    }

    public void Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();
}

/// <summary>
/// Legacy constructor sugar for <see cref="AgentRuntime"/> — folded into a
/// <see cref="Configuration"/> at construction time, never stored. Prefer
/// constructing a <see cref="Configuration"/> directly for new code.
/// </summary>
public sealed class AgentRuntimeOptions
{
    public string? ServerUrl { get; set; }
    public string? AuthKey { get; set; }
    public string? AuthSecret { get; set; }
}
