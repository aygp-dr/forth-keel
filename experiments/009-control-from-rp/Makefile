.RECIPEPREFIX = >
.PHONY: test host-keel negative
KEEL_CHECK = tests/keel-check.mjs
FRAMES = $(CURDIR)/src/frames.fs

test: host-keel negative

host-keel:
# Exit 77 means the wasm keel is not built: a skip, reported as one.
> @printf 'keel host:   '; node $(KEEL_CHECK) $(FRAMES); rc=$$?; [ $$rc -eq 77 ] && exit 0; exit $$rc

# The harness must be known to fail. Each control must exit non-zero.
# Without a keel host there is nothing to reject with, so the controls are
# skipped visibly; a skip read as "accepted" used to fail them instead.
negative:
> @node $(KEEL_CHECK) $(FRAMES) > /dev/null 2>&1; [ $$? -ne 77 ] || { echo "SKIP negative controls: no keel host"; exit 0; }; \
  tmp=$$(mktemp -d); trap 'rm -rf $$tmp' EXIT; \
  sed 's/rp@ @ dup cell + rp@ ! @/rp@ @ dup rp@ ! @/' src/frames.fs > $$tmp/lit.fs; \
  sed 's/@ @ cell - and/@ @ cell + and/' src/frames.fs > $$tmp/qbranch.fs; \
  sed 's/lit \[ &exit \]/lit [ 0 ]/' src/frames.fs > $$tmp/exitref.fs; \
  for f in lit qbranch exitref; do \
    if node $(KEEL_CHECK) $$tmp/$$f.fs > $$tmp/$$f.log 2>&1; then \
      echo "FAIL negative control $$f was accepted"; cat $$tmp/$$f.log; exit 1; \
    fi; \
  done; \
  echo "PASS 3 negative controls rejected (lit, ?branch, r> exit address)"
