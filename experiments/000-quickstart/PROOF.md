# 000 quickstart: proof

- `check.sh FILE EXPECTED` runs `gforth FILE -e bye` and compares stdout+stderr with EXPECTED byte for byte.
- `make test` runs the check on `tests/hello.fs` against `tests/hello.out` (must pass) and against `tests/wrong.out` (must fail).

Run: `make test`. Assumes `gforth` on PATH.
