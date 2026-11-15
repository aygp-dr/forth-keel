# 010 compile-from-the-map: proof

- `zero ( -- 0 )` is `sp@ dup invert and`: `sp@` manufactures a value, and a
  value anded with its own complement is zero whatever it was.
- `here ( -- addr )` is `zero`. The memory map puts the system variables first
  in memory with HERE first among them, so the address of HERE is address
  zero. This is the single place frames depends on the map, and it is why the
  keel needs no variable words.
- `, ( x -- )` is `here @ ! here @ cell + here !`: store at HERE, then advance
  HERE by one cell. That is the entire compiler.
- `allot ( n -- )` is `here @ + here !`.

The payoff is the last three checks. A thread remembers HERE on the return
stack, commas `[+ exit]` into the dictionary, pushes two numbers, and
`execute`s the address it saved — so frames builds a word at run time and runs
it, using nothing the keel named for it. Compiling `[dup + exit]` instead
gives a doubling word, and compiling two bodies in sequence and executing both
shows HERE really advanced between them.

Run: `make test`. Expected: `11 checks, 0 failures`, then two negative
controls that must each fail.

The execution tokens being compiled are supplied by the host as inline data.
Turning a *name* into a token is `find`, which needs a dictionary of names;
that is the next experiment, and it is the last thing between here and an
outer interpreter.

The keel host skips with a message naming the `wasm-pack` command when no wasm
build is present, so `make test` passes on a machine that has never built the
kernel.
