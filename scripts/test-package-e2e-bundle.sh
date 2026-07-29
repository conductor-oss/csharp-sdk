#!/usr/bin/env bash
set -euo pipefail

# ── Validator for package-e2e-bundle.sh ──────────────────────────────────────
# Builds the bundle at a throwaway version and asserts:
#   - tarball exists and extracts to the expected dir
#   - carries an executable, syntactically-valid run.sh + guard + README
#   - every e2e test source from the repo made it in (file-count parity),
#     plus the Settings.cs the suites `using Conductor.AI.Examples` for
#   - the SDK is pinned as a PackageReference at the version, with no
#     ProjectReference back at repo sources and no @VERSION@ left behind
#   - the assembly name stays Conductor.AI.E2eTests (InternalsVisibleTo target)
# All checks are static + deterministic (no network, no restore, no server).
#
# Run: ./scripts/test-package-e2e-bundle.sh [--build]
#   --build additionally packs the local SDK into a temp feed and compiles the
#   bundle against it — proves the suite builds with no ProjectReference, but
#   needs the .NET SDK and a NuGet cache (not run in the release workflow).

HERE="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(cd "$HERE/.." && pwd)"
VERSION="9.9.9-test"
DO_BUILD=0
[[ "${1:-}" == "--build" ]] && DO_BUILD=1

WORK="$(mktemp -d)"
trap 'rm -rf "$WORK"' EXIT

fail() { echo "FAIL: $*" >&2; exit 1; }
pass() { echo "  ok: $*"; }

"$HERE/package-e2e-bundle.sh" --version "$VERSION" --out "$WORK/dist" >/dev/null

NAME="conductor-ai-e2e-csharp-$VERSION"
TAR="$WORK/dist/$NAME.tar.gz"

[[ -f "$TAR" ]] || fail "tarball not produced ($TAR)"
pass "tarball produced"

mkdir -p "$WORK/x"
tar -xzf "$TAR" -C "$WORK/x"
ROOT="$WORK/x/$NAME"
[[ -d "$ROOT" ]] || fail "tarball does not extract to $NAME/"
pass "extracts to $NAME/"

[[ -f "$ROOT/run.sh" ]] || fail "missing run.sh"
[[ -x "$ROOT/run.sh" ]] || fail "run.sh not executable"
bash -n "$ROOT/run.sh"  || fail "run.sh has a bash syntax error"
[[ -f "$ROOT/README.md" ]] || fail "missing README.md"
python3 -c "import py_compile,sys; py_compile.compile(sys.argv[1], doraise=True)" \
  "$ROOT/check-results.py" >/dev/null \
  || fail "check-results.py has a syntax error"
grep -q "check-results.py" "$ROOT/run.sh" \
  || fail "run.sh does not invoke the vacuous-run guard"
# TRX drives the guard; junit-e2e.xml is what downstream CI publishes.
grep -q 'trx;LogFileName=agent-e2e.trx' "$ROOT/run.sh" \
  || fail "run.sh does not write results/agent-e2e.trx (the guard reads it)"
# Absolute path on purpose: the junit logger resolves LogFilePath against the
# project dir, ignoring --results-directory.
grep -q 'junit;LogFilePath=$HERE/results/junit-e2e.xml' "$ROOT/run.sh" \
  || fail "run.sh does not write results/junit-e2e.xml (downstream CI publishes it)"
pass "run.sh + guard + README present and valid"

# Every e2e test source made it into the bundle.
SRC_COUNT="$(find "$REPO_ROOT/Conductor.AI.E2eTests" -maxdepth 1 -type f -name '*.cs' | wc -l | tr -d ' ')"
BUNDLE_COUNT="$(find "$ROOT/Conductor.AI.E2eTests" -maxdepth 1 -type f -name '*.cs' | wc -l | tr -d ' ')"
[[ "$SRC_COUNT" == "$BUNDLE_COUNT" ]] \
  || fail "source parity: repo has $SRC_COUNT .cs files, bundle has $BUNDLE_COUNT"
[[ "$SRC_COUNT" -gt 0 ]] || fail "no e2e sources found in the repo — glob is wrong"
pass "all $SRC_COUNT e2e sources present"

# Settings.cs comes from the examples project via <Compile Include> in-repo; the
# bundle must carry its own copy or nothing compiles.
[[ -f "$ROOT/Conductor.AI.E2eTests/Shared/Settings.cs" ]] \
  || fail "missing Shared/Settings.cs (suites use Conductor.AI.Examples.Settings)"
grep -q "namespace Conductor.AI.Examples" "$ROOT/Conductor.AI.E2eTests/Shared/Settings.cs" \
  || fail "Shared/Settings.cs is not the Conductor.AI.Examples Settings"
pass "Settings.cs vendored with its namespace intact"

CSPROJ="$ROOT/Conductor.AI.E2eTests/Conductor.AI.E2eTests.csproj"
[[ -f "$CSPROJ" ]] || fail "missing Conductor.AI.E2eTests.csproj"

# SDK pinned as a package at the packaged version — never a path back to source.
python3 -c "
import sys
import xml.etree.ElementTree as ET
root = ET.parse(sys.argv[1]).getroot()
version = sys.argv[2]
pins = {r.get('Include'): r.get('Version') for r in root.iter('PackageReference')}
assert pins.get('conductor-ai') == version, f'conductor-ai pin is {pins.get(\"conductor-ai\")!r}, want {version!r}'
assert 'JunitXml.TestLogger' in pins, 'junit logger not referenced — run.sh cannot emit junit-e2e.xml'
refs = [r.get('Include') for r in root.iter('ProjectReference')]
assert not refs, f'bundle csproj still has ProjectReference(s): {refs}'
compiles = [c.get('Include') for c in root.iter('Compile')]
assert not compiles, f'bundle csproj reaches outside itself via <Compile>: {compiles}'
name = root.find('.//AssemblyName')
assert name is not None and name.text == 'Conductor.AI.E2eTests', \\
    'AssemblyName must stay Conductor.AI.E2eTests (Conductor.AI grants it InternalsVisibleTo)'
" "$CSPROJ" "$VERSION" || fail "csproj is not a valid standalone pin at $VERSION"
if grep -rn '@VERSION@' "$ROOT" >/dev/null 2>&1; then
  fail "unexpanded @VERSION@ placeholder left in bundle"
fi
pass "conductor-ai pinned at $VERSION as a package, assembly name preserved"

if [[ "$DO_BUILD" == 1 ]]; then
  # Pack the working tree at the throwaway version into a local feed, then build
  # the bundle against it — the compile-time proof that the suite needs nothing
  # but the published package.
  FEED="$WORK/feed"
  mkdir -p "$FEED"
  echo "  building: packing local SDK at $VERSION into a temp feed..."
  dotnet pack "$REPO_ROOT/Conductor/conductor-csharp.csproj" -o "$FEED" -c Release \
    "/p:Version=$VERSION" -v quiet --nologo >/dev/null || fail "dotnet pack conductor-csharp failed"
  dotnet pack "$REPO_ROOT/Conductor.AI/Conductor.AI.csproj" -o "$FEED" -c Release \
    "/p:Version=$VERSION" -v quiet --nologo >/dev/null || fail "dotnet pack conductor-ai failed"
  cat > "$ROOT/NuGet.config" <<XML
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="local-bundle-test" value="$FEED" />
  </packageSources>
</configuration>
XML
  dotnet build "$CSPROJ" -c Release --nologo \
    || fail "bundle does not compile against the packaged SDK at $VERSION"
  pass "bundle compiles against packaged conductor-ai $VERSION (no project references)"
fi

echo "ALL CHECKS PASSED"
