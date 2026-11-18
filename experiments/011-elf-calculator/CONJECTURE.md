# 011 elf-calculator

## Hypothesis

- [H] A gforth system with 64-bit cells can replay and judge Santa 2026 "Elf Calculator" programs exactly, with each instruction written as a Forth word (`0 1 2 r+` is `["+", 0, 1, 2]`), so that replaying a program is interpreting it.
- [H] A greedy solver that keeps earlier targets in a ring of registers and tries every pair of held values first ("reuse") uses fewer instructions than one that always builds from 1 or from a single base ("baseline"), on every puzzle family.

## Motivation

Santa 2026 - The Elf Calculator (Reade and Cruz, Kaggle, 2026) scores the total length of 1,000 straight-line register programs over `+ - * /` with exact division, 48 registers and a `value_limit` of at most 10^18. A straight-line register program is postfix already, which makes it a natural workload for a Forth: the program text is the source, and the outer interpreter is the replayer. This experiment is applied rather than layered — it uses gforth, not the keel, because the keel's 32-bit cells (R-K-03) cannot hold a value up to 10^18 in one cell, and R-K-01 forbids adding the wider arithmetic as primitives.

## Falsification criteria

- [H] Refuted if the replayer accepts a program that breaks a rule on the Data page: register outside 0..47, undefined source, `a - b` with `a <= b`, inexact division, a result above `value_limit` (including a product that wraps 64 bits), more than 4,096 instructions, or a target never produced.
- [H] Refuted if the replayer rejects the Data page's worked example, or counts it as anything but 4 instructions.
- [H] Refuted if a solver program that gforth accepts is rejected by `tools/replay.py`, an independent Python implementation of the same rules reading the submission CSV, or if the two totals differ.
- [H] Refuted if the reuse solver is not shorter in total than the baseline on the synthetic set, or longer on any family.
- [E] Not refuted by the solvers being far from the lower bound: the bound counts one instruction per target and is loose by construction.

## Prior art

- [P] Addition chains: the shortest chain to n is at least log2(n) + log2(ν(n)) - 2.13 steps (Schönhage 1975); double-and-add gives at most 2·log2(n). Knuth, TAOCP vol. 2, §4.6.3.
- [P] Tony Li, "Elf Calculator With Register Reuse" (Kaggle notebook, 2026-11-17): caches short expressions, searches backward from each target, then assigns the 48 registers; public score 191,867.
- [I] The synthetic families in `tools/gen.py` are guesses at what the Data page's family names mean, not the competition's generators.

## Evidence

- [E] `gmake test` passes: 12 rule puzzles give their expected codes; the worked example replays in 4; the reuse solver finds a 4-instruction program for it; both solvers' 100 synthetic programs are accepted by gforth and by `tools/replay.py` with identical totals.
- [E] Five negative controls are rejected: four `elf.fs` mutants that each drop one rule (divide, limit, subtract, undefined) change `tests/rules.out`, and a submission with one operator flipped fails the Python replay.
- [E] Seed 2026, 100 puzzles: baseline 58,030 instructions, reuse 53,229 (-8.3%), lower bound 1,610. By family, reuse/baseline: tiny 576/596, independent 15,573/15,901, shared 13,810/16,091, multiplicative 3,018/4,058, mixed 20,252/21,384.

## Scope

In: the replayer, both greedy solvers, CSV in and out, the cross-check. Out: search (backward or meet-in-the-middle), register allocation as a separate pass, and any result on the scored puzzles — those need the competition rules accepted before the data can be downloaded, after which `gmake kaggle` runs the same pipeline on `data/puzzles.csv`.
