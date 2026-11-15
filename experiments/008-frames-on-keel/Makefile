.RECIPEPREFIX = >
.PHONY: test host-gforth host-keel negative
KEEL_CHECK = tests/keel-check.mjs
FRAMES = $(CURDIR)/src/frames.fs

test: host-gforth host-keel negative

# Host 1: gforth, with only the keel primitives in the search order.
host-gforth:
> @tmp=$$(mktemp -d); trap 'rm -rf $$tmp' EXIT; \
  cat src/gforth-prelude.fs src/frames.fs src/gforth-end.fs > $$tmp/host.fs; \
  printf 'gforth host: '; gforth $$tmp/host.fs tests/gforth-check.fs

# Host 2: the Rust keel through its wasm shim. Skips if it has not been built.
host-keel:
> @printf 'keel host:   '; node $(KEEL_CHECK) $(FRAMES)

# The harness must be known to fail. Each control must exit non-zero.
negative:
> @tmp=$$(mktemp -d); trap 'rm -rf $$tmp' EXIT; \
  sed 's/ cell + cell + + !/ cell + + !/' src/frames.fs > $$tmp/broken-swap.fs; \
  sed 's/invert one +/invert 1 +/' src/frames.fs > $$tmp/literal.fs; \
  sed 's/^: dup .*/&\n: bad ( a b -- c ) * ;/' src/frames.fs > $$tmp/star.fs; \
  for f in broken-swap literal star; do \
    if node $(KEEL_CHECK) $$tmp/$$f.fs > $$tmp/$$f.log 2>&1; then \
      echo "FAIL negative control $$f was accepted"; cat $$tmp/$$f.log; exit 1; \
    fi; \
  done; \
  echo "PASS 3 negative controls rejected (broken swap, literal, word outside the keel)"
