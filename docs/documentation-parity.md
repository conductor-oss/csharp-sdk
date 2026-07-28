# Documentation parity

Where this SDK's documentation stands against the Java and Python SDKs. Java is the
reference structure; Python adopted it in
[python-sdk#441](https://github.com/conductor-oss/python-sdk/pull/441).

## Structural parity

| Area | Java | Python | .NET |
|---|---|---|---|
| `docs/README.md` index | ✅ | ✅ | ✅ |
| Top-level topic docs | ✅ | ✅ | ✅ |
| `docs/agents/concepts/` | ✅ | ✅ | ✅ |
| `docs/agents/frameworks/` | ✅ | ✅ | ✅ |
| `docs/agents/reference/` | ✅ | ✅ | ✅ |
| `agent-schema.json` | ✅ generated | ✅ | ⚠️ hand-maintained |

## Page-level differences

Pages present in a sibling SDK but not here, because the functionality does not exist in
.NET:

| Page | Status |
|---|---|
| `schema-client.md` | Stub — no schema registry client. See [schema-client.md](schema-client.md). |
| `agents/frameworks/langchain.md` | Stub — no LangChain adapter. |
| `agents/frameworks/langgraph.md` | Stub — no LangGraph adapter. |
| `agents/frameworks/claude-agent-sdk.md` | Stub — no Claude Agent SDK adapter. |
| `file-client.md` (Java only) | Absent. Python omits it too. |
| `spring-boot.md` (Java only) | Absent — not applicable to .NET. |

Pages present here but not in the siblings:

| Page | Reason |
|---|---|
| `agents/frameworks/semantic-kernel.md` | Semantic Kernel is a .NET-first framework; this adapter has no Python or Java counterpart. |

## Naming differences

Environment variables for the agent runtime use `CONDUCTOR_AGENT_*`, matching Java and
Python. Connection settings use `CONDUCTOR_*`, shared with the core SDK.

This SDK matches Python's *actual* behaviour: legacy `AGENTSPAN_*` names are removed
outright, not aliased. Note that Python's PR description claims it "kept legacy runtime
environment-variable aliases"; its diff does not, and its config docstring reads *"Only
`CONDUCTOR_AGENT_*` settings are supported."* The docstring is the accurate one.

This SDK goes further than Python in one respect: the public type names were renamed too
(`AgentspanException` → `ConductorAgentException`, `AgentspanJson` → `ConductorAgentJson`),
so no Agentspan naming remains in the surface this SDK owns.

See [upgrading.md](upgrading.md).

## Validation parity — the open gap

This is the substantive difference. Java enforces documentation quality in CI
(`.github/workflows/ci.yml`); this repo enforces none of it.

| Check | Java | .NET |
|---|---|---|
| Link validation (lychee, offline) | ✅ | ❌ |
| Retired-reference grep | ✅ | ❌ |
| Documented source links / test-server version | ✅ | ❌ |
| Agent schema verifier | ✅ | ❌ |

Python implemented equivalents as pytest tests (`test_documentation_links.py`,
`test_documentation_quality.py`, `test_agent_schema_contract.py`).

Consequences worth being explicit about:

- Nothing detects a broken internal link in these docs.
- Nothing detects `agent-schema.json` drifting from `AgentConfigSerializer`. Treat
  `runtime.PlanAsync(agent)` output as ground truth.
- Nothing prevents a doc from referencing a retired path.

A .NET-idiomatic closure would be CI steps for link checking and retired-reference
grepping (lychee is language-agnostic) plus xUnit tests in `Conductor.AI.Tests` for schema
contract assertions, so `dotnet test` gives local signal.

## Content coverage

The agent layer is at parity — every concept in the Java and Python `concepts/` sets has a
.NET page.

Core-SDK topic pages were authored for this restructure from the SDK source, since this
repo previously documented the core SDK only in `README.md` and two files under
`docs/readme/`. They are correspondingly newer and less battle-tested than the agent pages,
which were relocated from existing documentation.

## Known content gaps

Task-type builders the server supports but this SDK does not expose are listed in
[workflows.md](workflows.md#known-gaps) with issue links, rather than being silently
omitted from the task-type table.
