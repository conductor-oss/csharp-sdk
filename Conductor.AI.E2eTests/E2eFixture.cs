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
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace Conductor.AI.E2eTests;

/// <summary>
/// Shared fixture that checks server availability once per test collection.
/// Tests skip automatically when the server is unreachable.
/// </summary>
public sealed class E2eFixture : IAsyncLifetime
{
    private static readonly string ServerBase =
        (Environment.GetEnvironmentVariable("CONDUCTOR_SERVER_URL")
         ?? "http://localhost:8080/api")
        .TrimEnd('/').Replace("/api", "");

    public bool ServerAvailable { get; private set; }

    /// <summary>Spec R6 capability: does this server deliver TaskDef/Task runtimeMetadata?</summary>
    public bool RuntimeMetadataCapable { get; private set; }

    public async Task InitializeAsync()
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        try
        {
            var resp = await http.GetAsync($"{ServerBase}/health");
            ServerAvailable = resp.IsSuccessStatusCode;
        }
        catch
        {
            ServerAvailable = false;
        }

        if (ServerAvailable)
            RuntimeMetadataCapable = await ProbeRuntimeMetadataCapabilityAsync();
    }

    /// <summary>
    /// Capability probe (spec R6 SHOULD): register a scratch TaskDef with
    /// runtimeMetadata, read it back, and check the field survived. Guards the
    /// credential wire-delivery assertions against a server that doesn't yet
    /// carry conductor-oss PR #1255 / agentspan > 0.4.2.
    /// </summary>
    private async Task<bool> ProbeRuntimeMetadataCapabilityAsync()
    {
        const string probeName = "__e2e_runtime_metadata_probe__";
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var body = JsonSerializer.Serialize(new[] { new { name = probeName, runtimeMetadata = new[] { "PROBE" } } });
            using var content = new StringContent(body, Encoding.UTF8, "application/json");
            var postResp = await http.PostAsync($"{ServerBase}/api/metadata/taskdefs", content);
            if (!postResp.IsSuccessStatusCode) return false;

            var getResp = await http.GetAsync($"{ServerBase}/api/metadata/taskdefs/{probeName}");
            if (!getResp.IsSuccessStatusCode) return false;
            var node = JsonNode.Parse(await getResp.Content.ReadAsStringAsync());
            var stamped = node?["runtimeMetadata"]?.AsArray();
            return stamped is { Count: > 0 } && stamped[0]?.GetValue<string>() == "PROBE";
        }
        catch
        {
            return false;
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    /// <summary>
    /// Call at the start of every test.  Skips via SkippableException when the
    /// server is unavailable so CI stays green even without a running server.
    /// </summary>
    public void RequireServer()
    {
        Skip.IfNot(ServerAvailable, "Conductor server is not reachable — skipping e2e test.");
    }

    /// <summary>
    /// Call in addition to <see cref="RequireServer"/> for tests asserting
    /// wire-delivered credential values — skips (not fails) on a server that
    /// doesn't support the runtimeMetadata contract yet (spec R6).
    /// </summary>
    public void RequireRuntimeMetadataCapability()
    {
        Skip.IfNot(RuntimeMetadataCapable,
            "Server does not support the runtimeMetadata credential contract "
            + "(needs agentspan > 0.4.2 / conductor-oss PR #1255) — skipping wire-delivery assertions.");
    }

    /// <summary>
    /// Fetch a workflow execution from the server API for runtime-state
    /// assertions. Mirrors Python's <c>_get_workflow(execution_id)</c> helper
    /// used in suites 6, 10, 12, 14 to inspect compiled task graphs after a
    /// <c>RunAsync</c> call.
    /// </summary>
    public async Task<JsonNode?> FetchWorkflowAsync(string executionId)
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        var resp = await http.GetAsync($"{ServerBase}/api/workflow/{executionId}?includeTasks=true");
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadAsStringAsync();
        return JsonNode.Parse(body);
    }
}

[CollectionDefinition("E2e")]
public sealed class E2eCollection : ICollectionFixture<E2eFixture> { }
