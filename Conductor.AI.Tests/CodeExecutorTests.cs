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
using Conductor.AI;
using Xunit;

namespace Conductor.AI.Tests;

/// <summary>
/// Fix #2 — DockerCodeExecutor mirroring Python's. We do NOT run Docker in
/// unit tests; we assert the config surface + that a missing-docker run
/// returns a structured (non-throwing) error result.
/// </summary>
public class DockerCodeExecutorTests
{
    [Fact]
    public void Defaults_match_python()
    {
        var exec = new DockerCodeExecutor();
        Assert.Equal("python:3.12-slim", exec.Image);
        Assert.Equal("python", exec.Language);
        Assert.Equal(30, exec.Timeout);
        Assert.False(exec.NetworkEnabled);
        Assert.Null(exec.MemoryLimit);
        Assert.Empty(exec.Volumes);
    }

    [Fact]
    public void Config_surface_is_settable()
    {
        var exec = new DockerCodeExecutor(
            image: "node:20",
            language: "node",
            timeout: 15,
            networkEnabled: true,
            memoryLimit: "256m",
            volumes: new Dictionary<string, string> { ["/host"] = "/container" });

        Assert.Equal("node:20", exec.Image);
        Assert.Equal("node", exec.Language);
        Assert.Equal(15, exec.Timeout);
        Assert.True(exec.NetworkEnabled);
        Assert.Equal("256m", exec.MemoryLimit);
        Assert.Equal("/container", exec.Volumes["/host"]);
    }

    [Fact]
    public async Task Execute_returns_structured_result_never_throws()
    {
        // Use a deliberately bogus docker binary path so the call fails fast
        // without requiring (or running) a real Docker daemon. The executor
        // must return a structured ExecutionResult, not throw.
        var exec = new DockerCodeExecutor(dockerPath: "definitely-not-docker-xyz");
        var result = await exec.ExecuteAsync("print('hi')");
        Assert.False(result.Success);
        Assert.NotEqual(0, result.ExitCode);
    }
}
