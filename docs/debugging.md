> Derived from spec.org @ v0.1. The spec governs; if this file disagrees with it, this file is the bug. Do not add facts here that are not in the spec — add them to the spec first.

# Debugging forth-keel

gforth 0.7.3 is both the oracle and the debugger (R-H-01), so one engine
answers "what should this print?" and "why did it print that?". The build
flags, the engines and the gdb commands are specified in
[../spec.org](../spec.org) under *Debug toolchain*; this file is how to use
them.

## Start in Forth, not in gdb

Most questions are answered without a debugger at all.

| You want | Do this |
|---|---|
| the stack at a point inside a definition | put `~~` there: it prints file, line, depth and the stack |
| to know which word threw | nothing — a throw that escapes prints a backtrace naming the Forth words |
| to read back what you compiled | `see word` |
| to step a definition | `dbg word` |

```
$ gforth -e ': inner 1 0 / ;' -e ': outer inner ;' -e 'outer'
*OS command line*:-1: Division by zero
>>>outer<<<
Backtrace:
$277B4E6572F0 /
$277B4E657330 inner
```

## Then gdb, in Forth terms

```sh
gdb -x tools/gforth.gdb --args gforth yourfile.fs -e bye
```

- `fstart` — run to where the primitive table exists. Do this first:
  `vm_prims[]` is populated only after the image loads, so resolving a
  primitive earlier reads uninitialised memory.
- `fbreak <word>` — break on a Forth primitive by name, with an optional
  condition. This is the sharpest tool here: it stops on one *particular*
  execution of a word, by what is on the Forth stack.
- `fstack` / `frstack` — cells from `sp` / `rp`. `sp[0]` is the top.
- `fprims` — every primitive in this build, with its index.

Stopping on one specific addition:

```
(gdb) fstart
(gdb) fbreak + if sp[0]==3 && sp[1]==2
fbreak: "+" is primitive 53 at 0x405867
(gdb) continue
Breakpoint 2, gforth_engine (...) at engine/prim.i:2896
(gdb) fstack 2
sp = 0x800826ff0
  sp[0] = 3   <- top
  sp[1] = 2
```

## Why a primitive is an address, not a function

gforth compiles every primitive as a label inside one `gforth_engine`
function, generated into `engine/prim.i`, and dispatches between them with
computed gotos. So there is no `plus()` to break on; `fbreak` resolves the
name through `prim_names[]` to the label address recorded in `vm_prims[]`.
`engine/prim.i` is real, compiled source with debug information, so
`list`, `info locals` and line breakpoints all work inside it.

## Where the top of stack lives

It depends on the build. With the spec's debug flags the top of stack is in
memory at `sp[0]`, which is what makes `fbreak + if sp[0]==3` possible.
gforth's `-DDEBUG` engine instead caches it in a separate `spTOS` variable,
and computes depth against `sp0` with an offset — so a helper written for
one build misreads the other. `fstack` therefore prints cells from `sp`
rather than claiming a depth.

## Engines

| Engine | Use |
|---|---|
| `gforth` | debugging: backtraces, and what gdb attaches to |
| `gforth-fast` | long corpus runs |
| `gforth-itc` | indirect threaded, for comparing dispatch |
| `gforth-ditc` | direct threaded, bootstraps the image |
| `gforth-prof` | prints frequently occurring primitive sequences |

`gforth-prof` is the one to reach for before optimising anything in
`frames`: it reports which primitive sequences actually run, which is the
evidence for adding a superinstruction rather than a guess.

## Two traps

- **`-DDEBUG` pollutes output.** It traces every primitive with its ip,
  depth and stack to stderr. The sea-trials compare output, so a `-DDEBUG`
  engine fails them all. Build it separately if you want it; never make it
  the engine on `PATH`.
- **`VM_DEBUG` does not compile.** It appears in the generated `prim.i`,
  but gforth 0.7.3 never declares `vm_debug`, `vm_out` or `printarg_n`, so
  that branch is dead. Do not reach for it.

## The debug build must still be the oracle

Changing build flags must not change behaviour. The check is the corpus:

```sh
sea-trials/run.sh        # must be 29/29 against .out files from a stock 0.7.3
```

A debug engine that fails a sea-trial is not a debugging aid, it is a
second bug.
