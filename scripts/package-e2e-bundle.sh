#!/usr/bin/env bash
set -euo pipefail

# ── Package the agent e2e suite as a standalone bundle ───────────────────────
# Builds conductor-ai-e2e-csharp-<version>.tar.gz: a self-contained .NET test
# project carrying the e2e test sources (Conductor.AI.E2eTests/), pinned to the
# published conductor-ai@<version> NuGet package (no SDK source vendored).
#
# Downstream repos (e.g. orkes-io/orkes-conductor) download the bundle from the
# csharp-sdk GitHub release and run it against their own server build, so the
# e2e suite is pinned to the exact SDK release under test.
# Mirrors conductor-oss/javascript-sdk#134 (scripts/package-e2e-bundle.sh).
#
# Usage:
#   ./scripts/package-e2e-bundle.sh --version 3.0.0-rc2 [--out DIR]
#
# Packaging is static (no build, no restore, no network) — the pinned version
# does not have to be on nuget.org yet, so this can run before the publish job
# finishes.

HERE="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(cd "$HERE/.." && pwd)"

VERSION=""
OUT_DIR="$HERE/e2e-bundle-dist"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --version) VERSION="$2"; shift 2 ;;
    --out)     OUT_DIR="$2"; shift 2 ;;
    *) echo "ERROR: unknown arg '$1' (want --version X.Y.Z [--out DIR])" >&2; exit 1 ;;
  esac
done

[[ -n "$VERSION" ]] || { echo "ERROR: --version is required" >&2; exit 1; }

NAME="conductor-ai-e2e-csharp-$VERSION"
STAGE="$OUT_DIR/$NAME"
PROJ="$STAGE/Conductor.AI.E2eTests"

echo "Packaging agent e2e bundle ($NAME)..."
rm -rf "$STAGE"
mkdir -p "$PROJ"

# Test sources copy over verbatim — they reference the SDK by namespace
# (Conductor.AI, Conductor.Client), which resolves identically from the NuGet
# package. The in-repo csproj is NOT copied; a standalone one is generated below.
find "$REPO_ROOT/Conductor.AI.E2eTests" -maxdepth 1 -type f -name '*.cs' \
  -exec cp {} "$PROJ/" \;

# Suites `using Conductor.AI.Examples` for Settings (LlmModel / ServerUrl). The
# in-repo csproj pulls that one file in with a <Compile Include> from the
# examples project; the bundle carries a copy instead so it stands alone.
mkdir -p "$PROJ/Shared"
cp "$REPO_ROOT/Conductor.AI.Examples/Shared/Settings.cs" "$PROJ/Shared/Settings.cs"

# Standalone project: same TFM and test packages as the in-repo csproj, but the
# ProjectReference is replaced by a PackageReference at the released version —
# the published artifact is what gets exercised. conductor-ai pulls
# conductor-csharp (and RestSharp, via RestSharp.Serializers.NewtonsoftJson)
# transitively, matching the in-repo dependency graph.
#
# AssemblyName must stay Conductor.AI.E2eTests: Conductor.AI grants it
# InternalsVisibleTo, and a few suites read internal types.
cat > "$PROJ/Conductor.AI.E2eTests.csproj" <<'EOF'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
    <AssemblyName>Conductor.AI.E2eTests</AssemblyName>
    <RootNamespace>Conductor.AI.E2eTests</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Xunit.SkippableFact" Version="1.4.13" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <!-- Bundle-only: TRX is what the in-repo workflow consumes, but downstream
         CI (orkes-conductor et al.) publishes JUnit XML, which every other
         conductor-ai e2e bundle emits as results/junit-e2e.xml. -->
    <PackageReference Include="JunitXml.TestLogger" Version="6.1.0" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="conductor-ai" Version="@VERSION@" />
  </ItemGroup>
</Project>
EOF

# Guard: every e2e test is a [SkippableFact] that skips when the server is
# unreachable, so a dead server yields an all-skipped run that reads as green.
# Same check the SDK's own agent-e2e workflow applies to its TRX output.
cat > "$STAGE/check-results.py" <<'EOF'
#!/usr/bin/env python3
"""Fail a vacuous agent-e2e run: 0 tests executed, or skips from an unreachable server."""
import glob
import sys
import xml.etree.ElementTree as ET

NS = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
paths = sorted(glob.glob(sys.argv[1] if len(sys.argv) > 1 else "results/*.trx"))
if not paths:
    print("GUARD: no TRX files found — vacuous run")
    sys.exit(1)

executed = unreachable = 0
for p in paths:
    root = ET.parse(p).getroot()
    counters = root.find(".//t:ResultSummary/t:Counters", NS)
    if counters is not None:
        executed += int(counters.get("executed", "0"))
    for r in root.findall(".//t:UnitTestResult", NS):
        if r.get("outcome") == "NotExecuted":
            msg = r.find(".//t:Message", NS)
            if msg is not None and "server is not reachable" in (msg.text or ""):
                unreachable += 1

