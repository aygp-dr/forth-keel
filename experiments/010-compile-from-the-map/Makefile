.RECIPEPREFIX = >
.PHONY: test host-keel negative
KEEL_CHECK = tests/keel-check.mjs
FRAMES = $(CURDIR)/src/frames.fs

test: host-keel negative

host-keel:
> @printf 'keel host:   '; node $(KEEL_CHECK) $(FRAMES)

# The harness must be known to fail. Each control must exit non-zero.
negative:
> @tmp=$$(mktemp -d); trap 'rm -rf $$tmp' EXIT; \
  sed 's/here @ ! here @ cell + here !/here @ !/' src/frames.fs > $$tmp/comma.fs; \
  sed 's/^: here ( -- addr ) zero ;/: here ( -- addr ) cell ;/' src/frames.fs > $$tmp/here.fs; \
  for f in comma here; do \
    if node $(KEEL_CHECK) $$tmp/$$f.fs > $$tmp/$$f.log 2>&1; then \
      echo "FAIL negative control $$f was accepted"; cat $$tmp/$$f.log; exit 1; \
    fi; \
  done; \
  echo "PASS 2 negative controls rejected (comma, here)"
