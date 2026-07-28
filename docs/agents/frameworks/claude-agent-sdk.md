# Claude Agent SDK

> **Not available in the .NET SDK.** There is no Claude Agent SDK adapter in
> `conductor-ai`. This page exists to keep the documentation structure aligned with
> the sibling SDKs; see the [Python SDK Claude Agent SDK guide](https://github.com/conductor-oss/python-sdk/blob/main/docs/agents/frameworks/claude-agent-sdk.md)
> if you need it there.

## Using Claude models

The absence of an *adapter* does not mean the absence of Claude support. Anthropic
models are available to any agent through the normal `Model` property — the provider
prefix routes it:

```csharp
var agent = new Agent("assistant")
{
    Model        = "anthropic/claude-sonnet-4-6",
    Instructions = "You are helpful.",
};
```

This is the model string used throughout these docs. What the adapter would add is
the Claude Agent SDK's *authoring shape*, not the model access.

## What to use instead

- **[The native agent API](../concepts/agents.md)** — `Agent` with `[Tool]` methods
  covers the same ground as a Claude Agent SDK agent with tools.
- **[Skills](#skills)** — see below; the SDK can load agentskills.io skill
  directories directly.
- **[../concepts/multi-agent.md](../concepts/multi-agent.md)** — subagent delegation
  maps onto `Strategy.Handoff` or `AgentTool.Create(...)` for inline calls.

## Skills

`Conductor.AI.Skill` loads agentskills.io skill directories as agents, which is the
closest thing in this SDK to the Claude Agent SDK's skill loading:

```csharp
var agent = Skill.Load("path/to/skill-directory", model: "anthropic/claude-sonnet-4-6");
```

`Skill.Load(path, model?, agentModels?, parameters?, searchPath?)` requires a
`SKILL.md` in the directory. `Skill.LoadSkills(path, model?, searchPath?)` loads every
skill subdirectory under a root, returning a `Dictionary<string, Agent>` keyed by skill
name. Skill scripts and resources become local worker handlers via
`Skill.CreateSkillWorkers(agent)`, so these agents run through `AgentRuntime`.

> Note the terminology overlap: in this SDK, **Skill** means an agentskills.io skill
> directory loaded as an `Agent`. See `CONTEXT.md` at the repo root.
