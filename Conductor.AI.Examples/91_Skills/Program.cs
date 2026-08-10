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
// Skills — load an agentskills.io skill directory as an Conductor Agent.
//
// Skill scripts become worker tools, and resources under references/,
// examples/, assets/, plus root resource files, are available through the
// generated read_skill_file tool.
//
// Usage:
//   CONDUCTOR_SERVER_URL=http://localhost:8080/api \
//   CONDUCTOR_AGENT_LLM_MODEL=openai/gpt-4o-mini \
//   dotnet run --project sdk/csharp/examples/91_Skills/Example91Skills.csproj \
//     -- /path/to/skill "Review this repository"

using Conductor.AI;
using Conductor.AI.Examples;

var skillPath = args.Length > 0
    ? args[0]
    : System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".claude",
        "skills",
        "dg");
var prompt = args.Length > 1
    ? args[1]
    : "Run this skill against the current request and return a concise result.";

if (!File.Exists(System.IO.Path.Combine(skillPath, "SKILL.md")))
    throw new ArgumentException($"Expected a skill directory containing SKILL.md: {skillPath}");

var skillAgent = Skill.Load(
    skillPath,
    Settings.LlmModel,
    searchPath: [System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".agents",
        "skills")]);

await using var runtime = new AgentRuntime();
var direct = await runtime.RunAsync(skillAgent, prompt);
direct.PrintResult();

var parent = new Agent("skill_tool_manager_91")
{
    Model = Settings.LlmModel,
    Instructions = "Use the wrapped skill tool for the user request, then return the skill result.",
    Tools = [AgentTool.Create(skillAgent, description: "Run the loaded skill")],
    MaxTurns = 4,
};

var viaTool = await runtime.RunAsync(parent, prompt);
viaTool.PrintResult();
