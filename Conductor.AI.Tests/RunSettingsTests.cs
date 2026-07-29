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
// T12/T13 — RunSettings overrides applied to the serialized start payload: full
// override, partial override, zero-value fields honored (`!= null` gate, not
// truthiness), no-runSettings leaves the payload untouched, and the framework
// (`rawConfig`) wire-envelope variant.

using System.Text.Json.Nodes;
using Conductor.AI;
using Xunit;

namespace Conductor.AI.Tests;

public sealed class RunSettingsTests
{
    private static JsonObject Payload(string configKey = "agentConfig") => new()
    {
        [configKey] = new JsonObject { ["model"] = "openai/gpt-4o" },
        ["prompt"] = "hi",
    };

    [Fact]
    public void FullOverride_AppliesAllFields()
    {
        var payload = Payload();
        var rs = new RunSettings(
            Model: "openai/o3",
            Temperature: 0.2,
            MaxTokens: 2048,
            ReasoningEffort: "high",
            ThinkingBudgetTokens: 4096);

        rs.ApplyToPayload(payload);

        var root = payload["agentConfig"]!.AsObject();
        Assert.Equal("openai/o3", root["model"]!.GetValue<string>());
        Assert.Equal(0.2, root["temperature"]!.GetValue<double>());
        Assert.Equal(2048, root["maxTokens"]!.GetValue<int>());
        Assert.Equal("high", root["reasoningEffort"]!.GetValue<string>());
        var thinking = root["thinkingConfig"]!.AsObject();
        Assert.True(thinking["enabled"]!.GetValue<bool>());
        Assert.Equal(4096, thinking["budgetTokens"]!.GetValue<int>());
    }

    [Fact]
    public void NoRunSettings_PayloadUnchanged()
    {
        var payload = Payload();
        RunSettings? rs = null;

        rs?.ApplyToPayload(payload);

        var root = payload["agentConfig"]!.AsObject();
        Assert.Equal("openai/gpt-4o", root["model"]!.GetValue<string>());
        Assert.False(root.ContainsKey("temperature"));
    }

    [Fact]
    public void PartialOverride_OnlyProvidedFieldsChange()
    {
        var payload = Payload();
        var rs = new RunSettings(Temperature: 0.7);

        rs.ApplyToPayload(payload);

        var root = payload["agentConfig"]!.AsObject();
        Assert.Equal("openai/gpt-4o", root["model"]!.GetValue<string>()); // untouched
        Assert.Equal(0.7, root["temperature"]!.GetValue<double>());
        Assert.False(root.ContainsKey("maxTokens"));
        Assert.False(root.ContainsKey("reasoningEffort"));
        Assert.False(root.ContainsKey("thinkingConfig"));
    }

    [Fact]
    public void ZeroValues_AreHonored_NullCheckNotTruthiness()
    {
        var payload = Payload();
        var rs = new RunSettings(Temperature: 0.0, MaxTokens: 0);

        rs.ApplyToPayload(payload);

        var root = payload["agentConfig"]!.AsObject();
        Assert.Equal(0.0, root["temperature"]!.GetValue<double>());
        Assert.Equal(0, root["maxTokens"]!.GetValue<int>());
    }

    [Fact]
    public void FrameworkVariant_AppliesToRawConfigKey()
    {
        var payload = Payload(configKey: "rawConfig");
        var rs = new RunSettings(Model: "openai/o3");

        rs.ApplyToPayload(payload);

        Assert.Equal("openai/o3", payload["rawConfig"]!.AsObject()["model"]!.GetValue<string>());
    }

    [Fact]
    public void NoTopP_NotPartOfTheContract()
    {
        // Compile-time assertion: RunSettings has no TopP member (spec R8 exclusion).
        Assert.Null(typeof(RunSettings).GetProperty("TopP"));
    }
}
