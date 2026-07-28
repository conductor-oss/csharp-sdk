# Conductor .NET SDK

The .NET client SDK for Conductor/Orkes — workflow and task primitives, plus the
Agentspan durable AI-agent layer.

## Language

Unqualified, the terms below always mean the SDK types. The coding-agent-harness
sense is always qualified.

**Agent**:
The single orchestration primitive — an LLM with tools, or a multi-agent system.
_Avoid_: assistant, bot. For a Claude Code subagent say **coding agent**.

**Skill**:
An agentskills.io skill directory loaded as an Agent.
_Avoid_: plugin, capability. For an installed Claude Code skill say **Claude Code skill**.

**Tool**:
A named capability an Agent can invoke to act beyond generating text.
_Avoid_: function, action. For a harness tool (Read, Bash) say **harness tool**.

**Handoff**:
A transfer of control from one Agent to another named Agent.
_Avoid_: delegation, escalation. For the `claude-handoff` skill say **session handoff**.
