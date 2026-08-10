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
// Suite 20 — Live SSE streaming through the rebuilt OrkesAgentClient transport
// (spec R1/R2): the stream must complete with a terminal Done event carrying
// finish reason + output text, using the same Configuration.AccessToken as the
// rest of the client (no bespoke token logic).

using System.Threading.Tasks;
using Xunit;
using Conductor.AI.Examples;

namespace Conductor.AI.E2eTests;

[Collection("E2e")]
public sealed class Suite20_Streaming
{
    private readonly E2eFixture _fixture;
    public Suite20_Streaming(E2eFixture fixture) => _fixture = fixture;

    [SkippableFact]
    public async Task StreamAsync_YieldsEventsAndTerminatesWithDone()
    {
        _fixture.RequireServer();

        var agent = new Agent("s20_stream_haiku")
        {
            Model = Settings.LlmModel,
            Instructions = "You are a haiku poet. Write a single haiku and stop.",
        };

        await using var runtime = new AgentRuntime();

        var sawDone = false;
        string? finalContent = null;
        await foreach (var ev in runtime.StreamAsync(agent, "Write a haiku about testing software."))
        {
            if (ev.Type == EventType.Done)
            {
                sawDone = true;
                finalContent = ev.Content;
            }
        }

        Assert.True(sawDone, "Expected a terminal Done event from the SSE stream.");
        Assert.False(string.IsNullOrWhiteSpace(finalContent));
    }
}
