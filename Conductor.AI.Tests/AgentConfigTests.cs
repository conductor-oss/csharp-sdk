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
// T7 — AgentConfig.FromEnv() honors each env var, empty/invalid values fall back
// to the default, and the type carries no connection/auth/log field by
// construction (spec R4). Uses AgentConfig.EnvLookup (an injectable seam)
// instead of mutating real process environment variables.

using System.Collections.Generic;
using System.Linq;
using Conductor.AI;
using Xunit;

namespace Conductor.AI.Tests;

public sealed class AgentConfigTests
{
    private static AgentConfig WithEnv(Dictionary<string, string> env)
    {
        var previous = AgentConfig.EnvLookup;
        try
        {
            AgentConfig.EnvLookup = key => env.TryGetValue(key, out var v) ? v : null;
            return AgentConfig.FromEnv();
        }
        finally
        {
            AgentConfig.EnvLookup = previous;
        }
    }

    [Fact]
    public void NoEnv_AllDefaults()
    {
        var config = WithEnv(new());

        Assert.Equal(100, config.WorkerPollIntervalMs);
        Assert.Equal(1, config.WorkerThreadCount);
        Assert.True(config.AutoStartWorkers);
        Assert.True(config.DaemonWorkers);
        Assert.True(config.StreamingEnabled);
        Assert.True(config.LivenessEnabled);
        Assert.Equal(30.0, config.LivenessStallSeconds);
        Assert.Equal(10.0, config.LivenessCheckIntervalSeconds);
    }

    [Fact]
    public void EachEnvVar_Honored()
    {
        var env = new Dictionary<string, string>
        {
            ["CONDUCTOR_AGENT_WORKER_POLL_INTERVAL"] = "250",
            ["CONDUCTOR_AGENT_WORKER_THREADS"] = "4",
            ["CONDUCTOR_AGENT_AUTO_START_WORKERS"] = "false",
            ["CONDUCTOR_AGENT_DAEMON_WORKERS"] = "false",
            ["CONDUCTOR_AGENT_STREAMING_ENABLED"] = "false",
            ["CONDUCTOR_AGENT_LIVENESS_ENABLED"] = "false",
            ["CONDUCTOR_AGENT_LIVENESS_STALL_SECONDS"] = "45.5",
            ["CONDUCTOR_AGENT_LIVENESS_CHECK_INTERVAL_SECONDS"] = "5.5",
        };
        var config = WithEnv(env);

        Assert.Equal(250, config.WorkerPollIntervalMs);
        Assert.Equal(4, config.WorkerThreadCount);
        Assert.False(config.AutoStartWorkers);
        Assert.False(config.DaemonWorkers);
        Assert.False(config.StreamingEnabled);
        Assert.False(config.LivenessEnabled);
        Assert.Equal(45.5, config.LivenessStallSeconds);
        Assert.Equal(5.5, config.LivenessCheckIntervalSeconds);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-number")]
    [InlineData("   ")]
    public void InvalidOrEmpty_IntField_FallsBackToDefault(string raw)
    {
        var config = WithEnv(new() { ["CONDUCTOR_AGENT_WORKER_THREADS"] = raw });
        Assert.Equal(1, config.WorkerThreadCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-bool")]
    public void InvalidOrEmpty_BoolField_FallsBackToDefault(string raw)
    {
        var config = WithEnv(new() { ["CONDUCTOR_AGENT_AUTO_START_WORKERS"] = raw });
        Assert.True(config.AutoStartWorkers);
    }

    [Fact]
    public void NonPositive_IntField_FallsBackToDefault()
    {
        var config = WithEnv(new() { ["CONDUCTOR_AGENT_WORKER_POLL_INTERVAL"] = "0" });
        Assert.Equal(100, config.WorkerPollIntervalMs);
    }

    // ── Only CONDUCTOR_AGENT_* is supported ────────────────────────────
    //
    // The legacy AGENTSPAN_* names were removed outright — no aliasing. These tests
    // assert the removal positively, so re-introducing a fallback fails the build
    // rather than passing silently.

    [Fact]
    public void LegacyAgentspanName_IsIgnored()
    {
        var env = new Dictionary<string, string> { ["AGENTSPAN_WORKER_THREADS"] = "2" };
        Assert.Equal(1, WithEnv(env).WorkerThreadCount);
    }

    [Fact]
    public void LegacyAgentspanNames_IgnoredAcrossEveryKnob()
    {
        var env = new Dictionary<string, string>
        {
            ["AGENTSPAN_WORKER_POLL_INTERVAL"] = "250",
            ["AGENTSPAN_WORKER_THREADS"] = "4",
            ["AGENTSPAN_AUTO_START_WORKERS"] = "false",
            ["AGENTSPAN_DAEMON_WORKERS"] = "false",
            ["AGENTSPAN_STREAMING_ENABLED"] = "false",
            ["AGENTSPAN_LIVENESS_ENABLED"] = "false",
            ["AGENTSPAN_LIVENESS_STALL_SECONDS"] = "45.5",
            ["AGENTSPAN_LIVENESS_CHECK_INTERVAL_SECONDS"] = "5.5",
        };
        var config = WithEnv(env);

        Assert.Equal(100, config.WorkerPollIntervalMs);
        Assert.Equal(1, config.WorkerThreadCount);
        Assert.True(config.AutoStartWorkers);
        Assert.True(config.DaemonWorkers);
        Assert.True(config.StreamingEnabled);
        Assert.True(config.LivenessEnabled);
        Assert.Equal(30.0, config.LivenessStallSeconds);
        Assert.Equal(10.0, config.LivenessCheckIntervalSeconds);
    }

    [Fact]
    public void LegacyName_DoesNotOverrideCurrentName()
    {
        var env = new Dictionary<string, string>
        {
            ["CONDUCTOR_AGENT_WORKER_THREADS"] = "8",
            ["AGENTSPAN_WORKER_THREADS"] = "2",
        };
        Assert.Equal(8, WithEnv(env).WorkerThreadCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void BlankValue_FallsBackToDefault(string blank)
    {
        var env = new Dictionary<string, string> { ["CONDUCTOR_AGENT_WORKER_THREADS"] = blank };
        Assert.Equal(1, WithEnv(env).WorkerThreadCount);
    }

    [Fact]
    public void CurrentNames_HonoredForBoolAndDoubleFields()
    {
        var env = new Dictionary<string, string>
        {
            ["CONDUCTOR_AGENT_AUTO_START_WORKERS"] = "false",
            ["CONDUCTOR_AGENT_LIVENESS_STALL_SECONDS"] = "12.5",
        };
        var config = WithEnv(env);

        Assert.False(config.AutoStartWorkers);
        Assert.Equal(12.5, config.LivenessStallSeconds);
    }

    // ── R4: no connection/auth/log field by construction ───────────────

    [Fact]
    public void NoConnectionAuthOrLogFields()
    {
        var forbidden = new[] { "ServerUrl", "ApiKey", "AuthKey", "AuthSecret", "ApiSecret", "LogLevel" };
        var actual = typeof(AgentConfig).GetProperties().Select(p => p.Name);

        Assert.Empty(actual.Intersect(forbidden));
    }
}
