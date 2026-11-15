.RECIPEPREFIX = >
.PHONY: all test experiments trials trials-negative fmt-check docs tools tangle tangle-check
EMACS ?= emacs

all: test docs tangle-check

# code gate
test: experiments trials trials-negative fmt-check

experiments:
> @$(MAKE) -s -C experiments test

trials:
> @sea-trials/run.sh --lint
> @sea-trials/run.sh | tail -3

trials-negative:
> @tmp=$$(mktemp -d); cp -r sea-trials $$tmp/; printf 'wrong\n' > $$tmp/sea-trials/tests/250-sicp-1.2.5-gcd.out; \
  if $$tmp/sea-trials/run.sh gcd > /dev/null; then echo "FAIL runner accepted a wrong .out"; rm -rf $$tmp; exit 1; fi; \
  rm -rf $$tmp; echo "PASS runner rejects a wrong .out"

fmt-check:
> @python3 tools/forthfmt.py --check sea-trials/tests/*.fs 2>/dev/null && echo "PASS sea-trials formatted"

# docs gate
docs: tools/node_modules
> @python3 gates/docs_gate.py .
> @gates/test-docs-gate.sh

tools/node_modules: tools/package.json
> cd tools && npm install --silent

tools: tools/node_modules

tangle:
> @$(EMACS) --batch -Q --eval "(progn (require 'ob-tangle) (org-babel-tangle-file \"spec.org\"))" 2>&1 | grep Tangled

tangle-check:
> @gates/tangle-check.sh spec.org
