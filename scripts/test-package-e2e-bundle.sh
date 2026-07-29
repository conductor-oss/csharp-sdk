#!/usr/bin/env bash
set -euo pipefail

# ── Validator for package-e2e-bundle.sh ──────────────────────────────────────
# Asserts that a bundle tarball:
#   - extracts to the expected dir
#   - carries an executable, syntactically-valid run.sh + guard + README
#   - carries EVERY e2e test source from the repo (recursive path-set parity),
#     plus the Settings.cs the suites `using Conductor.AI.Examples` for
#   - pins the SDK as a PackageReference at the version, with no ProjectReference
#     back at repo sources, no <Compile> escapes, no @VERSION@ left behind
#   - keeps AssemblyName Conductor.AI.E2eTests (InternalsVisibleTo target)
#   - has not drifted from the in-repo csproj (same TFM, same test package pins)
#   - ships the shared guard verbatim, keyed to the skip message E2eFixture.cs
#     actually emits
# All checks are static + deterministic (no network, no restore, no server).
#
# Run: ./scripts/test-package-e2e-bundle.sh [--tarball PATH] [--version V] [--build]
#
#   (no args)    package a throwaway bundle into a temp dir and check that.
#                Convenient for local dev; checks the SCRIPT, not any shipped
#                artifact.
#   --tarball    check an existing tarball. The release workflow uses this so the
#                artifact it uploads is the artifact it checked — packaging twice
#                would leave every version-dependent assertion untested on the
#                real bundle.
#   --version    version the tarball was stamped at; derived from its filename
#                when omitted.
#   --build      additionally pack the local SDK into a temp feed and compile the
#                bundle against it — the compile-time proof that it needs nothing
#                but the published package. Needs the .NET SDK; runs on PRs via
#                the CI Build workflow.

HERE="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(cd "$HERE/.." && pwd)"

TARBALL=""
VERSION=""
DO_BUILD=0

while [[ $# -gt 0 ]]; do
  case "$1" in
    --tarball) TARBALL="$2"; shift 2 ;;
    --version) VERSION="$2"; shift 2 ;;
    --build)   DO_BUILD=1; shift ;;
    *) echo "ERROR: unknown arg '$1' (want [--tarball PATH] [--version V] [--build])" >&2; exit 1 ;;
  esac
done

WORK="$(mktemp -d)"
trap 'rm -rf "$WORK"' EXIT

fail() { echo "FAIL: $*" >&2; exit 1; }
pass() { echo "  ok: $*"; }

if [[ -n "$TARBALL" ]]; then
  [[ -f "$TARBALL" ]] || fail "no such tarball: $TARBALL"
  TAR="$(cd "$(dirname "$TARBALL")" && pwd)/$(basename "$TARBALL")"
  BASE="$(basename "$TAR")"
  case "$BASE" in
    conductor-ai-e2e-csharp-*.tar.gz) ;;
    *) fail "unexpected tarball name '$BASE' (want conductor-ai-e2e-csharp-<version>.tar.gz)" ;;
  esac
  if [[ -z "$VERSION" ]]; then
    VERSION="${BASE#conductor-ai-e2e-csharp-}"
    VERSION="${VERSION%.tar.gz}"
    [[ -n "$VERSION" ]] || fail "cannot derive a version from '$BASE'"
  fi
  echo "Validating shipped tarball $BASE (version $VERSION)..."
else
  VERSION="${VERSION:-9.9.9-test}"
  "$HERE/package-e2e-bundle.sh" --version "$VERSION" --out "$WORK/dist" >/dev/null
  TAR="$WORK/dist/conductor-ai-e2e-csharp-$VERSION.tar.gz"
  echo "Validating a throwaway bundle packaged at version $VERSION..."
fi

NAME="conductor-ai-e2e-csharp-$VERSION"

