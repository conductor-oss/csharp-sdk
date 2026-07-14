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

namespace Conductor.AI;

/// <summary>
/// Control-plane client for the Conductor <c>/agent/*</c> API (compile, deploy,
/// start, status, execution, list, respond, stop, signal, stream) plus
/// convenience entry points to <b>run</b> and <b>schedule</b> agents.
///
/// <para><b>Run is control-plane only:</b> the <see cref="RunAsync"/>/<see cref="StartAsync(Agent,string,string?,IEnumerable{string}?,Plans.Plan?,CancellationToken)"/>
/// family starts the agent and polls to a result — it does NOT register or poll
/// local tool workers. Agents that use local <c>[Tool]</c> functions must run
/// through <see cref="AgentRuntime"/>, which owns worker orchestration. For
/// LLM-only agents, remote tools (HTTP/MCP), or pre-deployed workflows, this
/// client suffices.</para>
///
/// <para>Obtain an instance via <see cref="OrkesApiClientExtensions.GetAgentClient(Conductor.Client.OrkesApiClient)"/>
/// or <see cref="OrkesApiClientExtensions.GetAgentClient(Conductor.Client.Configuration)"/> —
/// both share the same <see cref="Conductor.Client.Configuration"/> (and therefore
/// the same token cache) as the rest of the SDK.</para>
/// </summary>
public interface IAgentClient : IDisposable, IAsyncDisposable
{
    // ── Agent API (control plane, raw wire shapes) ─────────────

    /// <summary>Start an agent execution from a compiled config payload. Returns the execution id.</summary>
    Task<string> StartAsync(JsonObject payload, CancellationToken ct = default);

    /// <summary>Deploy (register) an agent on the server without starting execution. Returns the registered name.</summary>
    Task<string> DeployAsync(JsonObject agentConfig, CancellationToken ct = default);

    /// <summary>Compile an agent to a Conductor WorkflowDef without executing it.</summary>
    Task<JsonNode?> CompileAsync(JsonObject agentConfig, CancellationToken ct = default);

    /// <summary>Fetch the lightweight status of a running or completed execution.</summary>
    Task<JsonNode?> GetStatusAsync(string executionId, CancellationToken ct = default);

    /// <summary>Fetch the full execution record (includes tokenUsage, finishReason). Null on any failure (enrichment read).</summary>
    Task<JsonNode?> GetExecutionAsync(string executionId, CancellationToken ct = default);

    /// <summary>List agent executions matching optional query parameters (server-defined filters).</summary>
    Task<JsonNode?> ListExecutionsAsync(IReadOnlyDictionary<string, string>? queryParams = null, CancellationToken ct = default);

    /// <summary>Respond to a waiting HITL approval or HumanTool question.</summary>
    Task RespondAsync(string executionId, object body, CancellationToken ct = default);

    /// <summary>Gracefully stop an agent — sets _stop_requested and unblocks WMQ waits.</summary>
    Task StopAgentAsync(string executionId, CancellationToken ct = default);

    /// <summary>Immediately cancel an agent execution (TERMINATED status).</summary>
    Task CancelAgentAsync(string executionId, string reason = "", CancellationToken ct = default);

    /// <summary>Send a signal message to a running agent execution (<c>POST /agent/{id}/signal</c>).</summary>
    Task SignalAsync(string executionId, object message, CancellationToken ct = default);

    /// <summary>Push a message into a running agent's Workflow Message Queue.</summary>
    Task SendWorkflowMessageAsync(string executionId, object message, CancellationToken ct = default);

    /// <summary>Stream events from an agent execution over SSE, reconnecting with <c>Last-Event-ID</c> on mid-stream drop.</summary>
    IAsyncEnumerable<AgentEvent> StreamEventsAsync(string executionId, string? lastEventId = null, CancellationToken ct = default);

    /// <summary>Fetch the workflow definition (without tasks) — e.g. to read taskToDomain. Null on any failure (enrichment read).</summary>
    Task<JsonNode?> GetWorkflowAsync(string executionId, CancellationToken ct = default);

    /// <summary>
    /// Resolve credential values from the server using the execution token
    /// (pull path). TEMPORARY — deleted in favor of TaskDef/Task
    /// <c>runtimeMetadata</c> wire delivery (spec R6/R12).
    /// </summary>
    Task<Dictionary<string, string>> ResolveCredentialsAsync(
        string? executionToken, IEnumerable<string> names, CancellationToken ct = default);

    // ── Conveniences (agent-level, control-plane) ──────────────

    /// <summary>Compile + register + start an agent, then poll to a result. No local tool workers.</summary>
    Task<AgentResult> RunAsync(
        Agent agent, string prompt, string? sessionId = null,
        IEnumerable<string>? media = null, Plans.Plan? plan = null, CancellationToken ct = default);

    /// <summary>Compile + register + start an agent; returns a handle. No local tool workers.</summary>
    Task<AgentHandle> StartAsync(
        Agent agent, string prompt, string? sessionId = null,
        IEnumerable<string>? media = null, Plans.Plan? plan = null, CancellationToken ct = default);

    /// <summary>Compile + register one or more agents on the server (no execution).</summary>
    Task<DeploymentInfo[]> DeployAsync(params Agent[] agents);

    /// <summary>Cron-schedule lifecycle API (save/list/pause/resume/delete/runNow/preview/reconcile).</summary>
    Schedules Schedules { get; }

    /// <summary>Deploy an agent and reconcile its cron schedules declaratively.</summary>
    Task<DeploymentInfo> ScheduleAsync(Agent agent, IEnumerable<Schedule> schedules, CancellationToken ct = default);

    /// <summary>Start a pre-deployed workflow by name (no agentConfig payload).</summary>
    Task<string> StartWorkflowByNameAsync(string workflowName, string prompt, string sessionId = "", CancellationToken ct = default);
}
