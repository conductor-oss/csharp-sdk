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
// AgentResult.ToolCalls used to name every tool after its Conductor task type —
// correct only for a worker tool, whose executed SIMPLE task carries the tool's
// own name there, and wrong for every other kind ("HTTP", "CALL_MCP_TOOL",
// "SUB_WORKFLOW", "HUMAN", "GENERATE_IMAGE"). Detection also keyed on a
// `call_` reference-name prefix, which is the OpenAI tool-call ID format, so an
// Anthropic-backed agent recorded no tool calls at all.
//
// Fixtures here carry the shape real payloads have: the fork index and loop
// suffix on the reference name, and the `_agent_tool_name` / `_agent_state`
// keys the server's dispatch script injects.

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using Conductor.Client;
using RestSharp;
using Xunit;

namespace Conductor.AI.Tests;

public sealed class AgentToolCallExtractionTests
{
    /// <summary>Drive WaitAsync to completion over a stubbed server and return the built result.</summary>
    private static async Task<AgentResult> WaitWithTasksAsync(
        string workflowBody, string statusBody = DefaultStatus)
    {
        var configuration = StubAgentServer.Configure(
            request => StubAgentServer.Route(request, statusBody, "{}", workflowBody));
        var handle = new AgentHandle("e1", new OrkesAgentClient(configuration));
        return await handle.WaitAsync();
    }

    private const string DefaultStatus = """
        {"executionId":"e1","status":"COMPLETED","isComplete":true,"isRunning":false,
         "isWaiting":false,"output":{"result":"done"}}
        """;

    private static Dictionary<string, object> ArgsOf(Dictionary<string, object> toolCall)
        => Assert.IsType<Dictionary<string, object>>(toolCall["args"]);

    // ── Tool name comes from the payload, never from the task type ───────

    [Fact]
    public async Task WorkerTool_NameFromAgentToolName()
    {
        // A worker tool's executed SIMPLE task carries the tool name in taskType
        // too, so this is the one kind the old taskType read got right. It is
        // here to pin that the marker-based resolution agrees with it.
        var result = await WaitWithTasksAsync("""
            {"tasks":[
                {"taskType":"get_weather","taskDefName":"get_weather",
                 "referenceTaskName":"call_PMnNIdOPvm9EQ8e6tn2kbxPY_0__1",
                 "inputData":{"_agent_tool_name":"get_weather","_agent_state":{},
                              "method":"get_weather","city":"San Francisco"},
                 "outputData":{"result":"Sunny, 72F"}}
            ]}
            """);

        var toolCall = Assert.Single(result.ToolCalls!);
        Assert.Equal("get_weather", toolCall["name"]);
        Assert.Equal(["city"], ArgsOf(toolCall).Keys);
        Assert.Equal("Sunny, 72F", toolCall["result"]!.ToString());
    }

    [Theory]
    // Every non-worker kind the server's ToolCompiler.TYPE_MAP can produce: the
    // task type is the system type, and the tool's real name is in inputData.
    [InlineData("HTTP", "lookup_order")]
    [InlineData("CALL_MCP_TOOL", "search_docs")]
    [InlineData("SUB_WORKFLOW", "billing_agent")]
    [InlineData("HUMAN", "escalate")]
    [InlineData("GENERATE_IMAGE", "draw_chart")]
    [InlineData("GENERATE_AUDIO", "read_aloud")]
    [InlineData("GENERATE_VIDEO", "animate")]
    [InlineData("GENERATE_PDF", "make_report")]
    [InlineData("LLM_INDEX_TEXT", "index_docs")]
    [InlineData("LLM_SEARCH_INDEX", "search_index")]
    [InlineData("PULL_WORKFLOW_MESSAGES", "read_queue")]
    public async Task SystemTaskTool_NameFromAgentToolName_NotTaskType(string taskType, string toolName)
    {
        var result = await WaitWithTasksAsync("""
            {"tasks":[
                {"taskType":"TYPE","taskDefName":"NAME","referenceTaskName":"NAME_0__1",
                 "inputData":{"_agent_tool_name":"NAME","query":"x"},
                 "outputData":{"result":"ok"}}
            ]}
            """.Replace("TYPE", taskType).Replace("NAME", toolName));

        var toolCall = Assert.Single(result.ToolCalls!);
        Assert.Equal(toolName, toolCall["name"]);
        Assert.NotEqual(taskType, toolCall["name"]);
    }

    [Fact]
    public async Task NameFallsBackToMethod_WhenAgentToolNameAbsent()
    {
        // The dynamic-tools dispatch script sets no `_agent_tool_name`, but an
        // MCP task carries `method` — the tool name the LLM called.
        var result = await WaitWithTasksAsync("""
            {"tasks":[
                {"taskType":"CALL_MCP_TOOL","taskDefName":"call_mcp_tool",
                 "referenceTaskName":"search_docs_0",
                 "inputData":{"mcpServer":"docs","method":"search_docs","arguments":{"q":"x"}},
                 "outputData":{"result":["a"]}}
            ]}
            """);

        Assert.Equal("search_docs", Assert.Single(result.ToolCalls!)["name"]);
    }

