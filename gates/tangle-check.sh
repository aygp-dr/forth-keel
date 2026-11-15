#!/usr/bin/env bash
# Tangle spec.org into an empty directory; every tangled file must match the working tree byte for byte.
set -euo pipefail
spec="${1:-spec.org}"
root="$(cd "$(dirname "$spec")" && pwd)"; base="$(basename "$spec")"
tmp="$(mktemp -d)"; trap 'rm -rf "$tmp"' EXIT
cp "$spec" "$tmp/"
( cd "$tmp" && emacs --batch -Q --eval "(progn (require 'ob-tangle) (org-babel-tangle-file \"$base\"))" >/dev/null 2>&1 )
n=0; status=0
while IFS= read -r f; do
  rel="${f#"$tmp"/}"; n=$((n+1))
  cmp -s "$f" "$root/$rel" || { echo "tangle-check: FAIL $rel differs or is missing"; status=1; }
done < <(find "$tmp" -type f ! -name "$base")
[ "$n" -gt 0 ] || { echo "tangle-check: FAIL no files tangled"; exit 1; }
[ "$status" -eq 0 ] && echo "tangle-check: PASS $n files"
exit "$status"
