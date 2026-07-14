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
using Conductor.Api;
using Conductor.Client;
using Conductor.Client.Extensions;
using Conductor.Client.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Task = Conductor.Client.Models.Task;

namespace Conductor.AI;

// ── WorkerManager ──────────────────────────────────────────

/// <summary>
/// Registers all tool workers discovered from the agent tree as <see cref="AgentToolWorker"/>s
/// and hosts them on the Worker SDK (guide §25.1 — tools are ordinary Conductor
/// workers; polling, batching, and update-with-retry-backoff are the SDK's,
/// not ours). One <see cref="IHost"/> per <see cref="WorkerManager"/> instance
/// preserves the fresh-manager-per-stateful-run lifecycle.
/// </summary>
internal sealed class WorkerManager : IAsyncDisposable
{
    private readonly Configuration _conductorConfig;
    private readonly MetadataResourceApi _metadataClient;
    private readonly List<AgentToolWorker> _workers = [];
    private readonly int _pollIntervalMs;
    private readonly int _threadCount;
    private IHost? _host;

    public WorkerManager(Configuration conductorConfig, int pollIntervalMs = 100, int threadCount = 1)
    {
        _conductorConfig = conductorConfig;
        _metadataClient = new MetadataResourceApi(conductorConfig);
        _pollIntervalMs = pollIntervalMs > 0 ? pollIntervalMs : 100;
        _threadCount = threadCount > 0 ? threadCount : 1;
    }

    /// <summary>Test-only seam — the built host, or null (idempotent <see cref="StartAsync"/> guard).</summary>
    internal IHost? HostForTesting => _host;

    private AgentToolWorker NewWorker(
        string taskName,
        Func<Dictionary<string, JsonElement>, ToolContext?, System.Threading.Tasks.Task<object?>> handler,
        string[]? credentialNames = null,
        string? domain = null)
    {
        RegisterTaskDef(taskName, credentialNames);
        return new(taskName, new ToolTaskExecutor(taskName, handler, credentialNames), _pollIntervalMs, _threadCount, domain);
    }

    /// <summary>
    /// Upsert a task def for <paramref name="taskName"/>, stamping declared
    /// credential names onto <see cref="TaskDef.RuntimeMetadata"/> (spec R6).
    /// Runs on EVERY registration, not just the first — skipping it on
    /// re-registration would leave a stale runtimeMetadata stamp in place after
    /// credentials changed. PUT (overwrite) first; POST (create) only if the
    /// task def doesn't exist yet — a create-only POST would silently leave a
    /// stale def in place on re-registration.
    /// </summary>
    private void RegisterTaskDef(string taskName, string[]? credentialNames)
    {
        var taskDef = new TaskDef { Name = taskName };
        if (credentialNames is { Length: > 0 })
            taskDef.RuntimeMetadata = credentialNames.ToList();

        try
        {
            _metadataClient.UpdateTaskDef(taskDef);
        }
        catch
        {
            try { _metadataClient.RegisterTaskDef(new List<TaskDef> { taskDef }); }
            catch { /* best-effort — a failed registration surfaces later as a stuck SCHEDULED task */ }
        }
    }

    public void RegisterTools(IEnumerable<ToolDef> tools, string? domain = null)
    {
        foreach (var tool in tools)
        {
            if (tool.Handler is null) continue;
            _workers.Add(NewWorker(tool.Name, tool.Handler,
                credentialNames: tool.Credentials.Length > 0 ? tool.Credentials : null,
                domain: domain));
        }
    }

