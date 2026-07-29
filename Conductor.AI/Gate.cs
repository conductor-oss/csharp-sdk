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
namespace Conductor.AI;

/// <summary>
/// Stops a sequential pipeline if the agent's output contains the given text.
///
/// <para>When attached to an agent in a sequential pipeline (<c>a &gt;&gt; b</c>),
/// the pipeline halts after this agent if its output contains the sentinel text;
/// otherwise execution continues to the next stage. Compiled entirely server-side
/// (inline check) — no worker round-trip.</para>
/// </summary>
/// <example><code>
/// var checker = new Agent("checker") { Model = "openai/gpt-4o", Gate = new TextGate("STOP") };
/// var fixer   = new Agent("fixer")   { Model = "openai/gpt-4o" };
/// var pipeline = checker &gt;&gt; fixer;
/// </code></example>
public sealed class TextGate
{
    public string Text { get; }
    public bool CaseSensitive { get; }

    public TextGate(string text, bool caseSensitive = true)
    {
        Text = text;
        CaseSensitive = caseSensitive;
    }
}
