# 011 elf-calculator: proof

- `elf.fs` holds 48 registers, a written flag per register, and the sorted
  targets with a covered flag each. `r+ r- r* r/ ( i j d -- )` read both
  sources, apply the operator, and write the destination, so the destination
  may be a source. Every write looks itself up among the targets by binary
  search.
- Rules are checked where they can fail. `(-)` needs `a > b`. `(/)` needs
  `a b mod` to be 0. `(*)` uses `um*` and fails if the high cell is non-zero,
  which catches a product that wraps 64 bits before it is compared with the
  limit — `mul-wrap` in `tests/rules.fs` is 2^32 · 2^32. `dst!` compares
  unsigned against `value_limit`. The first failure's code sticks; later
  instructions are skipped.
- `done` adds code 7 if any target was never covered and prints
  `RESULT id ok|fail code steps covered targets lower-bound`.
- `solve.fs` executes each instruction through `elf.fs` as it prints it, so
  it always knows what is held. It never writes registers holding starting
  values. Baseline: the cheaper of double-and-add from 1, or a held base plus
  or minus a difference built by double-and-add. Reuse adds a one-instruction
  check over every pair of held values, treats an already-held difference as
  free, and rotates destinations through the free registers so earlier
  targets stay held.
- The printed program is judged twice: by a fresh gforth run of `elf.fs`, and
  after `tools/fs2csv.py` turns it into the competition's CSV, by
  `tools/replay.py`, which was written from the Data page's rules and shares
  no code with `elf.fs`.

Run: `gmake test`. Expected: seven PASS lines ending in
`PASS 5 negative controls rejected (divide, limit, subtract, undefined, csv)`.
`gmake report` writes `out/results.json` for the dashboard. With
`data/puzzles.csv` downloaded, `gmake kaggle` writes `out/submission.csv`.
