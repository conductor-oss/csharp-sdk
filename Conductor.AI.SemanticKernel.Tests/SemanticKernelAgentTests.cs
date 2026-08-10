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
using System.ComponentModel;
using System.Text.Json;
using Conductor.AI;
using Conductor.AI.SemanticKernel;
using Microsoft.SemanticKernel;
using Xunit;

namespace Conductor.AI.SemanticKernel.Tests;

/// <summary>
/// Plan-level (no LLM) tests for the SK → Agentspan bridge. Each test is a
/// counterfactual: it asserts a specific property and is designed to fail if
/// that property is silently dropped during bridging.
/// </summary>
public class SemanticKernelAgentTests
{
    public sealed class MathPlugin
    {
        [KernelFunction, Description("Add two integers.")]
        public int Add(
            [Description("first")] int a,
            [Description("second")] int b) => a + b;

        [KernelFunction("multiply"), Description("Multiply two integers.")]
        public int MultiplyValues(int a, int b) => a * b;
    }

    [Fact]
    public void From_extracts_one_tool_per_KernelFunction()
    {
        var agent = SemanticKernelAgent.From("a", "anthropic/claude-sonnet-4-6", "x", new MathPlugin());

        // COUNTERFACTUAL: if methods without [KernelFunction] leak in, count > 2.
        // If [KernelFunction] methods are dropped, count < 2.
        Assert.Equal(2, agent.Tools.Count);
    }

    [Fact]
    public void Tool_name_uses_KernelFunction_attribute_override()
    {
        var agent = SemanticKernelAgent.From("a", "anthropic/claude-sonnet-4-6", "x", new MathPlugin());

        // [KernelFunction("multiply")] overrides the method name "MultiplyValues".
        // COUNTERFACTUAL: if the attribute's name is ignored, this finds the
        // method name "MultiplyValues" instead and the assertion fails.
        Assert.Contains(agent.Tools, t => t.Name == "multiply");
        Assert.DoesNotContain(agent.Tools, t => t.Name == "MultiplyValues");
    }

    [Fact]
    public void Tool_description_is_propagated()
    {
        var agent = SemanticKernelAgent.From("a", "anthropic/claude-sonnet-4-6", "x", new MathPlugin());
        var add = agent.Tools.Single(t => t.Name == "Add");

        // COUNTERFACTUAL: if Description is dropped, this is "".
        Assert.Equal("Add two integers.", add.Description);
    }

    [Fact]
    public void InputSchema_includes_parameters_with_descriptions()
    {
        var agent = SemanticKernelAgent.From("a", "anthropic/claude-sonnet-4-6", "x", new MathPlugin());
        var add = agent.Tools.Single(t => t.Name == "Add");
        var schemaJson = add.InputSchema.ToJsonString();
        using var doc = JsonDocument.Parse(schemaJson);

        // type=object
        Assert.Equal("object", doc.RootElement.GetProperty("type").GetString());

        var props = doc.RootElement.GetProperty("properties");
        Assert.True(props.TryGetProperty("a", out var aProp),
            "param 'a' missing from schema. COUNTERFACTUAL: bridge dropped a parameter.");
        Assert.True(props.TryGetProperty("b", out var bProp),
            "param 'b' missing from schema.");

        // [Description("first")] should land in the schema
        Assert.Equal("first", aProp.GetProperty("description").GetString());
        Assert.Equal("second", bProp.GetProperty("description").GetString());

        // both params are required
        var required = doc.RootElement.GetProperty("required").EnumerateArray().Select(e => e.GetString()).ToList();
        Assert.Contains("a", required);
        Assert.Contains("b", required);
    }

    [Fact]
    public void IsSemanticKernelPlugin_detects_KernelFunction_methods()
    {
        Assert.True(SemanticKernelAgent.IsSemanticKernelPlugin(new MathPlugin()));
        Assert.False(SemanticKernelAgent.IsSemanticKernelPlugin(new object()));
        Assert.False(SemanticKernelAgent.IsSemanticKernelPlugin(null));
    }
}
