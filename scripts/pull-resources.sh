#!/usr/bin/env bash
# Derived from spec.org @ v0.1. The spec governs; if this file disagrees with it, this file is the bug.
# Pull reference sources into resources/vendor/. Idempotent: existing clones are re-pinned, pages re-fetched.
set -euo pipefail
cd "$(dirname "$0")/.."
dest=resources/vendor
mkdir -p "$dest"
pin() {  # key url commit
  if [ ! -d "$dest/$1/.git" ]; then git clone -q "$2" "$dest/$1"; fi
  git -C "$dest/$1" fetch -q origin "$3" 2>/dev/null || git -C "$dest/$1" fetch -q origin
  git -C "$dest/$1" -c advice.detachedHead=false checkout -q "$3"
  echo "git  $1 @ $(git -C "$dest/$1" rev-parse --short HEAD)"
}
page() {  # key url
  if curl -fsSL --max-time 60 -o "$dest/$1.html" "$2"; then echo "web  $1 ($(wc -c < "$dest/$1.html") bytes)"
  else rm -f "$dest/$1.html"; echo "web  $1 FAILED: $2" >&2; return 1; fi
}
status=0
pin forth2012-tests https://github.com/gerryjackson/forth2012-test-suite 4c87e5659c5b79bd7a6664daa56603df080c6f9b || status=1
pin sectorforth     https://github.com/cesarblum/sectorforth           32031ac6e77e30817c2f65ba11b1ccda07d564f9 || status=1
pin jonesforth      https://github.com/nornagon/jonesforth             d97a25bb0b06fb58da1975eac291f45618fd2ada || status=1
pin eforth          https://github.com/chochain/eforth                 b3a33b151ef6922bc7534ea5632eff3ceb016692 || status=1
pin minimal-forth   https://github.com/uho/minimal                     0db6014472d7450e3750fabefaa116156ac6f708 || status=1
pin waforth         https://github.com/remko/waforth                   280073c7722018f42a4e3fa37f4e123f7a236d94 || status=1
pin forsm           https://github.com/SupernaviX/forsm                712aad95b5c14554389d45e19e983da0d276632e || status=1
page forth-2012-testsuite-annex https://forth-standard.org/standard/testsuite || status=1
page dpans-core     http://lars.nocrew.org/dpans/dpans6.htm || status=1
page sicp           https://mitp-content-server.mit.edu/books/content/sectbyfn/books_pres_0/6515/sicp.zip/index.html || status=1
page cheney-1970    https://doi.org/10.1145/362790.362798 || status=1
exit "$status"
