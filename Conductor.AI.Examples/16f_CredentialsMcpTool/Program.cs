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
// Credentials — MCP tool with server-side credential resolution.
//
// Demonstrates:
//   - McpTools.Create() with credentials: ["MCP_API_KEY"]
//   - ${MCP_API_KEY} in headers resolved server-side before MCP calls
//   - MCP server authentication handled transparently — the C# process
//     never sees the plaintext secret
//
// MCP Test Server Setup (mcp-testkit):
//   pip install mcp-testkit
//
//   # Start with auth (to demonstrate credential resolution):
//   mcp-testkit --transport http --auth <secret>
//
//   # Store credentials via CLI or Conductor UI:
//   agentspan credentials set MCP_API_KEY <secret>
//
// Requirements:
//   - Conductor server running at CONDUCTOR_SERVER_URL
//   - CONDUCTOR_AGENT_LLM_MODEL set in environment
//   - mcp-testkit running on http://localhost:3001 (see above)
//   - MCP_API_KEY stored via `agentspan credentials set`

using Conductor.AI;
using Conductor.AI.Examples;

// MCP tool with credential-bearing headers.
// ${MCP_API_KEY} is resolved server-side from the credential store
// before each MCP call — the plaintext value never appears in code.
var mcpTools = McpTools.Create(
    serverUrl: "http://localhost:3001/mcp",
    headers: new Dictionary<string, string>
    {
        ["Authorization"] = "Bearer ${MCP_API_KEY}",
    },
    credentials: ["MCP_API_KEY"]);

var agent = new Agent("mcp_cred_agent")
{
    Model = Settings.LlmModel,
    Tools = [mcpTools],
    Instructions = "You have access to MCP tools. Use them to help the user.",
};

Console.WriteLine("=== MCP Tool with Credential Resolution ===");
await using var runtime = new AgentRuntime();
var result = await runtime.RunAsync(agent, "What tools are available?");
result.PrintResult();
