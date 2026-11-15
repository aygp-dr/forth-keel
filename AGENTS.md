> Derived from spec.org @ v0.1. The spec governs; if this file disagrees with it, this file is the bug. Do not add facts here that are not in the spec — add them to the spec first.

# Working in forth-keel

- `spec.org` governs. Change it first, then run `make tangle`. Never edit
  a derived file by hand: README.md, AGENTS.md, CLAUDE.md,
  docs/architecture.md, docs/diagrams/*.mmd, resources/README.md,
  scripts/pull-resources.sh.
- Layers: keel (host language) → frames (Forth) → planking (Forth). A
  layer uses only the layer below it and its own earlier definitions.
- New behaviour starts as `experiments/NNN-name/` with CONJECTURE.md
  (falsification criteria, labelled claims) and a Makefile whose
  `make test` passes inside the directory. Never reuse or delete an
  experiment number.
- Oracle: `sea-trials/`. Expected output comes from gforth 0.7.3.
  Regenerate with `sea-trials/run.sh --regen` only for a new test, and
  check every value by hand before committing.
- Gates before claiming done: `make test`, `make docs`, `make tangle-check`.
  Zero findings; there is no warning tier.
- Commits: conventional, one logical step each, with a git note holding
  a Timeline (including failures) and a Reproduction (command, output,
  cell).
- Deferred or suspected-wrong work goes in an issue with the trigger
  that forces the revisit, not in a source comment.
- Declared deviations go in docs/review-ledger.org.
