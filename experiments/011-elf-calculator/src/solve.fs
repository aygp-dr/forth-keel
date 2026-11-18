\ solve.fs -- greedy solver. Load after elf.fs.
\
\ Reads a puzzle header (puzzle, limit, targets:, start:) followed by the
\ word solve, and prints a complete puzzle file: the same header, a program,
\ and done. elf.fs executes each instruction as it is printed, so the solver
\ always knows what is held and covered, but the printed text is what gets
\ judged: a separate gforth run replays it.
\
\ For each target t not yet covered, take the cheapest of
\   - double-and-add from 1:              bits-1 + ones-1 instructions
\   - a held value v, plus or minus d:    cost(d) + 1, where d = |t - v|
\ With reuse? set, also
\   - two held values combined by + - * / into t:  1 instruction
\   - d already held:                               cost(d) = 0
\ and each target goes to the next register of a ring rather than always to
\ register 46, so earlier targets stay held as bases for later ones.
\ Starting values are never overwritten.

[defined] reuse? 0= [if] false constant reuse? [then]

47 constant one-r     \ holds 1 unless a starting value already does
46 constant prev-r    \ the only destination without reuse
45 constant diff-r    \ scratch for d

variable one          \ register that holds 1
variable #ops
variable ring         \ next destination when reuse? is set

: 3dup ( a b c -- a b c a b c ) 2 pick 2 pick 2 pick ;

: bitlen ( n -- b ) 0 swap begin dup while 1 rshift swap 1+ swap repeat drop ;
: ones ( n -- c ) 0 swap begin dup while dup 1 and rot + swap 1 rshift repeat drop ;
\ instructions to build n >= 1 by double-and-add from a register holding 1
: cost ( n -- c ) dup bitlen 1- swap ones 1- + ;

: .op ( i j d c -- )
  >r rot 0 .r space swap 0 .r space 0 .r space ." r" r> emit
  1 #ops +!  #ops @ 8 mod 0= if cr else space then ;
: e+ ( i j d -- ) 3dup [char] + .op r+ ;
: e- ( i j d -- ) 3dup [char] - .op r- ;
: e* ( i j d -- ) 3dup [char] * .op r* ;
: e/ ( i j d -- ) 3dup [char] / .op r/ ;

: reg@ ( r -- v ) cells regs + @ ;
: held? ( r -- f ) written + c@ 0<> ;

\ register holding v, or -1
: holder ( v -- r )
  #regs 0 ?do i held? if dup i reg@ = if drop i unloop exit then then loop drop -1 ;

: dcost ( d -- c ) reuse? if dup holder 0>= if drop 0 exit then then cost ;

: dest ( -- r )
  reuse? 0= #start @ diff-r >= or if prev-r exit then
  ring @ dup 1+ dup diff-r >= if drop #start @ then ring ! ;

\ Build n >= 2 into acc by double-and-add, starting from the 1 in one.
: build ( n acc -- )
  0 0 { n acc src k }
  one @ to src  n bitlen 2 - to k
  begin k 0>= while
    src src acc e+  acc to src
    n k rshift 1 and if acc one @ acc e+ then
    k 1- to k
  repeat ;

\ Put d in a register and return it: a holder, one, or diff-r.
: d>reg ( d -- r )
  dup 1 = if drop one @ exit then
  reuse? if dup holder dup 0>= if nip exit then drop then
  diff-r build diff-r ;

: need-one ( -- )
  #start @ 0 ?do i reg@ 1 = if i one ! unloop exit then loop
  0 0 one-r e/  one-r one ! ;

\ Two held values that make t in one instruction: i j op true, or false.
: pair ( t -- i j c true | false )
  0 0 { t a b }
  #regs 0 ?do i held? if  i reg@ to a
    #regs 0 ?do i held? if  i reg@ to b
      a b + t = if j i [char] + true unloop unloop exit then
      a b > if a b - t = if j i [char] - true unloop unloop exit then then
      a t b / <= if a b * t = if j i [char] * true unloop unloop exit then then
      a b mod 0= if a b / t = if j i [char] / true unloop unloop exit then then
    then loop
  then loop false ;

\ Cheapest way to make t by a base: cost, base register (-1 = from 1), op.
: plan ( t -- cost base op )
  dup cost -1 [char] + { t best base opc }
  #regs 0 ?do
    i diff-r <> i held? and if
      i reg@ t < if t i reg@ - dcost 1+ dup best < if to best i to base [char] + to opc else drop then then
      i reg@ t > if i reg@ t - dcost 1+ dup best < if to best i to base [char] - to opc else drop then then
    then
  loop best base opc ;

: make-pair ( i j c -- )
  >r dest r> case
    [char] + of e+ endof  [char] - of e- endof
    [char] * of e* endof  [char] / of e/ endof
  endcase ;

: make ( t -- )
  reuse? if dup pair if make-pair drop exit then then
  dup plan { t c base opc }
  base 0< if t dest build exit then
  opc [char] + = if
    t base reg@ - d>reg  base swap dest e+
  else
    base reg@ t - d>reg  base swap dest e-
  then ;

: solve ( -- )
  #start @ prev-r >= abort" no free register"
  0 #ops !  #start @ ring !
  ." puzzle " id-buf count type ."   limit " vlimit @ 0 .r cr
  ." targets: " #tgt @ 0 ?do i cells tgt + @ 0 .r space loop ." ;" cr
  ." start: " #start @ 0 ?do i reg@ 0 .r space loop ." ;" cr
  need-one
  #tgt @ 0 ?do
    covered i + c@ 0= if i cells tgt + @ make then
  loop
  #ops @ 8 mod if cr then ." done" cr ;
