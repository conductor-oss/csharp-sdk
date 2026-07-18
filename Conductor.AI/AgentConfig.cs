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
namespace Conductor.AI;

/// <summary>
/// Agent-runtime behavior tuning — worker polling, auto-start, streaming, and
/// liveness knobs. Connection, auth, and log level are NOT here (spec R4) —
/// those come from <see cref="Conductor.Client.Configuration"/> exclusively, so
/// this type is impossible to construct with a connection/auth field.
/// </summary>
/// <example>
/// <code>
/// var runtime = new AgentRuntime(settings: AgentConfig.FromEnv());
/// </code>
/// </example>
public sealed class AgentConfig
{
    /// <summary>Worker poll interval in ms (env <c>AGENTSPAN_WORKER_POLL_INTERVAL</c>, default 100).</summary>
    public int WorkerPollIntervalMs { get; init; } = 100;

    /// <summary>Worker thread count per task type (env <c>AGENTSPAN_WORKER_THREADS</c>, default 1).</summary>
    public int WorkerThreadCount { get; init; } = 1;

    /// <summary>
    /// When false, <c>run</c>/<c>start</c>/<c>stream</c> skip local tool-worker
    /// registration and polling (env <c>AGENTSPAN_AUTO_START_WORKERS</c>, default true).
    /// <c>serve</c> always starts workers regardless of this flag.
    /// </summary>
    public bool AutoStartWorkers { get; init; } = true;

    /// <summary>
    /// Whether SDK-owned background loops (worker polling, SSE reconnect) are
    /// daemon-like and never block process exit (env <c>AGENTSPAN_DAEMON_WORKERS</c>,
    /// default true) — documents the contract; .NET <see cref="Task"/>-based loops
    /// already don't keep the process alive.
    /// </summary>
    public bool DaemonWorkers { get; init; } = true;

    /// <summary>
    /// When false, <c>stream</c> skips SSE entirely and degrades to status polling
    /// (env <c>AGENTSPAN_STREAMING_ENABLED</c>, default true). SSE connection
    /// failures degrade the same way regardless of this flag.
    /// </summary>
    public bool StreamingEnabled { get; init; } = true;

    /// <summary>
    /// When true, stateful runs are monitored for worker stalls (spec R11; env
    /// <c>AGENTSPAN_LIVENESS_ENABLED</c>, default true).
    /// </summary>
    public bool LivenessEnabled { get; init; } = true;

    /// <summary>Seconds a scheduled task may sit unpolled before it counts as a stall (env <c>AGENTSPAN_LIVENESS_STALL_SECONDS</c>, default 30.0).</summary>
    public double LivenessStallSeconds { get; init; } = 30.0;

    /// <summary>Seconds between liveness checks (env <c>AGENTSPAN_LIVENESS_CHECK_INTERVAL_SECONDS</c>, default 10.0).</summary>
    public double LivenessCheckIntervalSeconds { get; init; } = 10.0;

    /// <summary>
    /// Env lookup seam — tests override this to assert precedence without
    /// mutating process environment variables.
    /// </summary>
    internal static Func<string, string?> EnvLookup = Environment.GetEnvironmentVariable;

    /// <summary>
    /// Build from environment variables, with lenient parsing — invalid or
    /// empty values fall back to the default rather than throwing.
    /// </summary>
    public static AgentConfig FromEnv() => new()
    {
        WorkerPollIntervalMs = ParseInt("AGENTSPAN_WORKER_POLL_INTERVAL", 100, min: 1),
        WorkerThreadCount = ParseInt("AGENTSPAN_WORKER_THREADS", 1, min: 1),
        AutoStartWorkers = ParseBool("AGENTSPAN_AUTO_START_WORKERS", true),
        DaemonWorkers = ParseBool("AGENTSPAN_DAEMON_WORKERS", true),
        StreamingEnabled = ParseBool("AGENTSPAN_STREAMING_ENABLED", true),
        LivenessEnabled = ParseBool("AGENTSPAN_LIVENESS_ENABLED", true),
        LivenessStallSeconds = ParseDouble("AGENTSPAN_LIVENESS_STALL_SECONDS", 30.0),
        LivenessCheckIntervalSeconds = ParseDouble("AGENTSPAN_LIVENESS_CHECK_INTERVAL_SECONDS", 10.0),
    };

    private static int ParseInt(string key, int defaultValue, int min)
    {
        var raw = EnvLookup(key);
        return int.TryParse(raw, out var v) && v >= min ? v : defaultValue;
    }

    private static double ParseDouble(string key, double defaultValue)
    {
        var raw = EnvLookup(key);
        return double.TryParse(raw, out var v) ? v : defaultValue;
    }

    private static bool ParseBool(string key, bool defaultValue)
    {
        var raw = EnvLookup(key);
        if (string.IsNullOrWhiteSpace(raw)) return defaultValue;
        return raw.Trim().ToLowerInvariant() switch
        {
            "true" or "1" or "yes" => true,
            "false" or "0" or "no" => false,
            _ => defaultValue,
        };
    }
}
