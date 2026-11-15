\ Layer-0 control flow, derived from the keel primitives @ ! sp@ rp@ 0= + nand.
\
\ The data words are repeated from experiment 008 because every experiment
\ directory is self-contained. What is new is everything below "control flow":
\ lit, branch, ?branch, >r, r> and rot, none of which needs a primitive the
\ keel does not have. No number literals appear in this file.

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

\ ---------------------------------------------------------------- control flow
\
\ Calling a word leaves the caller's instruction pointer on the return stack,
\ and rp@ is the address of that cell. So a word can read the cell its caller
\ was about to execute, and can choose where its caller resumes. That is the
\ whole mechanism: inline data and jumps, with no primitive for either.

\ Push the cell compiled after the call and resume past it.
: lit ( -- x ) rp@ @ dup cell + rp@ ! @ ;

\ Resume at the inline offset, measured from the offset cell itself.
: branch ( -- ) rp@ @ dup @ + rp@ ! ;

\ Resume at the inline offset when the flag is false, else past the offset.
\ Branchless: 0= turns the flag into a mask, so one of the two addresses is
\ added and the other is zeroed. No conditional exists yet to write this with.
: ?branch ( f -- ) 0= rp@ @ @ cell - and rp@ @ + cell + rp@ ! ;

\ The return stack cannot be grown by storing: rp only moves when a word is
\ called or returns. >r therefore parks its value in its own return-address
\ cell and calls >rexit, whose call supplies the extra cell, and >rexit
\ overwrites its own return address with the one >r displaced.
: >rexit ( addr -- ) rp@ ! ;
: >r ( x -- ) rp@ @ swap rp@ ! >rexit ;

\ r> is the inverse: lift the parked value out, put this word's own exit
\ address where the value was, and resume there, so exit runs twice and the
\ cell >r added is finally popped.
: r> ( -- x ) rp@ cell + @ rp@ @ rp@ cell + ! lit [ &exit ] rp@ ! ;

: rot ( a b c -- b c a ) >r swap r> swap ;
