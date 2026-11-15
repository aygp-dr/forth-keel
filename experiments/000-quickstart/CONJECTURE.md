# 000 quickstart

## Hypothesis

- [H] gforth 0.7.3 in this cell runs a Forth file non-interactively, and a byte comparison of its output against an expected file distinguishes a correct expectation from a wrong one.

## Motivation

Every later experiment and the sea-trials depend on running gforth from `make` and comparing output.

## Falsification criteria

- [H] Refuted if `gforth tests/hello.fs -e bye` exits non-zero or its output differs from `tests/hello.out`.
- [H] Refuted if `./check.sh tests/hello.fs tests/wrong.out` exits 0 (the check cannot fail).

## Prior art

- [P] gforth manual, "Invoking Gforth": `-e` evaluates its argument after loading the files.

## Scope

In: gforth invocation and output comparison. Out: any Forth layer code.
