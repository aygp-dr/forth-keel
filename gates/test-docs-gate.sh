#!/usr/bin/env bash
# Negative test of the docs gate: _base passes; each fixture fails with exactly its own check name.
set -u
cd "$(dirname "$0")/.."
gate="python3 gates/docs_gate.py"
status=0
if $gate gates/fixtures/docs/_base > /dev/null; then echo "PASS fixture _base passes"; else echo "FAIL fixture _base does not pass"; status=1; fi
for d in gates/fixtures/docs/*/; do
  check=$(basename "$d"); [ "$check" = _base ] && continue
  out=$($gate "$d"); rc=$?
  names=$(printf '%s\n' "$out" | sed -n 's/^docs-gate: FAIL \([a-z-]*\):.*/\1/p' | sort -u | tr '\n' ' ')
  if [ "$rc" -ne 0 ] && [ "$names" = "$check " ]; then echo "PASS fixture $check fails with $check"
  else echo "FAIL fixture $check: exit $rc, checks reported: ${names:-none}"; status=1; fi
done
exit "$status"
