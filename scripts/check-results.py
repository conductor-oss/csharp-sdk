#!/usr/bin/env python3
"""Fail a vacuous agent-e2e run: 0 tests executed, or skips from an unreachable server.

Every e2e test is a [SkippableFact] that skips when the server is unreachable, so
a dead server produces an all-skipped run that `dotnet test` exits 0 on. This
guard turns that into a failure.

Single source of truth, shared by both lanes so they cannot drift:
  - .github/workflows/agent-e2e.yml runs it against the in-repo suite
  - scripts/package-e2e-bundle.sh copies it into the released bundle, where
    run.sh runs it

Usage: check-results.py ['results/*.trx']
"""

import glob
import sys
import xml.etree.ElementTree as ET

NS = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}

# Substring of the skip message E2eFixture emits when the server is unreachable.
# This is a literal coupling to test source: reword it in
# Conductor.AI.E2eTests/E2eFixture.cs and this guard silently stops detecting
# dead-server runs. test-package-e2e-bundle.sh asserts the two still match.
UNREACHABLE_MARKER = "server is not reachable"

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
            if msg is not None and UNREACHABLE_MARKER in (msg.text or ""):
                unreachable += 1

print(f"GUARD: executed={executed}, server-unreachable skips={unreachable}")
if executed == 0:
    print("GUARD: 0 tests executed — vacuous run")
    sys.exit(1)
if unreachable > 0:
    print("GUARD: suite skipped due to unreachable server — vacuous run")
    sys.exit(1)
print("GUARD: OK")
