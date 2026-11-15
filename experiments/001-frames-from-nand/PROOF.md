# 001 frames-from-nand: proof

- `src/frames.fs` creates `keel-wl` with aliases for `@ ! sp@ rp@ 0= +`, an emulated `nand` (`and invert`), and the host's `: ; ( \`. It then compiles 16 words into `frames-wl` with only `frames-wl keel-wl` in the search order.
- `cell` is computed as `sp@ sp@ -`, so no cell-size constant is needed.
- `tests/test.fs` compares each frames word with gforth's word over the value grid (unary: 9 cases, binary: 81 cases, stack words: one fingerprint of the whole stack).
- `tests/isolation.fs` must fail to compile `: uses-star * ;` inside the frames search order.

Run: `make test`. Expected: `432 checks, 0 failures` (2 unary words x 9, 5 binary words x 81, 8 stack words, 1 cell check) and `PASS isolation`.

Assumption: gforth's `sp@` returns the address of the top stack item, and writing with `!` below the top item changes the stack. Both are observed by the `swap` checks.
