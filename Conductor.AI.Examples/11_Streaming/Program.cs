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
// Streaming — real-time event stream from an agent execution.
//
// Demonstrates StreamAsync() which yields AgentEvent objects as the
// agent thinks, calls tools, and completes.
//
// Requirements:
//   - Agentspan server with LLM support
//   - AGENTSPAN_SERVER_URL=http://localhost:6767/api in environment
//   - AGENTSPAN_LLM_MODEL set in environment

using Conductor.AI;
using Conductor.AI.Examples;

var agent = new Agent("haiku_writer")
{
    Model = Settings.LlmModel,
    Instructions = "You are a haiku poet. Write a single haiku.",
};

Console.WriteLine("Streaming agent execution:");
Console.WriteLine(new string('-', 40));

await using var runtime = new AgentRuntime();

await foreach (var ev in runtime.StreamAsync(agent, "Write a haiku about C# programming."))
{
    switch (ev.Type)
    {
        case EventType.Thinking:
            Console.WriteLine($"  [thinking] {ev.Content}");
            break;
        case EventType.ToolCall:
            Console.WriteLine($"  [tool_call] {ev.ToolName}");
            break;
        case EventType.ToolResult:
            Console.WriteLine($"  [tool_result] {ev.ToolName}");
            break;
        case EventType.Waiting:
            Console.WriteLine("  [waiting...]");
            break;
        case EventType.Done:
            Console.WriteLine();
            Console.WriteLine($"Result: {ev.Content}");
            Console.WriteLine($"Status: {ev.Status}");
            break;
        case EventType.Error:
            Console.WriteLine($"  [error] {ev.Content}");
            break;
    }
}
