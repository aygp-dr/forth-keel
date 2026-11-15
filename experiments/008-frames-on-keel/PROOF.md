# 008 frames-on-keel: proof

- `src/frames.fs` holds the 16 definitions and nothing host-specific: no literals, no `include`, only `:` `;` `(` `\` from its host.
- `one ( -- 1 )` is the key derivation. `sp@` manufactures a value; `dup invert and` is 0 whatever that value was; `0=` makes -1; `dup +` makes -2; `invert` makes 1. `negate` is then `invert one +`, which is two's complement stated directly.
- Host 1, gforth: the Makefile concatenates `src/gforth-prelude.fs`, `src/frames.fs` and `src/gforth-end.fs`, so frames never needs `include` and the search order holds only `keel-wl frames-wl` throughout. `keel-wl` exposes exactly `@ ! sp@ 0= +` plus an emulated `nand`. `tests/gforth-check.fs` compares each word with gforth's own.
- Host 2, the keel: `tests/keel-check.mjs` seeds the same file into the real kernel. A frames word becomes a thread of execution tokens whose body address is its own token; `nand` is a real primitive. Comparison is against 32-bit reference arithmetic, on exact stacks rather than a fingerprint.
- The seed assembler is the bootstrap, and it is deliberately not a Forth interpreter: it parses no text at run time and can push no literal. The outer interpreter that replaces it is itself a frames word, which is M4.

Run: `make test`. Expected on each host: `432 checks, 0 failures` (2 unary x 9, 5 binary x 81, 8 stack words, 1 cell check), then `PASS one derives 1 without a literal`, then three negative controls that must fail.

The keel host skips with a message naming the `wasm-pack` command when no wasm build is present, so `make test` passes on a machine that has never built the kernel. When it skips, only host 1 has run.

Assumption, observed on both hosts by the `swap` checks: `sp@` returns the address of the top stack item, the stack grows down, and storing with `!` below the top item changes the stack.
