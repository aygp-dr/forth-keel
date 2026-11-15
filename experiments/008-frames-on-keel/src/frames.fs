\ Layer-0 data words, defined from the keel primitives @ ! sp@ 0= + nand only.
\
\ No number literals appear anywhere below. The keel has no literal primitive,
\ so on a real keel a literal is a host facility, not a keel one: experiment
\ 001 borrowed gforth's number parser for the 1 in negate. Here 1 is derived,
\ so this file needs nothing from its host but : ; ( and \ .
\
\ Definitions are in dependency order. Each uses only the keel primitives and
\ frames words defined above it.

: dup ( x -- x x ) sp@ @ ;
: invert ( x -- ~x ) dup nand ;
: and ( x y -- x&y ) nand invert ;

\ ( -- 1 ) with no literal: sp@ makes a value, x nand-folded against its own
\ complement is 0 whatever x was, 0= makes -1, -1 + -1 is -2, and ~-2 is 1.
: one ( -- 1 ) sp@ dup invert and 0= dup + invert ;

: negate ( n -- -n ) invert one + ;
: - ( a b -- a-b ) negate + ;

\ The distance between two stack pointers is one cell, whatever a cell is.
: cell ( -- n ) sp@ sp@ - ;

\ Subtracting a copy of x from the cell below leaves that cell unchanged and
\ consumes x. Needs a cell below x, as it does on any host.
: drop ( x -- ) dup - + ;

: over ( a b -- a b a ) sp@ cell + @ ;
: swap ( a b -- b a ) over over sp@ cell cell + cell + + ! sp@ cell + ! ;
: or ( x y -- x|y ) invert swap invert and invert ;
: nip ( a b -- b ) swap drop ;
: tuck ( a b -- b a b ) swap over ;
: 2dup ( a b -- a b a b ) over over ;
: 2drop ( a b -- ) drop drop ;
: = ( a b -- f ) - 0= ;
: <> ( a b -- f ) = invert ;
