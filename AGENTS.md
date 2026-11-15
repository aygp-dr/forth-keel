> Derived from spec.org @ v0.1. The spec governs; if this file disagrees with it, this file is the bug. Do not add facts here that are not in the spec — add them to the spec first.

# Working in forth-keel

- `spec.org` governs. Change it first, then run `gmake tangle`. Never edit
  a derived file by hand.
- A layer uses only the layer below it and its own earlier definitions:
  keel → frames → planking.
- New behaviour starts as `experiments/NNN-name/` with a CONJECTURE.md
  (falsification criteria, labelled claims) and a Makefile whose
  `make test` passes inside the directory. Never reuse or delete a number.
- `sea-trials/` is the oracle. Regenerate an `.out` only for a new or
  changed test, and check every value by hand before committing.
- Before claiming done: `gmake test`, `gmake docs`, `gmake tangle-check`.
  Zero findings; there is no warning tier.
- Commits are conventional, one logical step each, with a git note that
  has a Timeline (including failures) and a Reproduction (command,
  output, cell).
- Deferred or suspected-wrong work goes in an issue that names the
  trigger forcing the revisit, not in a source comment.
- A deviation from these rules is allowed only when declared in the
  commit message and in docs/review-ledger.org.
