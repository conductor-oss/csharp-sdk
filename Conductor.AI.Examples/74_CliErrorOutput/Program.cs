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
// CLI Error Output — verify the agent sees stdout/stderr on non-zero exit.
//
// Runs an agent that deliberately triggers a failing CLI command and
// then asks the agent to report what it saw. The test passes when the
// agent's final output mentions the error from the failed command.
//
// CliTool.Create() registers a local run_command worker that uses
// System.Diagnostics.Process. The LLM calls it via function calling.
//
// Requirements:
//   - AGENTSPAN_SERVER_URL=http://localhost:6767/api in environment
//   - AGENTSPAN_LLM_MODEL set in environment

using Conductor.AI;
using Conductor.AI.Examples;

var agent = new Agent("cli_error_tester_74")
{
    Model = Settings.LlmModel,
    Tools = [CliTool.Create(allowedCommands: ["ls"])],
    Instructions =
        "You have a run_command tool. " +
        "Run the exact command the user asks you to run, then report " +
        "the full stdout and stderr you received from the tool result.",
};

await using var runtime = new AgentRuntime();
var result = await runtime.RunAsync(
    agent,
    "Run: ls /nonexistent_path_that_does_not_exist_74\n" +
    "Then tell me the exact stderr you got back.");
result.PrintResult();

// Verify the agent saw the error output
var output = result.Output?.GetValueOrDefault("result")?.ToString() ?? result.Error ?? "";
var hasError = output.Contains("No such file or directory", StringComparison.OrdinalIgnoreCase)
            || output.Contains("nonexistent", StringComparison.OrdinalIgnoreCase)
            || output.Contains("cannot access", StringComparison.OrdinalIgnoreCase);
Console.WriteLine(hasError
    ? "\nPASS: agent correctly reported CLI error output"
    : $"\nFAIL: agent did not surface CLI error output. Got: {output[..Math.Min(200, output.Length)]}");
