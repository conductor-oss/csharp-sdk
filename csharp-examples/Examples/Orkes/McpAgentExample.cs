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
using Conductor.Client.Extensions;
using Conductor.Client.Models;
using Conductor.Definition;
using Conductor.Definition.TaskType;
using Conductor.Definition.TaskType.LlmTasks;
using Conductor.Executor;
using System;
using System.Collections.Generic;

namespace Conductor.Examples.Orkes
{
    /// <summary>
    /// Demonstrates building an MCP (Model Context Protocol) agent workflow.
    /// The workflow:
    /// 1. Lists available MCP tools from a server
    /// 2. Uses an LLM to decide which tool to call based on user query
    /// 3. Calls the selected MCP tool
    /// 4. Returns the result to the user
    /// </summary>
    public class McpAgentExample
    {
        private readonly WorkflowExecutor _workflowExecutor;

        public McpAgentExample()
        {
            _workflowExecutor = ApiExtensions.GetWorkflowExecutor();
        }

        public ConductorWorkflow CreateMcpAgentWorkflow()
        {
            var workflow = new ConductorWorkflow()
                .WithName("mcp_agent_csharp")
                .WithDescription("MCP Agent that uses tools from an MCP server")
                .WithVersion(1);

            // Step 1: List available tools from the MCP server
            var listTools = new ListMcpToolsTask("list_tools_ref", "${workflow.input.mcp_server}");

            // Step 2: Use LLM to decide which tool to call
            var chatMessages = new List<ChatMessage>
            {
                new ChatMessage("system", "You are a helpful assistant with access to tools. Use the available tools to answer the user's question. Available tools: ${list_tools_ref.output.tools}"),
                new ChatMessage("user", "${workflow.input.user_query}")
            };
            var llmDecision = new LlmChatComplete(
                taskReferenceName: "llm_decide_ref",
                llmProvider: "${workflow.input.llm_provider}",
                model: "${workflow.input.llm_model}",
                messages: chatMessages
            );

            // Step 3: Call the selected MCP tool
            var callTool = new CallMcpToolTask(
                taskReferenceName: "call_tool_ref",
                mcpServerName: "${workflow.input.mcp_server}",
                toolName: "${llm_decide_ref.output.tool_name}",
                toolInput: new Dictionary<string, object> { { "args", "${llm_decide_ref.output.tool_args}" } }
            );

            // Step 4: Format the response with LLM
            var responseMessages = new List<ChatMessage>
            {
                new ChatMessage("system", "Format the following tool response into a user-friendly answer."),
                new ChatMessage("user", "Tool result: ${call_tool_ref.output}")
            };
            var formatResponse = new LlmChatComplete(
                taskReferenceName: "format_response_ref",
                llmProvider: "${workflow.input.llm_provider}",
                model: "${workflow.input.llm_model}",
                messages: responseMessages
            );

            workflow
                .WithTask(listTools)
                .WithTask(llmDecision)
                .WithTask(callTool)
                .WithTask(formatResponse);

            return workflow;
        }

        public void RegisterAndRun()
        {
            var workflow = CreateMcpAgentWorkflow();
            _workflowExecutor.RegisterWorkflow(workflow, true);

            var input = new Dictionary<string, object>
            {
                { "mcp_server", "weather-server" },
                { "llm_provider", "openai" },
                { "llm_model", "gpt-4" },
                { "user_query", "What's the weather in San Francisco?" }
            };

            Console.WriteLine("Starting MCP Agent workflow...");
            var workflowId = _workflowExecutor.StartWorkflow(new StartWorkflowRequest(name: workflow.Name, version: workflow.Version, input: input));
            Console.WriteLine($"MCP Agent workflow started. WorkflowId: {workflowId}");
        }
    }
}
