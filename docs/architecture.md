> Derived from spec.org @ v0.1. The spec governs; if this file disagrees with it, this file is the bug. Do not add facts here that are not in the spec — add them to the spec first.

# forth-keel architecture

Diagram sources are in [diagrams/](diagrams/). Text, requirements and
labels are in [../spec.org](../spec.org).

## 1. Kernel levels

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

## 2. Primitive derivation

```mermaid
flowchart LR
  fetch["@"] --> dup
  spat["sp@"] --> dup
  nand --> invert
  dup --> invert
  invert --> and_["and"]
  nand --> and_
  invert --> negate
  plus["+"] --> negate
  negate --> minus["-"]
  plus --> minus
  spat --> cell
  minus --> cell
  dup --> drop
  minus --> drop
  plus --> drop
  spat --> over
  cell --> over
  fetch --> over
  over --> swap
  store["!"] --> swap
  cell --> swap
  swap --> or_["or"]
  invert --> or_
  and_ --> or_
  rpat["rp@"] --> tor[">r  r>"]
  swap --> tor
  rpat --> branch["branch  ?branch  lit"]
  zeq["0="] --> branch
  branch --> ifthen["if then begin while repeat"]
  tor --> rot
  swap --> rot
  umstar["um*"] --> star["*"]
  umslash["um/mod"] --> divmod["/  mod  /mod"]
```

## 3. Memory map

```mermaid
flowchart LR
  subgraph MEM["linear memory (low address to high)"]
    direction LR
    sys["system variables: here latest state base hp"]
    tib["terminal input buffer"]
    dict["dictionary: headers + threaded code, grows up"]
    from["heap semispace A (from-space)"]
    to["heap semispace B (to-space)"]
    ds["data stack, grows down"]
    rs["return stack, grows down"]
  end
  sys --- tib --- dict --- from --- to --- ds --- rs
```

## 4. Inner and outer interpreter

```mermaid
stateDiagram-v2
  [*] --> ReadToken
  ReadToken --> Lookup: token parsed
  ReadToken --> Refill: input empty
  Refill --> ReadToken: line read
  Refill --> [*]: end of input
  Lookup --> Execute: found, state=0 or immediate
  Lookup --> Compile: found, state=1, not immediate
  Lookup --> Number: not found
  Number --> PushLiteral: parsed, state=0
  Number --> CompileLiteral: parsed, state=1
  Number --> Error: not a number
  Execute --> ReadToken
  Compile --> ReadToken
  PushLiteral --> ReadToken
  CompileLiteral --> ReadToken
  Error --> ReadToken: reset stacks, state=0
```

## 5. Object layout (planking)

```mermaid
classDiagram
  class Cell {
    low bit 0: fixnum n*2
    low bit 1: object addr+1
  }
  class Pair {
    header = t-pair
    car : Cell
    cdr : Cell
  }
  class Symbol {
    header = t-symbol
    length
    chars
  }
  class Closure {
    header = t-closure
    xt
    env : Cell
  }
  class Promise {
    header = t-promise
    forced flag
    thunk or value : Cell
  }
  class Forward {
    header = t-forward
    new address
  }
  Cell --> Pair
  Cell --> Symbol
  Cell --> Closure
  Cell --> Promise
  Pair ..> Forward : during GC
```

## 6. Garbage collection (Cheney)

```mermaid
sequenceDiagram
  participant M as mutator
  participant A as allocator
  participant C as collector
  participant T as to-space
  M->>A: cons (needs 3 cells)
  A->>A: free + 3 > limit?
  A->>C: collect
  C->>C: swap from-space and to-space
  loop each root (data stack, return stack, globals)
    C->>T: copy object, leave forward in from-space
  end
  loop scan < free
    C->>T: for each field: copy referent or follow forward
  end
  C-->>A: free pointer reset to end of copied data
  A-->>M: new pair
```

## 7. System context

```mermaid
flowchart TB
  subgraph SRC["Forth source (identical on every host)"]
    frames["frames/*.fs"]
    planking["planking/*.fs"]
  end
  subgraph HOSTS["hosts"]
    gforth["gforth 0.7.3 (reference oracle)"]
    rsnative["keel-rs native (cargo test)"]
    rswasm["keel-rs wasm32 (wasm-bindgen)"]
    waforth["WAForth (comparison host)"]
  end
  subgraph WEB["browser"]
    cljs["ClojureScript app (shadow-cljs)"]
    ui["REPL, stack view, dictionary view"]
  end
  trials["sea-trials runner"]
  SRC --> rsnative
  SRC --> rswasm
  rswasm --> cljs
  waforth --> cljs
  cljs --> ui
  gforth -->|generates .out| trials
  rsnative -->|checked by| trials
  rswasm -->|checked by, via node| trials
  waforth -->|checked by, via node| trials
```

## 8. ClojureScript interop

```mermaid
sequenceDiagram
  participant U as user
  participant C as ClojureScript
  participant J as wasm-bindgen glue
  participant K as keel (wasm)
  C->>J: dynamic-import /pkg/keel.js
  J->>K: instantiate keel_bg.wasm
  C->>K: new Forth() (loads frames + planking)
  U->>C: types "10 4 cc ."
  C->>K: interpret(src)
  K-->>C: Ok("292 ")
  C->>K: stack()
  K-->>C: [ ]
  C-->>U: render output, stack, dictionary
```

## 9. Build sequence

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

## 10. Gating by sea-trials layer

```mermaid
flowchart LR
  subgraph L0["layer 0 (000-090, 110, 210-300, 320-330): 22 tests"]
    t0["stack, arith, logic, memory, control, xt, does>, base"]
    t1["SICP ch1-3 programs on fixed pools"]
  end
  subgraph L1["layer 1 (400-499, to be written)"]
    t2["cells, pairs, symbols, closures, promises, GC, reader"]
  end
  subgraph LX["layer x (100, 120-150, 200, 310): 7 tests"]
    t3["strings, exceptions, floats, heap, locals"]
  end
  L0 -->|gates| M3["M3, M4, M4b"]
  L1 -->|gates| M6["M6-M9"]
  LX -->|reported, never gates| R["runner summary"]
```

## 11. Incremental development loop

```mermaid
stateDiagram-v2
  [*] --> Conjecture: CONJECTURE.md with falsification criteria
  Conjecture --> Proof: make test passes in the experiment dir
  Proof --> Refutation: adversarial inputs, 2+ evidence entries
  Refutation --> Refinement: SURVIVED
  Refutation --> NewConjecture: REFUTED, file next number
  NewConjecture --> Conjecture
  Refinement --> Layer: moved into keel/ frames/ planking/ + ADR
  Layer --> [*]: sea-trials layer passes
```

## 12. Repository layout

```mermaid
flowchart LR
  root["forth-keel/"]
  root --> spec["spec.org (governs)"]
  root --> derived["README.org AGENTS.md CLAUDE.md (tangled)"]
  root --> docs["docs/ architecture.md, diagrams/, review-ledger.org"]
  keelrepo["aygp-dr/keel-rs (M2, M4): keel/ keel-wasm/"]
  root -.-> keelrepo
  root --> frames["frames/ (M3)"]
  root --> planking["planking/ (M6)"]
  root --> trials["sea-trials/ tests/*.fs, *.out, run.sh"]
  root --> exps["experiments/ 000-quickstart 001 002"]
  root --> gates["gates/ docs-gate.sh, tangle-check.sh, fixtures/"]
  root --> tools["tools/ forthfmt.py, mmcheck.mjs"]
  root --> res["resources/ README.md, vendor/ (pulled, untracked)"]
```
