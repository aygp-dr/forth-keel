#!/usr/bin/env python3
"""Rebuild gates/fixtures/docs/: a passing _base tree and one tree per docs-gate check
that violates only that check. Run from the repository root."""
import os
import shutil

SPEC = """#+TITLE: fixture
* Scope
* Working rules
* Survey and names
* Architecture
#+begin_src mermaid
flowchart LR
  a --> b
#+end_src
* Build sequence
* Requirements
| ID     | Statement |
|--------+-----------|
| R-X-01 | example   |
* Experiments
* Resources
* Gates
* Debug toolchain
* Open items
* Review ledger
* Derived files
#+begin_src markdown :tangle README.md
> Derived from spec.org @ fixture.

See [spec](spec.org).
#+end_src
"""
README = "> Derived from spec.org @ fixture.\n\nSee [spec](spec.org).\n"
CONJ = """# 001 example

## Hypothesis

- [H] Example claim for R-X-01.

## Falsification criteria

- [H] Refuted if the example fails.
"""
BASE = {"spec.org": SPEC, "README.md": README, "experiments/001-example/CONJECTURE.md": CONJ}
MUTATIONS = {
    "spec-sections": ("spec.org", "* Open items\n", ""),
    "links": ("README.md", "[spec](spec.org)", "[spec](missing.org)"),
    "falsification": ("experiments/001-example/CONJECTURE.md", "## Falsification criteria\n\n- [H] Refuted if the example fails.\n", ""),
    "labels": ("experiments/001-example/CONJECTURE.md", "- [H] Example claim", "- Example claim"),
    "derived-header": ("README.md", "> Derived from spec.org @ fixture.\n\n", ""),
    "req-refs": ("experiments/001-example/CONJECTURE.md", "R-X-01", "R-X-99"),
    "mermaid": ("spec.org", "  a --> b\n", "  a --> \n  b[[[c\n"),
}


def write(root, files):
    for rel, text in files.items():
        p = os.path.join(root, rel)
        os.makedirs(os.path.dirname(p), exist_ok=True)
        with open(p, "w") as fh:
            fh.write(text)


out = "gates/fixtures/docs"
shutil.rmtree(out, ignore_errors=True)
write(os.path.join(out, "_base"), BASE)
for check, (rel, old, new) in MUTATIONS.items():
    files = dict(BASE)
    assert old in files[rel], (check, old)
    files[rel] = files[rel].replace(old, new)
    write(os.path.join(out, check), files)
print(f"fixtures: _base + {len(MUTATIONS)} checks in {out}")
