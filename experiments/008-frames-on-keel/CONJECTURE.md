# 008 frames-on-keel

## Hypothesis

- [H] The 16 layer-0 data words can be defined from the keel primitives `@ ! sp@ 0= + nand` with **no number literals at all**, which experiment 001 did not achieve: it borrowed gforth's number parser for the `1` in `negate`.
- [H] One frames source file then runs unchanged on two hosts — gforth and the Rust keel — and on each one every word matches that host's own reference over 9 values per argument.

## Motivation

R-F-01 and R-K-02. Experiment 001 proved the words derive from the primitives, but it proved it *on a host Forth*, with gforth supplying `:`, `;`, the number parser, and an emulated `nand`. Its ledger entry promised the same 432 checks against a real keel at M3, and the spec listed an open item for the emulated `nand`. A literal is the subtler of the two dependencies: the keel has no literal primitive, so a literal in frames is a host facility wearing a keel disguise. If the data words cannot be written without one, then either the primitive list is wrong or `frames` cannot be the first layer above the keel.

## Falsification criteria

- [H] Refuted if any of the 16 words produces a different result or stack from its host's reference on any of 9 values per argument (-1, 0, 1, 2, 7, 255, -256, max-n, min-n), or on the stack `10 20 30` for stack words — on either host.
- [H] Refuted if the frames source needs a number literal, a host word, or any primitive outside `@ ! sp@ 0= + nand`. The keel host refuses all three by construction, because the seed assembler has no token to resolve them to.
- [H] Refuted if the two hosts need different frames source. The same file is fed to both.
- [E] Not refuted by `drop`, `nip` and `2drop` requiring a cell beneath their argument: that is a property of the derivation on every host, and experiment 001 records it too.

## Prior art

- [P] sectorforth defines `dup` as `sp@ @`, `drop` as `dup - +`, and `swap` through `sp@` and `!`.
- [P] Minimal Forth (uho/minimal) uses wordlist isolation on a host Forth to establish which words are primitive.
- [E] Experiment 001 established the 16 definitions and the 432-check grid this experiment reuses.

## Evidence

- [E] gforth host: `432 checks, 0 failures`, and `one` derives 1 with no literal.
- [E] keel host: `432 checks, 0 failures` against the Rust kernel through its wasm shim, comparing exact stacks rather than experiment 001's fingerprint.
- [E] `nand` is a real primitive on the keel host, not `and invert`, which closes the spec open item that waited on M2.
- [E] Three negative controls run as part of `make test`, so the harness is known to fail: dropping one `cell +` from `swap` gives 61 failures; restoring experiment 001's literal `1` is refused; a definition using `*` is refused.

## Scope

In: the data-stack, arithmetic and logic words, on two hosts. Out: control flow, the return stack, the compiler and the outer interpreter — they need threaded-code layout the keel does not yet expose to frames, and they are what makes M3 complete. Out: promoting this source to `frames/`, which is M3's deliverable rather than its evidence.
