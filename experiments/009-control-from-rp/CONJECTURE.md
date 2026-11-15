# 009 control-from-rp

## Hypothesis

- [H] `lit`, `branch` and `?branch` can be defined in frames from the keel primitives alone, with no primitive for inline data and none for jumping, by reading and rewriting the caller's instruction pointer through `rp@`.
- [H] `>r` and `r>` can be defined even though the keel has no `rp!`, so frames cannot move the return stack pointer: only calling and returning move it.
- [H] `rot`, which needs the return stack, then follows.

## Motivation

R-F-02. The spec's primitive-derivation diagram draws `rp@` to `>r r>` and to `branch ?branch lit`, and those to `if then begin while repeat`. That edge is the load-bearing claim of the whole layered design: if control flow is not derivable, the keel needs more primitives and the 13-word list in R-K-01 is wrong. Experiment 008 settled the data words but deliberately left control flow out, because it needs the threaded-code layout a real keel exposes and gforth does not expose the same way.

The sharp sub-claim is `>r`. Storing through `rp@` can overwrite a return-stack cell but cannot add one, so growing the return stack looks impossible without `rp!`.

## Falsification criteria

- [H] Refuted if any of `lit`, `branch`, `?branch`, `>r`, `r>`, `rot` needs a keel primitive outside `@ ! sp@ rp@ 0= + nand exit`, or needs a number literal.
- [H] Refuted if `?branch` cannot be written without a conditional, since no conditional exists yet at that point in the derivation.
- [H] Refuted if a thread cannot jump backwards, which would mean no loop can be built.
- [H] Refuted if `>r` leaves the return stack unbalanced, so that the word containing it cannot return.
- [E] Not refuted by the host assembling the threads that exercise these words: `if`, `then`, `begin` and `until` are immediate words over `here` and `,`, which frames cannot compile for itself until it has a compiler. That is the next experiment, not this one.

## Prior art

- [P] sectorforth derives all of this from the same eight primitives, with no `rp!`: `branch` is `rp@ @ dup @ + rp@ !`, `?branch` masks the offset with `0=`, and `lit` steps the return address past the inline cell.
- [P] sectorforth's `>r` is the key trick: it parks its value in its own return-address cell and then calls a second word, `>rexit`, whose call supplies the extra return-stack cell, and `>rexit` overwrites its own return address with the one `>r` displaced. `r>` is the inverse, resuming at its own `exit` cell so that `exit` runs twice.
- [I] This experiment follows that derivation on the keel, with `cell` derived rather than sectorforth's literal 2, so the same source does not assume a 16-bit cell.

## Evidence

- [E] 18 checks pass on the Rust keel: `lit` (3), `branch` (1), `?branch` (3), a hand-assembled conditional taking either arm (3), `>r`/`r>` including nesting (3), `rot` (2), and a backward-branching countdown from 5, 1 and 100 (3).
- [E] `>r` works with no `rp!`, confirming the return stack can be grown through the call mechanism alone.
- [E] Three negative controls run as part of `make test`: `lit` that forgets to step past its cell gives 11 failures, `?branch` with the wrong cell correction gives 4 (the loops hit the step limit), and `r>` pointing at the wrong exit gives 5 (an unaligned fetch inside `rot`).

## Scope

In: the control-flow words a thread executes, on the keel. Out: the words that *compile* control flow — `if`, `then`, `else`, `begin`, `while`, `repeat`, `until` are immediate and need `here`, `,`, `state` and the immediate flag. The keel exposes no such words, but its memory map fixes the system variables at known addresses, so frames should be able to derive them; that is the next step. Out: gforth as a second host, because these words depend on the caller's instruction pointer being a cell on the return stack, which is a property of this engine's threading rather than of Forth.
