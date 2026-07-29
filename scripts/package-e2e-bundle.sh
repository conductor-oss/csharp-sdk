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

# The version is interpolated into a filename and into the sed replacement that
# stamps @VERSION@, so restrict it to characters that are inert in both. Without
# this a version carrying '/' or '&' silently corrupts every stamped file.
[[ "$VERSION" =~ ^[A-Za-z0-9][A-Za-z0-9.+-]*$ ]] || {
  echo "ERROR: --version '$VERSION' is not a plain version string" >&2
  echo "       (allowed: alphanumerics, dot, plus, hyphen — e.g. 3.0.0-rc2)" >&2
  exit 1
}

NAME="conductor-ai-e2e-csharp-$VERSION"
STAGE="$OUT_DIR/$NAME"
PROJ="$STAGE/Conductor.AI.E2eTests"

echo "Packaging agent e2e bundle ($NAME)..."
rm -rf "$STAGE"
mkdir -p "$PROJ"

# Test sources copy over verbatim — they reference the SDK by namespace
# (Conductor.AI, Conductor.Client), which resolves identically from the NuGet
# package. The in-repo csproj is NOT copied; a standalone one is generated below.
#
# Recursive, preserving layout: a new subdirectory of test sources must not be
# silently dropped from the bundle. test-package-e2e-bundle.sh compares the full
# relative path sets, so it catches an omission here rather than sharing the
# same blind spot.
SRC_DIR="$REPO_ROOT/Conductor.AI.E2eTests"
while IFS= read -r -d '' f; do
  rel="${f#"$SRC_DIR/"}"
  mkdir -p "$PROJ/$(dirname "$rel")"
  cp "$f" "$PROJ/$rel"
done < <(find "$SRC_DIR" -type f -name '*.cs' \
           -not -path "$SRC_DIR/obj/*" -not -path "$SRC_DIR/bin/*" -print0)

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
# Copied verbatim rather than inlined here, so the bundle and the SDK's own
# agent-e2e workflow run the identical script — including the skip-message
# marker, which would otherwise be duplicated in two places and drift.
cp "$HERE/check-results.py" "$STAGE/check-results.py"

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
# Requires the .NET 8 SDK and python3 (for the vacuous-run guard).
# Usage: ./run.sh [extra dotnet test args]
HERE="$(cd "$(dirname "$0")" && pwd)"
cd "$HERE"

# Checked up front, and fatal: an all-skipped run (dead server) exits 0 from
# `dotnet test`, so a missing guard would turn that into a silent pass — the
# exact failure this bundle is supposed to make impossible. Better to refuse to
# start than to run for 20 minutes and report an unverifiable green.
if ! command -v python3 >/dev/null 2>&1; then
  echo "ERROR: python3 not found, but it is required for the vacuous-run guard." >&2
  echo "       Install python3 (>= 3.8) and re-run." >&2
  exit 1
fi

rc=0
dotnet test Conductor.AI.E2eTests/Conductor.AI.E2eTests.csproj \
  --configuration Release \
  --logger "console;verbosity=normal" \
  --logger "trx;LogFileName=agent-e2e.trx" \
  --logger "junit;LogFilePath=$HERE/results/junit-e2e.xml" \
  --results-directory results \
  "$@" || rc=$?

python3 check-results.py 'results/*.trx' || rc=1

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
| python3 >= 3.8 (run guard)        | —                           | —                           |
| Conductor server w/ agent runtime | `CONDUCTOR_SERVER_URL`      | `http://localhost:8080/api` |
| LLM model                         | `CONDUCTOR_AGENT_LLM_MODEL` | `openai/gpt-4o-mini`        |
| mcp-testkit (MCP + HTTP suites)   | — (fixed `localhost:3001`)  | `pip install mcp-testkit`   |

The server needs the agent runtime: conductor-oss `>= 3.32.0-rc.8`, or
orkes-conductor booted with the agent runtime embedded. LLM provider API keys
(e.g. `OPENAI_API_KEY`) go to the **server** process; a few suites also gate on
`OPENAI_API_KEY` in their own environment and skip without it.

Every test skips rather than fails when the server is unreachable, so `run.sh`
ends with a guard (`check-results.py`) that fails an all-skipped ("vacuous")
run. The guard is not optional — `run.sh` refuses to start without python3,
because a missing guard would report a dead server as a pass.

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