    public void RegisterGuardrails(IEnumerable<GuardrailDef> guardrails, string? domain = null)
    {
        foreach (var g in guardrails)
        {
            if (g.Handler is null) continue;
            var handler = g.Handler;
            var onFail = g.OnFail;
            var maxRetries = g.MaxRetries;
            var gName = g.Name;

            _workers.Add(NewWorker(g.Name, async (args, _ctx) =>
            {
                string content = args.TryGetValue("content", out var contentEl)
                    ? (contentEl.ValueKind == JsonValueKind.String
                        ? contentEl.GetString() ?? ""
                        : contentEl.GetRawText())
                    : "";

                int iteration = args.TryGetValue("iteration", out var iterEl) &&
                                iterEl.ValueKind == JsonValueKind.Number
                    ? iterEl.GetInt32()
                    : 0;

                GuardrailResult result;
                try
                {
                    result = await handler(content);
                }
                catch (Exception ex)
                {
                    var effectiveOnFailOnEx = onFail;
                    if (effectiveOnFailOnEx == OnFail.Retry && iteration >= maxRetries)
                        effectiveOnFailOnEx = OnFail.Raise;
                    return (object)new Dictionary<string, object?>
                    {
                        ["passed"] = false,
                        ["message"] = $"Guardrail error: {ex.Message}",
                        ["on_fail"] = effectiveOnFailOnEx.ToString().ToLowerInvariant(),
                        ["fixed_output"] = null,
                        ["guardrail_name"] = gName,
                        ["should_continue"] = effectiveOnFailOnEx == OnFail.Retry,
                    };
                }

                if (!result.Passed)
                {
                    var effectiveOnFail = onFail;
                    if (effectiveOnFail == OnFail.Retry && iteration >= maxRetries)
                        effectiveOnFail = OnFail.Raise;
                    if (effectiveOnFail == OnFail.Fix && result.FixedOutput is null)
                        effectiveOnFail = OnFail.Raise;

                    return (object)new Dictionary<string, object?>
                    {
                        ["passed"] = false,
                        ["message"] = result.Message ?? "",
                        ["on_fail"] = effectiveOnFail.ToString().ToLowerInvariant(),
                        ["fixed_output"] = result.FixedOutput,
                        ["guardrail_name"] = gName,
                        ["should_continue"] = effectiveOnFail == OnFail.Retry,
                    };
                }

                return (object)new Dictionary<string, object?>
                {
                    ["passed"] = true,
                    ["message"] = "",
                    ["on_fail"] = "pass",
                    ["fixed_output"] = null,
                    ["guardrail_name"] = "",
                    ["should_continue"] = false,
                };
            }, domain: domain));
        }
    }

    public void RegisterAgentTools(Agent agent, string? domain = null)
    {
        if (agent.Framework == "skill")
            RegisterSkillWorkers(agent, domain);

        RegisterTools(agent.Tools, domain);
        RegisterGuardrails(agent.Guardrails, domain);
        foreach (var tool in agent.Tools)
            RegisterGuardrails(tool.Guardrails, domain);
        RegisterCallbacks(agent, domain);

        // Local code execution worker — the server adds an execute_code tool to
        // the agent when LocalCodeExecution=true (or CodeExecution is set), but
        // the SDK is responsible for polling and actually running the code.
        // Without this, the LLM's execute_code calls would sit in SCHEDULED
        // forever. Mirrors Java's AgentRuntime.prepareWorkers code-exec branch.
        if (agent.LocalCodeExecution || agent.CodeExecution is not null)
            RegisterLocalCodeExecutionWorker(agent, domain);

        if (agent.Strategy == Strategy.Swarm && agent.Agents.Count > 0)
            RegisterSwarmTransferWorkers(agent, domain);

        if (agent.Strategy == Strategy.Manual && agent.Agents.Count > 0)
            RegisterManualSelectionWorker(agent, domain);

        foreach (var sub in agent.Agents)
            RegisterAgentTools(sub, domain);
        if (agent.Router is not null)
            RegisterAgentTools(agent.Router, domain);

        foreach (var tool in agent.Tools)
        {
            if (tool.ToolType == "agent_tool" && tool.WrappedAgent is not null)
                RegisterAgentTools(tool.WrappedAgent, domain);
        }
    }

    private void RegisterSkillWorkers(Agent agent, string? domain = null)
    {
        foreach (var worker in Skill.CreateSkillWorkers(agent))
        {
            _workers.Add(NewWorker(worker.Name, async (args, _ctx) =>
            {
                var input = args.ToDictionary(
                    kv => kv.Key,
                    kv => JsonElementToObject(kv.Value));
                return await worker.Handler(input);
            }, domain: domain));
        }
    }

