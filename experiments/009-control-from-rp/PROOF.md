# 009 control-from-rp: proof

The mechanism is one observation: calling a word leaves the caller's
instruction pointer on the return stack, and `rp@` is the address of that
cell. So a word can read the cell its caller was about to execute, and can
decide where its caller resumes.

- `lit ( -- x )` is `rp@ @ dup cell + rp@ ! @`: take the return address, store
  it one cell on so the caller resumes past the data, then fetch the data.
- `branch` is `rp@ @ dup @ + rp@ !`: the inline cell is an offset from itself.
- `?branch` is `0= rp@ @ @ cell - and rp@ @ + cell + rp@ !`. It cannot use a
  conditional, because none exists yet, so `0=` turns the flag into a mask and
  one of the two candidate addresses is zeroed out. Offsets are measured from
  the offset cell, and `cell -` then `cell +` is what makes both arms land
  correctly.
- `>r` cannot grow the return stack by storing, because `rp` moves only when a
  word is called or returns. It parks its value in its own return-address cell
  and calls `>rexit`; that call supplies the extra cell, and `>rexit`
  overwrites its own return address with the one `>r` displaced.
- `r>` lifts the parked value out, writes its own `exit` address where the
  value was, and resumes there, so `exit` runs twice and the cell `>r` added
  is finally popped. Its own exit address is the one thing frames cannot
  compute for itself yet, so the seed assembler resolves `&exit`.
- `rot` is then `>r swap r>`.

Run: `make test`. Expected: `18 checks, 0 failures`, then three negative
controls that must each fail.

The threads that exercise these words are assembled by the host, because
`if` and `begin` are immediate words over `here` and `,`. Two scaffolds in the
seed assembler make that possible and are the compiler's job: `[ n ]` compiles
raw cells, and `&exit` resolves a definition's own exit address.

The keel host skips with a message naming the `wasm-pack` command when no wasm
build is present, so `make test` passes on a machine that has never built the
kernel.
