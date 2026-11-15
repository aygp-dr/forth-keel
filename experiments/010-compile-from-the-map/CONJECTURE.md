# 010 compile-from-the-map

## Hypothesis

- [H] `here` and `,` can be defined in frames with no new keel primitive, because the memory map fixes the system variables at known addresses and frames can derive an address the same way it derives any other number.
- [H] Frames can then compile a new word at run time and execute it, which is the last mechanism the compiler needs before it needs a dictionary of names.

## Motivation

R-F-02 and R-K-04. Experiment 009 derived the control-flow words a thread *executes*, but the words that *compile* control flow — `if`, `then`, `begin`, `until` — are immediate words over `here` and `,`. The keel has neither, and R-K-01 forbids adding them: it is thirteen primitives and nothing else. sectorforth reaches `here` by exposing it as a sixth variable word beyond its eight primitives, so the obvious reading is that forth-keel's list is one short.

It is not, and the reason is R-K-04. The memory map is not decoration: because it fixes HERE first among the system variables, the address of HERE is a constant frames can derive, and `,` follows from `@` and `!`. The map is load-bearing, and this experiment is what makes it so.

## Falsification criteria

- [H] Refuted if `here` or `,` needs a primitive outside `@ ! sp@ rp@ 0= + nand exit`, or a number literal.
- [H] Refuted if a cell appended with `,` cannot be fetched back, or if `,` does not advance HERE by exactly one cell.
- [H] Refuted if two words compiled one after the other overlap, which is what a `,` that fails to advance HERE would cause.
- [H] Refuted if a word compiled at run time cannot be executed, or cannot be executed twice.
- [E] Not refuted by the host supplying the execution tokens that get compiled: looking a name up to find its token is `find`, which needs a dictionary of names, and that is the next experiment.

## Prior art

- [P] sectorforth defines `, ( x -- )` as `here @ ! here @ 2 + here !`, with `here` as one of five variable words it provides alongside its eight primitives.
- [I] Deriving the address of HERE from the memory map instead of naming it is the difference here, and it is what keeps R-K-01 at thirteen.

## Evidence

- [E] 11 checks pass on the Rust keel: `zero` and `here` derive their constants, `here @` is a cell-aligned dictionary address, `,` round-trips a cell and advances HERE by exactly one cell, two commas advance by two, `allot` reserves without writing, and a word compiled at run time adds, doubles, stays distinct from a second word compiled after it, and survives being executed twice.
- [E] Two negative controls run as part of `make test`. A `,` that does not advance HERE gives 6 failures, including two words compiled in sequence overwriting each other. A `here` pointing one cell on, at LATEST, is caught by the alignment check and then corrupts HERE itself, so no further thread can be assembled — which the harness reports rather than crashing on.

## Scope

In: `zero`, `here`, `,`, `allot`, and compiling an executable word at run time. Out: a dictionary of names, and therefore `find`, `create` and `:` — without names there is nothing for the outer interpreter to look words up in. Out: the immediate words `if` and `begin` themselves, which also need `state` and an immediate flag; both are at known addresses in the map, so the mechanism is now in place for them.