    private static object? JsonElementToObject(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number when value.TryGetInt64(out var l) => l,
            JsonValueKind.Number when value.TryGetDouble(out var d) => d,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => value.GetRawText(),
        };
    }

    private void RegisterLocalCodeExecutionWorker(Agent agent, string? domain)
    {
        var taskName = $"{agent.Name}_execute_code";
        var timeout = agent.CodeExecution?.Timeout ?? 30;

        _workers.Add(NewWorker(taskName, async (args, _) =>
        {
            string language = "python";
            if (args.TryGetValue("language", out var lang) && lang.ValueKind == JsonValueKind.String)
                language = lang.GetString() ?? "python";

            string code = "";
            if (args.TryGetValue("code", out var c) && c.ValueKind == JsonValueKind.String)
                code = c.GetString() ?? "";

            return await ExecuteLocalCodeAsync(language, code, timeout);
        }, domain: domain));
    }

    private static async System.Threading.Tasks.Task<object?> ExecuteLocalCodeAsync(string language, string code, int timeoutSeconds)
    {
        // Delegate to the shared LocalCodeExecutor (subprocess + temp file +
        // interpreter table + timeout + cleanup), then map its structured
        // ExecutionResult onto this worker's wire contract. stdout+stderr are
        // combined into "output" to preserve the prior runtime behavior.
        var exec = new LocalCodeExecutor(language: language, timeout: timeoutSeconds);
        var er = await exec.ExecuteAsync(code);

        var result = new Dictionary<string, object?>
        {
            ["output"] = (er.Output ?? "") + (er.Error ?? ""),
            ["exit_code"] = er.ExitCode,
            ["success"] = er.Success,
        };
        if (!er.Success)
        {
            result["error"] = er.TimedOut
                ? $"Code execution timed out after {timeoutSeconds}s"
                : !string.IsNullOrEmpty(er.Error) ? er.Error
                : $"Process exited with code {er.ExitCode}";
        }
        return result;
    }

    private void RegisterCallbacks(Agent agent, string? domain = null)
    {
        // before_model / after_model keep their bespoke argument signatures
        // (messages list / llm_result string). Track them so the generic
        // position loop below doesn't double-register the same task name.
        var registered = new HashSet<string>(StringComparer.Ordinal);

        if (agent.BeforeModelCallback is not null)
        {
            var cb = agent.BeforeModelCallback;
            registered.Add("before_model");
            _workers.Add(NewWorker($"{agent.Name}_before_model", (args, _) =>
            {
                List<JsonElement>? messages = null;
                if (args.TryGetValue("messages", out var msgEl) && msgEl.ValueKind == JsonValueKind.Array)
                    messages = msgEl.EnumerateArray().ToList();
                var result = cb(messages);
                return System.Threading.Tasks.Task.FromResult<object?>(result ?? new Dictionary<string, object>());
            }, domain: domain));
        }

        if (agent.AfterModelCallback is not null)
        {
            var cb = agent.AfterModelCallback;
            registered.Add("after_model");
            _workers.Add(NewWorker($"{agent.Name}_after_model", (args, _) =>
            {
                string? llmResult = args.TryGetValue("llm_result", out var resEl) && resEl.ValueKind == JsonValueKind.String
                    ? resEl.GetString()
                    : null;
                var result = cb(llmResult);
                return System.Threading.Tasks.Task.FromResult<object?>(result ?? new Dictionary<string, object>());
            }, domain: domain));
        }

        // Generic kwargs-based callbacks: the agent/tool function callbacks plus
        // any CallbackHandler that overrides a hook. Multiple delegates can target
        // one position (run in order, first non-empty return short-circuits).
        var byPosition =
            new Dictionary<string, List<Func<Dictionary<string, JsonElement>, Dictionary<string, object>?>>>(
                StringComparer.Ordinal);

        void Add(string position, Func<Dictionary<string, JsonElement>, Dictionary<string, object>?>? fn)
        {
            if (fn is null) return;
            if (!byPosition.TryGetValue(position, out var list)) byPosition[position] = list = [];
            list.Add(fn);
        }

        Add("before_agent", agent.BeforeAgentCallback);
        Add("after_agent", agent.AfterAgentCallback);
        Add("before_tool", agent.BeforeToolCallback);
        Add("after_tool", agent.AfterToolCallback);

        foreach (var (position, method) in CallbackHandler.Positions)
            foreach (var handler in agent.Callbacks)
                if (handler.Overrides(method))
                {
                    var h = handler;
                    var m = method;
                    Add(position, kwargs => h.Invoke(m, kwargs));
                }

        foreach (var (position, delegates) in byPosition)
        {
            if (registered.Contains(position)) continue;
            var fns = delegates;
            _workers.Add(NewWorker($"{agent.Name}_{position}", (args, _) =>
            {
                foreach (var fn in fns)
                {
                    var r = fn(args);
                    if (r is { Count: > 0 }) return System.Threading.Tasks.Task.FromResult<object?>(r);
                }
                return System.Threading.Tasks.Task.FromResult<object?>(new Dictionary<string, object>());
            }, domain: domain));
        }
    }

    private void RegisterSwarmTransferWorkers(Agent agent, string? domain = null)
    {
        var allNames = new List<string> { agent.Name };
        allNames.AddRange(agent.Agents.Select(a => a.Name));

        var registered = new HashSet<string>();
        foreach (var sourceName in allNames)
        {
            foreach (var targetName in allNames)
            {
                if (sourceName == targetName) continue;
                var toolName = $"{sourceName}_transfer_to_{targetName}";
                if (!registered.Add(toolName)) continue;
                _workers.Add(NewWorker(toolName,
                    (_, _) => System.Threading.Tasks.Task.FromResult<object?>(new Dictionary<string, object>()),
                    domain: domain));
            }
        }

        foreach (var name in allNames)
        {
            _workers.Add(NewWorker($"{name}_check_transfer", (args, _) =>
            {
                if (args.TryGetValue("tool_calls", out var tcEl) && tcEl.ValueKind == JsonValueKind.Array)
                {
                    foreach (var tc in tcEl.EnumerateArray())
                    {
                        var tcName = tc.TryGetProperty("name", out var np) ? np.GetString() ?? "" : "";
                        if (tcName.Contains("_transfer_to_"))
                        {
                            var transferTarget = tcName.Split("_transfer_to_", 2)[1];
                            return System.Threading.Tasks.Task.FromResult<object?>(new Dictionary<string, object>
                            {
                                ["is_transfer"] = true,
                                ["transfer_to"] = transferTarget,
                            });
                        }
                    }
                }
                return System.Threading.Tasks.Task.FromResult<object?>(new Dictionary<string, object>
                {
                    ["is_transfer"] = false,
                    ["transfer_to"] = "",
                });
            }, domain: domain));
        }

        var nameToIdx = new Dictionary<string, string> { [agent.Name] = "0" };
        for (int i = 0; i < agent.Agents.Count; i++)
            nameToIdx[agent.Agents[i].Name] = (i + 1).ToString();
        var idxToName = nameToIdx.ToDictionary(kv => kv.Value, kv => kv.Key);
        var allowedTransitions = agent.AllowedTransitions;

        bool IsAllowed(string sourceIdx, string targetName)
        {
            if (allowedTransitions is null) return true;
            var sourceName = idxToName.TryGetValue(sourceIdx, out var sn) ? sn : "";
            return allowedTransitions.TryGetValue(sourceName, out var targets)
                && targets.Contains(targetName);
        }

        bool IsTransferTruthy(JsonElement val) =>
            val.ValueKind == JsonValueKind.True ||
            (val.ValueKind == JsonValueKind.String && val.GetString()?.Trim().ToLower() == "true");

        var handoffConditions = agent.Handoffs;

        _workers.Add(NewWorker($"{agent.Name}_handoff_check", (args, _) =>
        {
            var activeAgent = args.TryGetValue("active_agent", out var ae) ? ae.GetString() ?? "0" : "0";
            var isTransfer = args.TryGetValue("is_transfer", out var it) && IsTransferTruthy(it);
            var transferTo = args.TryGetValue("transfer_to", out var tt) ? tt.GetString() ?? "" : "";

            // Priority 1: explicit transfer tool was called.
            if (isTransfer && !string.IsNullOrEmpty(transferTo) && IsAllowed(activeAgent, transferTo))
            {
                var targetIdx = nameToIdx.TryGetValue(transferTo, out var ti) ? ti : activeAgent;
                if (targetIdx != activeAgent)
                    return System.Threading.Tasks.Task.FromResult<object?>(new Dictionary<string, object>
                    {
                        ["active_agent"] = targetIdx,
                        ["handoff"] = true,
                    });
            }

            // Priority 2: condition-based handoffs (fallback). Mirrors Python's
            // handoff_check_worker — evaluate each trigger against the context.
            if (handoffConditions.Count > 0)
            {
                var context = new Dictionary<string, object?>
                {
                    ["result"] = args.TryGetValue("result", out var rEl) ? rEl.ToString() : "",
                    ["messages"] = args.TryGetValue("conversation", out var cEl) ? cEl.ToString() : "",
                    ["tool_name"] = "",
                    ["tool_result"] = "",
                };
                foreach (var cond in handoffConditions)
                {
                    if (!cond.ShouldHandoff(context)) continue;
                    if (!IsAllowed(activeAgent, cond.Target)) continue;
                    var targetIdx = nameToIdx.TryGetValue(cond.Target, out var ci) ? ci : activeAgent;
                    if (targetIdx != activeAgent)
                        return System.Threading.Tasks.Task.FromResult<object?>(new Dictionary<string, object>
                        {
                            ["active_agent"] = targetIdx,
                            ["handoff"] = true,
                        });
                }
            }

            return System.Threading.Tasks.Task.FromResult<object?>(new Dictionary<string, object>
            {
                ["active_agent"] = activeAgent,
                ["handoff"] = false,
            });
        }, domain: domain));
    }

    private void RegisterManualSelectionWorker(Agent agent, string? domain = null)
    {
        var nameToIdx = agent.Agents.Select((a, i) => (a.Name, Index: i.ToString()))
                                    .ToDictionary(t => t.Name, t => t.Index);

        _workers.Add(NewWorker($"{agent.Name}_process_selection", (args, _) =>
        {
            string selected = "0";
            if (args.TryGetValue("human_output", out var ho))
            {
                if (ho.ValueKind == JsonValueKind.Object)
                {
                    string? agentName = null;
                    if (ho.TryGetProperty("selected", out var sp)) agentName = sp.GetString();
                    else if (ho.TryGetProperty("agent", out var ap)) agentName = ap.GetString();

                    if (agentName != null && nameToIdx.TryGetValue(agentName, out var idx))
                        selected = idx;
                    else if (agentName != null)
                        selected = agentName;
                }
                else if (ho.ValueKind == JsonValueKind.String)
                {
                    var sv = ho.GetString() ?? "0";
                    selected = nameToIdx.TryGetValue(sv, out var idx2) ? idx2 : sv;
                }
                else if (ho.ValueKind == JsonValueKind.Number)
                {
                    selected = ho.GetInt32().ToString();
                }
            }
            return System.Threading.Tasks.Task.FromResult<object?>(
                new Dictionary<string, object> { ["selected"] = selected });
        }, domain: domain));
    }

    /// <summary>
    /// Build and start the Worker SDK host for every worker registered so far.
    /// Idempotent — a shared <see cref="WorkerManager"/> can have this invoked
    /// more than once (e.g. overlapping runs sharing the same AgentRuntime);
    /// re-invoking must not build a second host and double-poll every task type.
    /// </summary>
    public async System.Threading.Tasks.Task StartAsync()
    {
        if (_host is not null) return;
        _host = WorkflowTaskHost.CreateWorkerHost(_conductorConfig, LogLevel.Warning, _workers.ToArray());
        await _host.StartAsync();
    }

    public async System.Threading.Tasks.Task StopAsync()
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
            _host = null;
        }
        _workers.Clear();
    }

    public async ValueTask DisposeAsync() => await StopAsync();
}
