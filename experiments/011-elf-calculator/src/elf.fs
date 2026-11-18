\ elf.fs -- replay an Elf Calculator program and check it against the rules.
\
\ Santa 2026 - The Elf Calculator (Reade and Cruz, Kaggle, 2026). A program
\ is plain Forth here: each instruction is a word that takes its three
\ register numbers from the stack, so "0 0 1 r/" is ["/", 0, 0, 1]. Replaying
\ a program is interpreting it. Needs 64-bit cells: value_limit reaches 1e18.
\
\ A puzzle file reads:
\   puzzle example_0001  limit 9  targets: 1 2 5 8 ;  start: 3 ;
\   0 0 1 r/  0 1 2 r-  0 2 3 r+  0 3 4 r+
\   done
\ and done prints one line:
\   RESULT <id> ok|fail <code> <steps> <covered> <targets> <lower-bound>

warnings off
1 cells 8 < [if] .( elf.fs needs 64-bit cells) cr bye [then]

48   constant #regs
4096 constant #steps-max
8192 constant #targets-max

create regs     #regs cells allot
create written  #regs allot
create tgt      #targets-max cells allot
create covered  #targets-max allot
create id-buf   64 allot

variable #tgt
variable #start     \ how many starting values
variable vlimit      \ value_limit
variable steps
variable err        \ 0 = legal so far; the first failure's code sticks
variable bound      \ targets not already covered by the starting values

\ failure codes
1 constant e-register     \ register index outside 0..47
2 constant e-undefined    \ source register never written
3 constant e-subtract     \ a - b with a <= b
4 constant e-divide       \ b does not divide a
5 constant e-limit        \ result above value_limit
6 constant e-length       \ more than 4096 instructions
7 constant e-uncovered    \ a target never appeared

: fail ( code -- ) err @ 0= if err ! else drop then ;

\ ---------------------------------------------------------------- parsing
: next-word ( -- addr u )
  begin parse-name dup 0= while 2drop refill 0= abort" unexpected end of file" repeat ;
: >num ( addr u -- n ) s>number? 0= abort" not a number" d>s ;
: ;? ( addr u -- f ) s" ;" compare 0= ;

\ ---------------------------------------------------------------- coverage
\ Targets arrive sorted, so a write looks itself up by binary search.
: cover ( v -- )
  0 #tgt @ 0 { v lo hi mid }
  begin lo hi < while
    lo hi + 2/ to mid
    mid cells tgt + @ v = if 1 covered mid + c! exit then
    mid cells tgt + @ v < if mid 1+ to lo else mid to hi then
  repeat ;

: #covered ( -- n ) 0 #tgt @ 0 ?do covered i + c@ + loop ;

\ ---------------------------------------------------------------- puzzle
: puzzle ( "id" -- )
  next-word 63 min id-buf place
  regs #regs cells erase  written #regs erase
  covered #targets-max erase  0 #tgt !  0 steps !  0 err !  0 vlimit ! ;

: limit ( "n" -- ) next-word >num vlimit ! ;

: targets: ( "t1 .. tn ;" -- )
  0 begin next-word 2dup ;? 0= while
    >num over cells tgt + !  1+
    dup #targets-max >= abort" too many targets"
  repeat 2drop #tgt ! ;

\ Starting values fill registers 0, 1, ... and cover any target they equal.
: start: ( "v1 .. vn ;" -- )
  0 begin next-word 2dup ;? 0= while
    >num over #regs >= abort" too many starting values"
    dup cover over cells regs + !  1 over written + c!  1+
  repeat 2drop #start !
  #tgt @ #covered - bound ! ;

\ ---------------------------------------------------------------- replay
: reg? ( r -- f ) 0 #regs within ;

: src ( r -- v )
  dup reg? 0= if drop e-register fail 1 exit then
  dup written + c@ 0= if drop e-undefined fail 1 exit then
  cells regs + @ ;

: dst! ( v r -- )
  dup reg? 0= if 2drop e-register fail exit then
  over vlimit @ u> if 2drop e-limit fail exit then
  over cover  1 over written + c!  cells regs + ! ;

: (+) ( a b -- v ) + ;
: (-) ( a b -- v ) 2dup > 0= if 2drop e-subtract fail 1 exit then - ;
: (*) ( a b -- v ) um* if drop e-limit fail 1 exit then ;
: (/) ( a b -- v ) 2dup mod if 2drop e-divide fail 1 exit then / ;

\ Both sources are read before the destination is written.
: op ( i j d xt -- )
  { i j d xt }
  err @ if exit then
  1 steps +!  steps @ #steps-max > if e-length fail exit then
  i src j src  err @ if 2drop exit then
  xt execute   err @ if drop exit then
  d dst! ;

: r+ ( i j d -- ) ['] (+) op ;
: r- ( i j d -- ) ['] (-) op ;
: r* ( i j d -- ) ['] (*) op ;
: r/ ( i j d -- ) ['] (/) op ;

: done ( -- )
  err @ 0= if #covered #tgt @ < if e-uncovered fail then then
  ." RESULT " id-buf count type
  err @ if ."  fail " else ."  ok " then
  err @ 0 .r space  steps @ 0 .r space  #covered 0 .r space
  #tgt @ 0 .r space  bound @ 0 .r cr ;