[[ -f "$TAR" ]] || fail "tarball not produced ($TAR)"
pass "tarball present"

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
# The guard must be fatal, not best-effort: `dotnet test` exits 0 on an
# all-skipped run, so a warn-and-continue path reports a dead server as a pass.
grep -q 'ERROR: python3 not found' "$ROOT/run.sh" \
  || fail "run.sh does not hard-fail when python3 is missing — the guard would be skippable"
# TRX drives the guard; junit-e2e.xml is what downstream CI publishes.
grep -q 'trx;LogFileName=agent-e2e.trx' "$ROOT/run.sh" \
  || fail "run.sh does not write results/agent-e2e.trx (the guard reads it)"
# Absolute path on purpose: the junit logger resolves LogFilePath against the
# project dir, ignoring --results-directory.
grep -q 'junit;LogFilePath=$HERE/results/junit-e2e.xml' "$ROOT/run.sh" \
  || fail "run.sh does not write results/junit-e2e.xml (downstream CI publishes it)"
pass "run.sh + guard + README present and valid"

# ── The guard is shared, and keyed to a message the tests really emit ─────────
# check-results.py matches a substring of E2eFixture's skip message. If that
# message is reworded, the guard stops detecting dead-server runs and every lane
# goes green on a dead server — so assert the coupling instead of trusting it.
cmp -s "$HERE/check-results.py" "$ROOT/check-results.py" \
  || fail "bundle check-results.py differs from scripts/check-results.py — the guard has been forked"
MARKER="$(python3 - "$HERE/check-results.py" <<'PY'
import re
import sys
src = open(sys.argv[1], encoding="utf-8").read()
m = re.search(r'^UNREACHABLE_MARKER\s*=\s*"([^"]+)"', src, re.M)
if not m:
    sys.exit("UNREACHABLE_MARKER not found in check-results.py")
print(m.group(1))
PY
)" || fail "cannot read UNREACHABLE_MARKER out of scripts/check-results.py"
grep -qF "$MARKER" "$REPO_ROOT/Conductor.AI.E2eTests/E2eFixture.cs" \
  || fail "guard marker '$MARKER' does not appear in E2eFixture.cs — reword one to match the other, or a dead server reads as green"
grep -q 'scripts/check-results.py' "$REPO_ROOT/.github/workflows/agent-e2e.yml" \
  || fail "agent-e2e.yml does not call scripts/check-results.py — the guard has been duplicated again"
pass "guard shared verbatim, marker '$MARKER' matches E2eFixture.cs"

# ── Every e2e test source made it in ─────────────────────────────────────────
# Recursive path-set comparison, not a top-level count: a count over the same
# glob the packager uses would agree whether or not a subdirectory was dropped.
list_cs() { ( cd "$1" && find . -type f -name '*.cs' \
                 -not -path './obj/*' -not -path './bin/*' | LC_ALL=C sort ); }
list_cs "$REPO_ROOT/Conductor.AI.E2eTests" > "$WORK/repo-cs.txt"
list_cs "$ROOT/Conductor.AI.E2eTests"      > "$WORK/bundle-cs.txt"

COUNT="$(wc -l < "$WORK/repo-cs.txt" | tr -d ' ')"
[[ "$COUNT" -gt 0 ]] || fail "no e2e sources found in the repo — glob is wrong"
MISSING="$(comm -23 "$WORK/repo-cs.txt" "$WORK/bundle-cs.txt")"
[[ -z "$MISSING" ]] || fail "bundle is missing e2e sources:"$'\n'"$MISSING"
EXTRA="$(comm -13 "$WORK/repo-cs.txt" "$WORK/bundle-cs.txt")"
[[ "$EXTRA" == "./Shared/Settings.cs" ]] \
  || fail "bundle has unexpected sources (want only ./Shared/Settings.cs):"$'\n'"${EXTRA:-<none>}"
pass "all $COUNT e2e sources present (recursive parity), plus the vendored Settings.cs"

