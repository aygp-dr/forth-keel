# 002 tagged-cells: proof

- `src/cells.fs`: `>fix` is `2*`, `fix>` is `2/` (arithmetic shift), `fix?` tests low bit 0, `obj?` low bit 1. `cons` allocates 3 cells (header `t-pair`, car, cdr) from a 4096-cell heap and returns address+1. `null` is fixnum 0.
- `tests/test.fs`: 78 checks: 8 round trips, 24 predicate checks, 40 pair checks, 3 nested-pair checks, 3 list checks.

Run: `make test`. Expected: `78 checks, 0 failures`.

Assumption: `create` returns a cell-aligned address, so address+1 always has low bit 1. The pair checks observe this for every allocated pair.