    [Fact]
    public async Task NameFallsBackToTaskDefName_WhenNeitherMarkerPresent()
    {
        var result = await WaitWithTasksAsync("""
            {"tasks":[
                {"taskType":"GENERATE_IMAGE","taskDefName":"generate_image",
                 "referenceTaskName":"generate_image_0",
                 "inputData":{"prompt":"a cat"},
                 "outputData":{"result":"http://img"}}
            ]}
            """);

        Assert.Equal("generate_image", Assert.Single(result.ToolCalls!)["name"]);
    }

    // ── Detection no longer keys on the provider's tool-call ID ──────────

    [Fact]
    public async Task AnthropicReferenceName_StillDetected()
    {
        // Anthropic tool-call IDs start `toolu_`; the reference name is seeded
        // from the provider's ID, so a `call_` prefix test found nothing here.
        var result = await WaitWithTasksAsync("""
            {"tasks":[
                {"taskType":"get_weather","taskDefName":"get_weather",
                 "referenceTaskName":"toolu_01A09q90qw90lq917835lq9_0__1",
                 "inputData":{"_agent_tool_name":"get_weather","_agent_state":{},"city":"Paris"},
                 "outputData":{"result":"Rainy"}}
            ]}
            """);

        Assert.Equal("get_weather", Assert.Single(result.ToolCalls!)["name"]);
    }

    [Fact]
    public async Task WorkerToolWithoutToolNameMarker_DetectedByAgentState()
    {
        // The dynamic-tools script injects `_agent_state` but not
        // `_agent_tool_name`, and a worker tool's task type is the tool's own
        // name, so it cannot be recognised from an allowlist of system types.
        var result = await WaitWithTasksAsync("""
            {"tasks":[
                {"taskType":"echo","taskDefName":"echo","referenceTaskName":"toolu_xyz_0",
                 "inputData":{"_agent_state":{},"method":"echo","text":"hi"},
                 "outputData":{"result":"hi"}}
            ]}
            """);

        Assert.Equal("echo", Assert.Single(result.ToolCalls!)["name"]);
    }

    [Fact]
    public async Task NonToolTasks_Excluded()
    {
        // The agent's own scaffolding, plus the INLINE task the dispatch script
        // substitutes for a hallucinated tool name — none of these is a tool call.
        var result = await WaitWithTasksAsync("""
            {"tasks":[
                {"taskType":"LLM_CHAT_COMPLETE","taskDefName":"llm_chat_complete",
                 "referenceTaskName":"llm_1","inputData":{},"outputData":{"promptTokens":10}},
                {"taskType":"FORK_JOIN_DYNAMIC","taskDefName":"fork","referenceTaskName":"fork_1",
                 "inputData":{},"outputData":{}},
                {"taskType":"JOIN","taskDefName":"join","referenceTaskName":"join_1",
                 "inputData":{},"outputData":{}},
                {"taskType":"INLINE","taskDefName":"made_up_tool","referenceTaskName":"made_up_tool",
                 "inputData":{"evaluatorType":"graaljs","expression":"...","errorMessage":"Unknown tool"},
                 "outputData":{"result":"Unknown tool 'made_up_tool'.","is_error":true}},
                {"taskType":"SWITCH","taskDefName":"switch","referenceTaskName":"switch_1",
                 "inputData":{},"outputData":{}},
                {"taskType":"DO_WHILE","taskDefName":"loop","referenceTaskName":"loop_1",
                 "inputData":{},"outputData":{}},
                {"taskType":"SET_VARIABLE","taskDefName":"set_var","referenceTaskName":"set_var_1",
                 "inputData":{},"outputData":{}}
            ]}
            """);

        Assert.Null(result.ToolCalls);
    }

    [Fact]
    public async Task MultiAgentSetVariable_NotAToolCall()
    {
        // A multi-agent coordinator's SET_VARIABLE task carries `_agent_state`
        // in its inputs, so the dispatch marker alone does not make a task a
        // tool call — the worker case also needs the executed-SIMPLE signature
        // of a task type equal to its own taskDefName.
        var result = await WaitWithTasksAsync("""
            {"tasks":[
                {"taskType":"SET_VARIABLE","taskDefName":"triage_init",
                 "referenceTaskName":"triage_init",
                 "inputData":{"conversation":"hello","_agent_state":{"turn":1}},
                 "outputData":{}}
            ]}
            """);

        Assert.Null(result.ToolCalls);
    }

    [Fact]
    public async Task WorkerTaskWithoutDispatchMarker_NotAToolCall()
    {
        // The framework-passthrough wrapper is a SIMPLE task named after a
        // worker, but the LLM never dispatched it, so it carries no marker.
        var result = await WaitWithTasksAsync("""
            {"tasks":[
                {"taskType":"claude_code","taskDefName":"claude_code",
                 "referenceTaskName":"_fw_task",
                 "inputData":{"prompt":"hi","session_id":"s1"},
                 "outputData":{"result":"hello"}}
            ]}
            """);

        Assert.Null(result.ToolCalls);
    }

