#!/usr/bin/env bash
# sea-trials runner: run every test, compare its output with tests/NNN-name.out.
#
#   ./run.sh                          run every test with gforth
#   ./run.sh --impl 'node keel.mjs {}' run with another host ({} = test file path)
#   ./run.sh --layer 0                only tests whose "\ layer:" line is 0 (repeatable: --layer 0 --layer 1)
#   ./run.sh --supports core,core-ext skip tests whose "\ requires:" line needs other wordsets
#   ./run.sh --loose                  compare after collapsing runs of spaces and trimming line ends
#   ./run.sh --lint                   only check that every test has "\ requires:" and "\ layer:" lines
#   ./run.sh --regen                  rewrite tests/*.out from the current --impl (default gforth)
#   ./run.sh sicp                     only tests whose file name contains "sicp"
#
# Exit status: 0 if every selected, non-skipped test passed (or --lint found nothing), 1 otherwise.
set -u
cd "$(dirname "$0")"
impl='gforth {} -e bye'
supports='' layers='' loose=0 regen=0 lint=0 filter=''
while [ $# -gt 0 ]; do
  case "$1" in
    --impl) impl="$2"; shift 2 ;;
    --supports) supports=",$2,"; shift 2 ;;
    --layer) layers="$layers,$2,"; shift 2 ;;
    --loose) loose=1; shift ;;
    --regen) regen=1; shift ;;
    --lint) lint=1; shift ;;
    *) filter="$1"; shift ;;
  esac
done
norm() { if [ "$loose" = 1 ]; then sed -e 's/  */ /g' -e 's/ *$//'; else cat; fi; }
header() { sed -n "${2}s/^\\\\ $1: *//p" "$3"; }

if [ "$lint" = 1 ]; then
  bad=0
  for t in tests/*.fs; do
    [ -n "$(header requires 1 "$t")" ] || { echo "lint: $t line 1 is not '\\ requires: ...'"; bad=1; }
    case "$(header layer 2 "$t")" in 0|1|x) ;; *) echo "lint: $t line 2 is not '\\ layer: 0|1|x'"; bad=1 ;; esac
    [ -f "${t%.fs}.out" ] || { echo "lint: ${t%.fs}.out missing"; bad=1; }
  done
  [ "$bad" = 0 ] && echo "lint: PASS $(ls tests/*.fs | wc -l) tests"
  exit "$bad"
fi

declare -A lp lf
pass=0 fail=0 skip=0
tmp=$(mktemp); trap 'rm -f "$tmp"' EXIT
for t in tests/*.fs; do
  case "$t" in *"$filter"*) ;; *) continue ;; esac
  name=$(basename "$t" .fs)
  layer=$(header layer 2 "$t")
  if [ -n "$layers" ]; then case "$layers" in *",$layer,"*) ;; *) continue ;; esac; fi
  if [ -n "$supports" ]; then
    missing=''
    for r in $(header requires 1 "$t"); do case "$supports" in *",$r,"*) ;; *) missing="$missing $r" ;; esac; done
    if [ -n "$missing" ]; then echo "SKIP $name (needs$missing)"; skip=$((skip+1)); continue; fi
  fi
  cmd=${impl//\{\}/$t}
  actual=$(timeout 20 bash -c "$cmd" 2>&1 < /dev/null)
  if [ "$regen" = 1 ]; then printf '%s\n' "$actual" > "tests/$name.out"; echo "REGEN $name"; continue; fi
  if diff -u <(norm < "tests/$name.out") <(printf '%s\n' "$actual" | norm) > "$tmp"; then
    echo "PASS $name"; pass=$((pass+1)); lp[$layer]=$(( ${lp[$layer]:-0} + 1 ))
  else
    echo "FAIL $name"; sed 's/^/    /' "$tmp" | head -20; fail=$((fail+1)); lf[$layer]=$(( ${lf[$layer]:-0} + 1 ))
  fi
done
[ "$regen" = 1 ] && exit 0
for l in 0 1 x; do
  p=${lp[$l]:-0} f=${lf[$l]:-0}
  [ $((p+f)) -gt 0 ] && echo "layer $l: $p/$((p+f))"
done
echo "pass $pass  fail $fail  skip $skip"
[ "$fail" = 0 ]
