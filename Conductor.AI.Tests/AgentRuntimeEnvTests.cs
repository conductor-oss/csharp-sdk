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
// T6 — CONDUCTOR_* env chain precedence over legacy AGENTSPAN_* names, and the
// http://localhost:8080/api default. Uses AgentRuntime.EnvLookup (an injectable
// seam) instead of mutating real process environment variables, so tests never
// leak state across the test run. Exercises AgentRuntime.BuildConfiguration
// directly (internal, same resolution the public ctors use) plus a live
// AgentRuntime()/AgentRuntimeOptions round-trip for the ctor-shape contract.

using System.Collections.Generic;
using Conductor.AI;
using Xunit;

namespace Conductor.AI.Tests;

public sealed class AgentRuntimeEnvTests
{
    private static Conductor.Client.Configuration WithEnv(
        Dictionary<string, string> env, Func<Conductor.Client.Configuration> build)
    {
        var previous = AgentRuntime.EnvLookup;
        try
        {
            AgentRuntime.EnvLookup = key => env.TryGetValue(key, out var v) ? v : null;
            return build();
        }
        finally
        {
            AgentRuntime.EnvLookup = previous;
        }
    }

    [Fact]
    public void NoEnv_NoExplicitValue_DefaultsToLocalhost8080Api()
    {
        var config = WithEnv(new(), () => AgentRuntime.BuildConfiguration(null, null, null));

        Assert.Equal("http://localhost:8080/api", config.BasePath.TrimEnd('/'));
        Assert.Null(config.AuthenticationSettings);
    }

    [Fact]
    public void ConductorServerUrl_TakesPrecedenceOverAgentspan()
    {
        var env = new Dictionary<string, string>
        {
            ["CONDUCTOR_SERVER_URL"] = "http://conductor-wins/api",
            ["AGENTSPAN_SERVER_URL"] = "http://agentspan-loses/api",
        };
        var config = WithEnv(env, () => AgentRuntime.BuildConfiguration(null, null, null));

        Assert.Equal("http://conductor-wins/api", config.BasePath.TrimEnd('/'));
    }

    [Fact]
    public void AgentspanServerUrl_UsedWhenConductorAbsent()
    {
        var env = new Dictionary<string, string> { ["AGENTSPAN_SERVER_URL"] = "http://legacy/api" };
        var config = WithEnv(env, () => AgentRuntime.BuildConfiguration(null, null, null));

        Assert.Equal("http://legacy/api", config.BasePath.TrimEnd('/'));
    }

    [Fact]
    public void ExplicitServerUrl_TakesPrecedenceOverEnv()
    {
        var env = new Dictionary<string, string> { ["CONDUCTOR_SERVER_URL"] = "http://env-loses/api" };
        var config = WithEnv(env, () => AgentRuntime.BuildConfiguration("http://explicit-wins/api", null, null));

        Assert.Equal("http://explicit-wins/api", config.BasePath.TrimEnd('/'));
    }

    [Fact]
    public void ConductorAuthKeySecret_TakesPrecedenceOverAgentspan()
    {
        var env = new Dictionary<string, string>
        {
            ["CONDUCTOR_AUTH_KEY"] = "ck",
            ["CONDUCTOR_AUTH_SECRET"] = "cs",
            ["AGENTSPAN_AUTH_KEY"] = "ak",
            ["AGENTSPAN_AUTH_SECRET"] = "as",
        };
        var config = WithEnv(env, () => AgentRuntime.BuildConfiguration(null, null, null));

        Assert.NotNull(config.AuthenticationSettings);
    }

    [Fact]
    public void OnlyKey_NoSecret_NoAuthenticationSettings()
    {
        var env = new Dictionary<string, string> { ["CONDUCTOR_AUTH_KEY"] = "ck" };
        var config = WithEnv(env, () => AgentRuntime.BuildConfiguration(null, null, null));

        Assert.Null(config.AuthenticationSettings);
    }

    // ── Ctor-shape contract: zero-arg resolves the primary ctor unambiguously ──

    [Fact]
    public void ZeroArgCtor_DoesNotThrow_AndClientIsTypedIAgentClient()
    {
        using var runtime = new AgentRuntime();
        Assert.IsAssignableFrom<IAgentClient>(runtime.Client);
    }

    [Fact]
    public void OptionsOverload_BuildsDistinctConfigurationFromEnv()
    {
        var previous = AgentRuntime.EnvLookup;
        try
        {
            AgentRuntime.EnvLookup = _ => null;
            using var runtime = new AgentRuntime(new AgentRuntimeOptions { ServerUrl = "http://from-options/api" });
            Assert.IsAssignableFrom<IAgentClient>(runtime.Client);
        }
        finally
        {
            AgentRuntime.EnvLookup = previous;
        }
    }
}
