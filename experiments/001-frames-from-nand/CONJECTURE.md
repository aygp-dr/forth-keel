# 001 frames-from-nand

## Hypothesis

- [H] The frames data words `dup drop swap over nip tuck 2dup 2drop invert and or negate - = <> cell` can be defined using only the keel primitives `@ ! sp@ 0= + nand` and number literals, and each behaves exactly like the gforth word of the same name.

## Motivation

R-F-01. If the data words need more than these primitives, the keel primitive list in the spec is wrong before any host is written.

## Falsification criteria

- [H] Refuted if any of the 16 words produces a different result or stack from gforth's word on any of 9 values per argument (-1, 0, 1, 2, 7, 255, -256, max-n, min-n), or on the stack `10 20 30` for stack words.
- [H] Refuted if a frames definition can use a word outside `keel-wl` and `frames-wl` (the isolation would be fake).

## Prior art

- [P] sectorforth defines `dup` as `sp@ @`, `drop` as `dup - +`, and `swap` with `sp@` and `!` from 8 primitives.
- [P] Minimal Forth (uho/minimal) builds a restricted wordlist on a host Forth to check which words are primitive.

## Scope

In: data-stack, arithmetic and logic words, on gforth as host. Out: control flow (`branch`, `if`), the return stack, the compiler, and the outer interpreter; these depend on threaded-code layout and need a real keel (M2).
