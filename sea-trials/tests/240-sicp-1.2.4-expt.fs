\ requires: core core-ext
\ layer: 0
\ SICP 1.2.4 exponentiation: linear recursive, iterative, fast.
: expt-rec ( b n -- b^n ) dup 0= if 2drop 1 else over swap 1- recurse * then ;
: expt-iter ( b n -- b^n ) 1 swap 0 ?do over * loop nip ;
: square ( n -- n^2 ) dup * ;
: even? ( n -- f ) 2 mod 0= ;
: fast-expt ( b n -- b^n )
  dup 0= if 2drop 1 exit then
  dup even? if 2/ recurse square exit then
  over swap 1- recurse * ;
: main ( -- )
  11 0 do i . 2 i expt-rec . 3 i expt-iter . 7 i fast-expt . cr loop ;
main
