# LangChain

> **Not available in the .NET SDK.** There is no LangChain adapter in
> `conductor-ai`. This page exists to keep the documentation structure aligned with
> the sibling SDKs; see the [Python SDK LangChain guide](https://github.com/conductor-oss/python-sdk/blob/main/docs/agents/frameworks/langchain.md)
> if you need it there.

## What to use instead

LangChain is a Python-first framework with no first-party .NET distribution, so
there is nothing for an adapter to bridge. The .NET equivalents:

- **[Semantic Kernel](semantic-kernel.md)** — the closest analogue for .NET. If you
  have existing orchestration logic you want to reuse, `[KernelFunction]` methods map
  onto agent tools directly.
- **[The native agent API](../concepts/agents.md)** — chains and agent loops are
  expressed as `Strategy.Sequential` pipelines or `Strategy.Handoff` teams. See
  [../concepts/multi-agent.md](../concepts/multi-agent.md).
- **[Tools](../concepts/tools.md)** — retrieval that would use a LangChain
  vectorstore maps onto `RagTools.Index` / `RagTools.Search`, which run server-side.

## If you need LangChain specifically

Run the LangChain-authored agent on the Python SDK against the same Conductor server.
Agents deployed from any SDK are visible to all of them, and a .NET agent can call a
Python-deployed agent by name with `runtime.StartByNameAsync(...)` — see
[../concepts/deploy-serve-run.md](../concepts/deploy-serve-run.md#the-four-verbs).
