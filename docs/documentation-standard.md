# Documentation standard

How these docs are structured and written. This mirrors the standard used by the Java and
Python SDKs so the three sets stay navigable to the same reader.

## Layout

```
docs/
├── README.md                  ← index
├── *.md                       ← core SDK topics, one per concern
└── agents/
    ├── README.md              ← agent-layer index
    ├── getting-started.md
    ├── concepts/              ← one file per concept
    ├── frameworks/            ← one file per framework adapter
    └── reference/             ← lookup tables, schemas
```

Every directory has an index that links to its contents. A file that nothing links to is
a bug.

## The concepts / reference split

- **`concepts/`** answers *how do I do this* — prose, worked examples, the reasoning
  behind a choice.
- **`reference/`** answers *what is the exact signature* — tables, exhaustive member
  lists, no narrative.

Content belongs in exactly one of them. Concepts pages link to reference for signatures;
reference pages link back for usage. Duplicating a signature table into a concepts page is
how the two drift apart.

## Writing rules

1. **Lead with the working example.** Show the code, then explain it. A reader who only
   copies the first block should get something that runs.
2. **State defaults explicitly.** `RetryCount` (default 2) beats "configurable retries".
3. **Name the failure mode.** Where a mistake is common — approving the root execution
   instead of the event's, mismatching `TaskType` — say so where the reader will hit it.
4. **Mark what does not exist.** A capability absent from this SDK gets a page with a
   status banner and the nearest alternative, not silence. See
   [schema-client.md](schema-client.md).
5. **Link rather than repeat.** One canonical location per fact.
6. **Prefer verified fact over plausible description.** If a signature was not checked
   against the source, do not write it.

## Domain language

Use the terms defined in [`CONTEXT.md`](../CONTEXT.md) at the repo root. In particular
**Agent**, **Skill**, **Tool**, and **Handoff** unqualified always mean the SDK types; the
coding-agent-harness sense is always qualified ("coding agent", "Claude Code skill").

## Code samples

- C# with `csharp` fences; shell with `shell`.
- Compile-plausible: real type names, real parameter names, real namespaces.
- Prefer the shape a user would actually write — `await using var runtime = new AgentRuntime();`
  rather than a bare constructor call.
- `anthropic/claude-sonnet-4-6` is the conventional example model string.

## Status banners

A page documenting something unavailable in this SDK opens with a blockquote stating that
plainly, then gives the nearest supported path. Never describe absent functionality as if
it works, and never frame it as a roadmap commitment unless one exists.

## Deprecations

Deprecated surfaces stay documented as long as they are honored, with the current name
first and the legacy name marked as such. See [upgrading.md](upgrading.md).

## Validation

This repo currently has **no automated documentation validation** — no link checking, no
retired-reference grep, no schema verifier. The Java SDK enforces all three in CI. Until
that gap is closed, these conventions are maintained by review, and
[agents/reference/agent-schema.json](agents/reference/agent-schema.json) in particular is
hand-maintained against the serializer with nothing checking that it stays true. See
[documentation-parity.md](documentation-parity.md).