    [Theory]
    // The agent layer emits these types for its own structure: a sub-agent, a
    // strategy workflow, a router and a plan execution are all SUB_WORKFLOW, and
    // a plan's approval step is HUMAN. None is dispatched by the LLM, so none
    // carries a dispatch marker, and none may be reported as a tool call.
    [InlineData("SUB_WORKFLOW", "billing_strategy")]
    [InlineData("SUB_WORKFLOW", "triage_router")]
    [InlineData("HUMAN", "plan_approval")]
    public async Task AgentStructureReusingAToolTaskType_NotAToolCall(string taskType, string taskDefName)
    {
        var result = await WaitWithTasksAsync("""
            {"tasks":[
                {"taskType":"TYPE","taskDefName":"NAME","referenceTaskName":"0_NAME__1",
                 "inputData":{"prompt":"hello","media":[],"session_id":"s1",
                              "context":{"turn":1}},
                 "outputData":{"result":"handled"}}
            ]}
            """.Replace("TYPE", taskType).Replace("NAME", taskDefName));

        Assert.Null(result.ToolCalls);
        Assert.Single(result.Events!);
    }

    [Fact]
    public async Task AgentAsTool_DetectedByDispatchMarker()
    {
        // The same SUB_WORKFLOW type is a genuine tool call when the dispatch
        // script marked it as one.
        var result = await WaitWithTasksAsync("""
            {"tasks":[
                {"taskType":"SUB_WORKFLOW","taskDefName":"billing_agent_wf",
                 "referenceTaskName":"toolu_01xyz_0__1",
                 "inputData":{"_agent_tool_name":"billing_agent","prompt":"refund order A-1",
                              "session_id":"s1"},
                 "outputData":{"result":"refunded"}}
            ]}
            """);

        Assert.Equal("billing_agent", Assert.Single(result.ToolCalls!)["name"]);
    }

    [Fact]
    public async Task InternalInputKeys_StrippedFromArgs()
    {
        var result = await WaitWithTasksAsync("""
            {"tasks":[
                {"taskType":"HTTP","taskDefName":"lookup","referenceTaskName":"lookup_0",
                 "inputData":{"_agent_tool_name":"lookup","_agent_state":{},"method":"lookup",
                              "ctx":"x","workerTag":"y","agentConfig":{},"order":"A-1"},
                 "outputData":{"result":"shipped"}}
            ]}
            """);

        Assert.Equal(["order"], ArgsOf(Assert.Single(result.ToolCalls!)).Keys);
    }

    // ── Events ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Events_ToolCallAndResultPerToolTask_ThenDone()
    {
        var result = await WaitWithTasksAsync("""
            {"tasks":[
                {"taskType":"HTTP","taskDefName":"lookup_order","referenceTaskName":"lookup_order_0",
                 "inputData":{"_agent_tool_name":"lookup_order","order":"A-1"},
                 "outputData":{"result":"shipped"}}
            ]}
            """);

        Assert.Collection(
            result.Events!,
            e =>
            {
                Assert.Equal(EventType.ToolCall, e.Type);
                Assert.Equal("lookup_order", e.ToolName);
                Assert.Equal(["order"], e.Args!.Keys);
            },
            e =>
            {
                Assert.Equal(EventType.ToolResult, e.Type);
                Assert.Equal("lookup_order", e.ToolName);
                Assert.Equal("shipped", e.Result!.ToString());
            },
            e =>
            {
                Assert.Equal(EventType.Done, e.Type);
                Assert.Equal("e1", e.ExecutionId);
            });
    }

    [Fact]
    public async Task Events_NeverNull_SoEnumerationDoesNotThrow()
    {
        var result = await WaitWithTasksAsync("""{"tasks":[]}""");

        Assert.NotNull(result.Events);
        var terminal = Assert.Single(result.Events!);
        Assert.Equal(EventType.Done, terminal.Type);
    }

    [Fact]
    public async Task Events_FailedRun_EndsWithErrorNotDone()
    {
        var result = await WaitWithTasksAsync("""{"tasks":[]}""", statusBody: """
            {"executionId":"e1","status":"FAILED","isComplete":true,"isRunning":false,
             "isWaiting":false,"reasonForIncompletion":"tool worker never polled"}
            """);

        var terminal = Assert.Single(result.Events!);
        Assert.Equal(EventType.Error, terminal.Type);
        Assert.Equal("tool worker never polled", terminal.Content);
    }

    [Fact]
    public async Task HttpToolResult_FallsBackToWholeOutput()
    {
        // An HTTP tool answers under `response`, not `result`, so keying only on
        // `result` would report a tool call with no result at all.
        var result = await WaitWithTasksAsync("""
            {"tasks":[
                {"taskType":"HTTP","taskDefName":"lookup_order","referenceTaskName":"lookup_order_0",
                 "inputData":{"_agent_tool_name":"lookup_order","order":"A-1"},
                 "outputData":{"response":{"status":"shipped"},"statusCode":200}}
            ]}
            """);

        var toolResult = Assert.IsType<JsonElement>(Assert.Single(result.ToolCalls!)["result"]);
        Assert.Equal(
            ["response", "statusCode"],
            toolResult.EnumerateObject().Select(p => p.Name));
    }
}