# Settings.cs comes from the examples project via <Compile Include> in-repo; the
# bundle must carry its own copy or nothing compiles.
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

# ── No drift from the in-repo csproj ─────────────────────────────────────────
# The bundle csproj is generated with its TFM and test-package versions written
# out longhand, so a bump in the in-repo csproj would otherwise leave the bundle
# quietly testing on stale xunit / Test.Sdk.
REPO_CSPROJ="$REPO_ROOT/Conductor.AI.E2eTests/Conductor.AI.E2eTests.csproj"
[[ -f "$REPO_CSPROJ" ]] || fail "in-repo csproj not found ($REPO_CSPROJ)"
python3 - "$CSPROJ" "$REPO_CSPROJ" <<'PY' || fail "bundle csproj has drifted from the in-repo csproj"
import sys
import xml.etree.ElementTree as ET


def load(path):
    root = ET.parse(path).getroot()
    pins = {r.get("Include"): r.get("Version") for r in root.iter("PackageReference")}
    tfm = root.find(".//TargetFramework")
    return (tfm.text if tfm is not None else None), pins


bundle_tfm, bundle_pins = load(sys.argv[1])
repo_tfm, repo_pins = load(sys.argv[2])

if bundle_tfm != repo_tfm:
    sys.exit(f"TargetFramework drift: bundle {bundle_tfm!r} vs repo {repo_tfm!r}")

drift = {
    name: (want, bundle_pins.get(name))
    for name, want in repo_pins.items()
    if bundle_pins.get(name) != want
}
if drift:
    detail = ", ".join(
        f"{name}: repo {want!r} vs bundle {got!r}" for name, (want, got) in sorted(drift.items())
    )
    sys.exit(
        "test package drift — update the generated csproj in "
        f"scripts/package-e2e-bundle.sh: {detail}"
    )

print(f"  ok: no drift from the in-repo csproj ({repo_tfm}, {len(repo_pins)} shared pins)")
PY

if [[ "$DO_BUILD" == 1 ]]; then
  # Pack the working tree at a throwaway version into a local feed, then build
  # the bundle against it — the compile-time proof that the suite needs nothing
  # but the published package. Uses its own version so it never collides with a
  # real one in the NuGet cache.
  BUILD_VERSION="9.9.9-test"
  FEED="$WORK/feed"
  mkdir -p "$FEED"
  echo "  building: packing local SDK at $BUILD_VERSION into a temp feed..."
  dotnet pack "$REPO_ROOT/Conductor/conductor-csharp.csproj" -o "$FEED" -c Release \
    "/p:Version=$BUILD_VERSION" -v quiet --nologo >/dev/null || fail "dotnet pack conductor-csharp failed"
  dotnet pack "$REPO_ROOT/Conductor.AI/Conductor.AI.csproj" -o "$FEED" -c Release \
    "/p:Version=$BUILD_VERSION" -v quiet --nologo >/dev/null || fail "dotnet pack conductor-ai failed"
  cat > "$ROOT/NuGet.config" <<XML
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="local-bundle-test" value="$FEED" />
  </packageSources>
</configuration>
XML
  # Repoint at the locally packed version: when validating a shipped tarball the
  # pin is a real release that may not be published yet, and the point of this
  # step is to compile the SOURCES against a package boundary.
  ( cd "$ROOT" && dotnet add Conductor.AI.E2eTests/Conductor.AI.E2eTests.csproj \
      package conductor-ai --version "$BUILD_VERSION" --no-restore >/dev/null ) \
    || fail "could not repoint the bundle at conductor-ai $BUILD_VERSION"
  dotnet build "$CSPROJ" -c Release --nologo \
    || fail "bundle does not compile against the packaged SDK at $BUILD_VERSION"
  pass "bundle compiles against packaged conductor-ai $BUILD_VERSION (no project references)"
fi

echo "ALL CHECKS PASSED"