print(f"GUARD: executed={executed}, server-unreachable skips={unreachable}")
if executed == 0:
    print("GUARD: 0 tests executed — vacuous run")
    sys.exit(1)
if unreachable > 0:
    print("GUARD: suite skipped due to unreachable server — vacuous run")
    sys.exit(1)
print("GUARD: OK")
EOF

cat > "$STAGE/run.sh" <<'EOF'
#!/usr/bin/env bash
set -euo pipefail
# Runs the agent e2e suite against a live Conductor server with the agent
# runtime enabled (conductor-oss >= 3.32.0-rc.8, or orkes-conductor with the
# agent runtime embedded).
#
# Required services (NOT started by this script):
#   - Conductor server → CONDUCTOR_SERVER_URL (default http://localhost:8080/api)
#   - mcp-testkit on http://localhost:3001 (MCP + HTTP-tool suites; the URL is
#     fixed in those suites, not configurable)
# Optional:
#   - CONDUCTOR_AGENT_LLM_MODEL (default openai/gpt-4o-mini); the provider API
#     key must be configured on the SERVER. A few suites additionally gate on
#     OPENAI_API_KEY being present in this environment and skip without it.
#
# Requires the .NET 8 SDK. Usage: ./run.sh [extra dotnet test args]
HERE="$(cd "$(dirname "$0")" && pwd)"
cd "$HERE"

rc=0
dotnet test Conductor.AI.E2eTests/Conductor.AI.E2eTests.csproj \
  --configuration Release \
  --logger "console;verbosity=normal" \
  --logger "trx;LogFileName=agent-e2e.trx" \
  --logger "junit;LogFilePath=$HERE/results/junit-e2e.xml" \
  --results-directory results \
  "$@" || rc=$?

# An all-skipped run (dead server) must not read as a pass.
if command -v python3 >/dev/null 2>&1; then
  python3 check-results.py 'results/*.trx' || rc=1
else
  echo "WARN: python3 not found — skipping the vacuous-run guard" >&2
fi

echo "Results: $HERE/results/agent-e2e.trx"
exit "$rc"
EOF
chmod +x "$STAGE/run.sh"

cat > "$STAGE/README.md" <<'EOF'
# Conductor Agent SDK (.NET) — E2E suite @VERSION@

Self-contained end-to-end tests for the Conductor .NET agent SDK, pinned to
release **@VERSION@**. Restores the `conductor-ai` NuGet package at that exact
version — no SDK source is vendored, so a run exercises the published package.
Cut from
[conductor-sdk/conductor-csharp](https://github.com/conductor-sdk/conductor-csharp)
(`Conductor.AI.E2eTests/`).

## Prerequisites (you provide these)

| Requirement                       | Env var                     | Default                     |
|-----------------------------------|-----------------------------|-----------------------------|
| .NET 8 SDK                        | —                           | —                           |
| Conductor server w/ agent runtime | `CONDUCTOR_SERVER_URL`      | `http://localhost:8080/api` |
| LLM model                         | `CONDUCTOR_AGENT_LLM_MODEL` | `openai/gpt-4o-mini`        |
| mcp-testkit (MCP + HTTP suites)   | — (fixed `localhost:3001`)  | `pip install mcp-testkit`   |

The server needs the agent runtime: conductor-oss `>= 3.32.0-rc.8`, or
orkes-conductor booted with the agent runtime embedded. LLM provider API keys
(e.g. `OPENAI_API_KEY`) go to the **server** process; a few suites also gate on
`OPENAI_API_KEY` in their own environment and skip without it.

Every test skips rather than fails when the server is unreachable, so `run.sh`
ends with a guard that fails an all-skipped ("vacuous") run.

## Run

```bash
./run.sh                                        # full suite
./run.sh --filter 'FullyQualifiedName~Suite1'   # filter, plus any dotnet test args
```

Results land in `results/` — `agent-e2e.trx` plus `junit-e2e.xml` for CI report
publishers.

## Testing an unreleased SDK

```bash
dotnet add Conductor.AI.E2eTests/Conductor.AI.E2eTests.csproj \
  package conductor-ai --version <other-version>
./run.sh
```

For a locally built package, drop the `.nupkg` in a directory and add it as a
source: `dotnet nuget add source <dir> -n local`.
EOF

# Stamp the version everywhere (skip binary fixtures).
find "$STAGE" -type f ! -name '*.png' ! -name '*.jpg' ! -name '*.jpeg' \
    ! -name '*.gif' ! -name '*.webp' ! -name '*.pdf' -print0 \
  | xargs -0 sed -i.bak "s/@VERSION@/$VERSION/g"
find "$STAGE" -name '*.bak' -delete

mkdir -p "$OUT_DIR"
tar -czf "$OUT_DIR/$NAME.tar.gz" -C "$OUT_DIR" "$NAME"
rm -rf "$STAGE"

echo "OK: $OUT_DIR/$NAME.tar.gz"
