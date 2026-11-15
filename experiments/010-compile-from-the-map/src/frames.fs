\ Layer-0 compilation, derived from the keel primitives and the memory map.
\
\ The keel provides no `here', no `,' and no variables at all: R-K-01 is
\ thirteen primitives and nothing else. What it does provide is R-K-04, a
\ memory map whose system variables sit at fixed, known addresses, with HERE
\ first. An address is just a number, and frames can derive numbers, so frames
\ can reach the system variables without the keel naming them.
\
\ Data and control words are repeated from experiments 008 and 009 because
\ every experiment directory is self-contained. What is new is at the bottom.

: dup ( x -- x x ) sp@ @ ;
: invert ( x -- ~x ) dup nand ;
: and ( x y -- x&y ) nand invert ;
: one ( -- 1 ) sp@ dup invert and 0= dup + invert ;
: negate ( n -- -n ) invert one + ;
: - ( a b -- a-b ) negate + ;
: cell ( -- n ) sp@ sp@ - ;
: drop ( x -- ) dup - + ;
: over ( a b -- a b a ) sp@ cell + @ ;
: swap ( a b -- b a ) over over sp@ cell cell + cell + + ! sp@ cell + ! ;
: or ( x y -- x|y ) invert swap invert and invert ;
: nip ( a b -- b ) swap drop ;
: 2dup ( a b -- a b a b ) over over ;
: 2drop ( a b -- ) drop drop ;
: = ( a b -- f ) - 0= ;
: <> ( a b -- f ) = invert ;

: lit ( -- x ) rp@ @ dup cell + rp@ ! @ ;
: branch ( -- ) rp@ @ dup @ + rp@ ! ;
: ?branch ( f -- ) 0= rp@ @ @ cell - and rp@ @ + cell + rp@ ! ;
: >rexit ( addr -- ) rp@ ! ;
: >r ( x -- ) rp@ @ swap rp@ ! >rexit ;
: r> ( -- x ) rp@ cell + @ rp@ @ rp@ cell + ! lit [ &exit ] rp@ ! ;
: rot ( a b c -- b c a ) >r swap r> swap ;

\ ---------------------------------------------------------------- compilation
\
\ Zero with no literal: sp@ makes a value, and a value anded with its own
\ complement is zero whatever it was.
: zero ( -- 0 ) sp@ dup invert and ;

\ The memory map puts HERE first among the system variables, so the address of
\ HERE is address zero. This is the one place frames depends on the map.
: here ( -- addr ) zero ;

\ Append a cell to the dictionary and advance HERE: the whole compiler, in one
\ definition, with no primitive for it.
: , ( x -- ) here @ ! here @ cell + here ! ;

\ Reserve n bytes without writing them.
: allot ( n -- ) here @ + here ! ;

