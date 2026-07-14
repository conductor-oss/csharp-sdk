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
// Composable Termination — AND/OR rules for stopping agents.
//
// Combines termination conditions using & (AND) and | (OR) operators:
//   - TextMentionTermination: stop when output contains specific text
//   - StopMessageTermination: stop on exact match
//   - MaxMessageTermination: stop after N messages
//   - TokenUsageTermination: stop when token budget exceeded
//
// Requirements:
//   - Agentspan server with LLM support
//   - AGENTSPAN_SERVER_URL=http://localhost:8080/api in environment
//   - AGENTSPAN_LLM_MODEL set in environment

using Conductor.AI;
using Conductor.AI.Examples;

await using var runtime = new AgentRuntime();

// ── Example 1: Simple text mention ───────────────────────────────────

var agent1 = new Agent("researcher")
{
    Model = Settings.LlmModel,
    Instructions = "Research the topic and say DONE when you have enough info.",
    Termination = new TextMentionTermination("DONE"),
};

Console.WriteLine("--- Simple text mention termination ---");
var result1 = await runtime.RunAsync(agent1, "What are AI agents?");
result1.PrintResult();
