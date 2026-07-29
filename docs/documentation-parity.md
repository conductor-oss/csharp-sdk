# Documentation parity

Where this SDK's documentation stands against the Java and Python SDKs. Java is the
reference structure; Python adopted it in
[python-sdk#441](https://github.com/conductor-oss/python-sdk/pull/441).

## Structural parity — file counts

Measured against the 43 documentation files python-sdk#441 added:

| Group | Python | .NET | Missing |
|---|---|---|---|
| Top-level `docs/*.md` | 21 | 21 | **0** |
| `docs/agents/concepts/` | 11 | 11 | **0** |
| `docs/agents/reference/` | 6 | 6 | **0** |
| `docs/agents/frameworks/` | 5 | 6 | **0** (+`semantic-kernel.md`) |
| **Total** | **43** | **44** | **0** |

| Area | Java | Python | .NET |
|---|---|---|---|
| `docs/README.md` index | ✅ | ✅ | ✅ |
| `docs/agents/README.md` index | ✅ | ✅ | ✅ |
| Root `README.md` as navigation hub | ✅ | ✅ | ✅ |
| Examples index README | — | ✅ | ✅ |
| `agent-schema.json` | ✅ generated | ✅ | ⚠️ hand-maintained |

### Reproducing the comparison

```shell
gh api --paginate "repos/conductor-oss/python-sdk/pulls/441/files" \
  --jq '.[] | select(.status=="added") | .filename' | grep '^docs/'
```

### Why python-sdk's `docs/` looks larger

A directory listing shows **33** files in python-sdk's `docs/`, not 21. The extra 12 are
legacy SCREAMING_CASE documents that predate the restructure and that PR #441 did not
touch:

```
AUTHORIZATION.md  INTEGRATION.md  LEASE_EXTENSION.md  METADATA.md
PROMPT.md  SCHEDULE.md  SECRET_MANAGEMENT.md  TASK_MANAGEMENT.md
WORKER.md  WORKFLOW.md  WORKFLOW_TESTING.md  workflow-message-queue.md
```

They are not part of the target structure — their content is covered by the new topic docs
(`WORKER.md` → `workers.md`, `WORKFLOW.md` → `workflows.md`, `SCHEDULE.md` →
`schedules-events.md`, `SECRET_MANAGEMENT.md` → `security.md`). Literal filename parity
would mean importing files python-sdk has not finished retiring, so this SDK does not
mirror them.

## Content parity is not a goal

Structure is aligned; **content is not, and should not be**. Code samples are C#, package
names are NuGet ids, and four pages document functionality .NET does not have. Reading
"1:1" as identical prose would mean documenting a different SDK.

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

## READMEs

PR #441 did not only add files under `docs/` — it also gutted several READMEs, moving their
content into the new structure. The equivalents here:

| README | Python (#441) | .NET |
|---|---|---|
| Root `README.md` | +112 / −525 — rebuilt as a navigation hub | Rebuilt to the same section structure |
| `docs/README.md` | added | added |
| `docs/agents/README.md` | +33 / −26 | rewritten as the agent index |
| Examples index | `examples/agents/README.md`, +21 / −341 | `Conductor.AI.Examples/README.md`, added |

The root README follows python-sdk's section order — Choose your path · Choose your
Conductor server · Why Conductor? · Requirements and compatibility · Install the SDK · AI
agent quickstart · Workflow and worker quickstart · Common tasks · Troubleshooting ·
Support and project policies · License — so a reader moving between SDKs finds the same
shape.

The inline Hello World that previously lived in the root README now lives in
[core-quickstart.md](core-quickstart.md), which removes a duplicated copy that would have
drifted.

Python's per-framework example READMEs (`examples/agents/adk/`, `openai/`, `langgraph/`)
have no .NET counterpart, because this repo keeps all 175 agent examples in one flat
directory rather than per-framework subdirectories. The single
`Conductor.AI.Examples/README.md` indexes them by prefix instead.

## Known content gaps

Task-type builders the server supports but this SDK does not expose are listed in
[workflows.md](workflows.md#known-gaps) with issue links, rather than being silently
omitted from the task-type table.
