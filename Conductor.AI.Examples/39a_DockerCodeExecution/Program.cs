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
// Docker-Sandboxed Code Execution — run LLM-generated code in a container.
//
// The agent writes Python code and the DockerExecutor tool runs it inside an
// isolated Docker container: no network access, limited memory, and the host
// filesystem is untouched.
//
// In C#, Docker sandboxing is implemented as a local worker tool that calls
// `docker run` via subprocess. The LLM calls execute_in_docker like any
// other tool and sees its stdout/stderr output.
//
// Requirements:
//   - Conductor server running at CONDUCTOR_SERVER_URL
//   - CONDUCTOR_AGENT_LLM_MODEL set in environment
//   - Docker installed and daemon running
//   - python:3.12-slim image available (docker pull python:3.12-slim)

using System.Diagnostics;
using Conductor.AI;
using Conductor.AI.Examples;

var agent = new Agent("docker_coder_39a")
{
    Model = Settings.LlmModel,
    Tools = ToolRegistry.FromInstance(new DockerExecutor()),
    Instructions =
        "You write Python code that runs in a sandboxed Docker container. " +
        "You have no network access. Write self-contained code and call execute_in_docker.",
};

Console.WriteLine("--- Docker Sandboxed Code Execution ---");
await using var runtime = new AgentRuntime();
var result = await runtime.RunAsync(
    agent,
    "Print Python's version and compute 2 ** 100.");
result.PrintResult();

// ── Docker executor tool ─────────────────────────────────────────────

internal sealed class DockerExecutor
{
    [Tool("Execute Python code in an isolated Docker container. Returns stdout/stderr.")]
    public async Task<string> ExecuteInDocker(string code)
    {
        using var proc = new Process();
        proc.StartInfo = new ProcessStartInfo
        {
            FileName = "docker",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        proc.StartInfo.ArgumentList.Add("run");
        proc.StartInfo.ArgumentList.Add("--rm");
        proc.StartInfo.ArgumentList.Add("--network=none");
        proc.StartInfo.ArgumentList.Add("-m");
        proc.StartInfo.ArgumentList.Add("256m");
        proc.StartInfo.ArgumentList.Add("python:3.12-slim");
        proc.StartInfo.ArgumentList.Add("python");
        proc.StartInfo.ArgumentList.Add("-c");
        proc.StartInfo.ArgumentList.Add(code);

        proc.Start();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var stdout = await proc.StandardOutput.ReadToEndAsync(cts.Token);
        var stderr = await proc.StandardError.ReadToEndAsync(cts.Token);
        await proc.WaitForExitAsync(cts.Token);

        return proc.ExitCode == 0
            ? stdout.Trim()
            : $"Error (exit {proc.ExitCode}): {stderr.Trim()}";
    }
}
