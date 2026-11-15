> Derived from spec.org @ v0.1. The spec governs; if this file disagrees with it, this file is the bug. Do not add facts here that are not in the spec — add them to the spec first.

# forth-keel

A Forth system built in three layers, each written on top of the one
below, and tested against a fixed oracle corpus of SICP programs.

| Layer    | Written in | Contents |
|----------|------------|----------|
| keel     | host language (Rust, C) | 13 primitives and the inner interpreter |
| frames   | Forth | stack words, arithmetic, control flow, compiler, outer interpreter |
| planking | Forth | tagged cells, pairs, symbols, closures, promises, Cheney GC, reader |

```mermaid
flowchart BT
  subgraph K["keel (host language, ~13 primitives)"]
    direction LR
    k1["@  !  sp@  rp@"]
    k2["0=  +  nand"]
    k3["exit  execute"]
    k4["key  emit"]
    k5["um*  um/mod"]
  end
  subgraph F["frames (Forth, ~150 lines)"]
    direction LR
    f1["stack: dup drop swap over rot"]
    f2["arith/logic: invert and or negate - * / mod"]
    f3["control: branch ?branch lit if then begin while repeat do loop"]
    f4["compiler: : ; create , here immediate"]
    f5["outer interpreter: parse find number"]
  end
  subgraph P["planking (Forth, ~300 lines)"]
    direction LR
    p1["tagged cells"]
    p2["pairs: cons car cdr"]
    p3["symbols: intern eq?"]
    p4["closures + apply"]
    p5["delay / force"]
    p6["Cheney GC"]
    p7["reader"]
  end
  subgraph S["SICP programs (sea-trials 200-499)"]
    s1["ch1: procedures, recursion, higher-order"]
    s2["ch2: data abstraction, lists"]
    s3["ch3: state, streams"]
    s4["ch4: metacircular evaluator"]
  end
  K --> F --> P --> S
```

## Build sequence

```mermaid
flowchart TD
  M0["M0 oracle: sea-trials + runner (done)"]
  M1["M1 frames data words on gforth, exp 001 (done)"]
  M2["M2 keel-rs native: 13 primitives + NEXT"]
  M3["M3 frames complete on keel-rs: sea-trials layer 0"]
  M4["M4 keel-rs to wasm32 + node runner: layer 0"]
  M4b["M4b WAForth host via node: layer 0 baseline"]
  M5["M5 ClojureScript app loads the wasm keel"]
  M6["M6 planking cells, pairs, symbols, exp 002 onward"]
  M7["M7 closures, apply, delay/force"]
  M8["M8 Cheney GC"]
  M9["M9 reader + metacircular evaluator (SICP 4.1)"]
  M0 --> M1 --> M2 --> M3 --> M4 --> M5
  M0 --> M4b --> M5
  M3 --> M6 --> M7 --> M8 --> M9
  M4 --> M9
```

## Commands

- `make test` — code gate: experiments and sea-trials on gforth
- `make docs` — docs gate, including negative fixtures
- `make tangle` — regenerate derived files from `spec.org`
- `make tangle-check` — confirm derived files match `spec.org`
- `scripts/pull-resources.sh` — fetch reference sources into `resources/vendor/`

All diagrams: [docs/architecture.md](docs/architecture.md).
Governing spec: [spec.org](spec.org).
