\ requires: core core-ext
\ layer: 0
\ SICP 1.2.1 linear recursion and iteration.
: fac ( n -- n! ) dup 2 < if drop 1 else dup 1- recurse * then ;
: fact-iter ( product counter max -- n! )
  2dup > if 2drop exit then
  >r tuck * swap 1+ r> recurse ;
: fac-i ( n -- n! ) 1 1 rot fact-iter ;
: main ( -- ) 13 0 do i . i fac . i fac-i . cr loop ;
main
