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
/// Fix #1 — ConversationMemory parity with Python (messages + maxMessages,
/// trim semantics preserving system messages).
/// </summary>
public class ConversationMemoryTests
{
    [Fact]
    public void AddsUserAssistantSystem_inWireShape()
    {
        var mem = new ConversationMemory();
        mem.AddUserMessage("hi");
        mem.AddAssistantMessage("hello");
        mem.AddSystemMessage("be nice");

        var msgs = mem.ToChatMessages();
        Assert.Equal(3, msgs.Count);
        Assert.Equal("user", msgs[0]["role"]);
        Assert.Equal("hi", msgs[0]["message"]);
        Assert.Equal("assistant", msgs[1]["role"]);
        Assert.Equal("system", msgs[2]["role"]);
    }

    [Fact]
    public void Trim_keeps_system_and_drops_oldest_nonsystem()
    {
        var mem = new ConversationMemory(maxMessages: 3);
        mem.AddSystemMessage("sys");
        mem.AddUserMessage("u1");
        mem.AddAssistantMessage("a1");
        mem.AddUserMessage("u2"); // now 4 > 3 -> trim oldest non-system (u1)

        var msgs = mem.ToChatMessages();
        Assert.Equal(3, msgs.Count);
        // system preserved at its original position
        Assert.Equal("system", msgs[0]["role"]);
        Assert.Equal("sys", msgs[0]["message"]);
        // u1 dropped, a1 + u2 retained in order
        Assert.Equal("a1", msgs[1]["message"]);
        Assert.Equal("u2", msgs[2]["message"]);
    }

    [Fact]
    public void Serializes_to_messages_and_maxMessages()
    {
        var mem = new ConversationMemory(maxMessages: 10);
        mem.AddUserMessage("hi");

        var cfg = mem.ToMemoryConfig();
        Assert.True(cfg.ContainsKey("messages"));
        Assert.Equal(10, cfg["maxMessages"]);
    }

    [Fact]
    public void Empty_memory_serializes_to_empty_config()
    {
        var mem = new ConversationMemory();
        var cfg = mem.ToMemoryConfig();
        Assert.False(cfg.ContainsKey("messages"));
        Assert.False(cfg.ContainsKey("maxMessages"));
    }
}
