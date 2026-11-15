\ requires: core core-ext
\ layer: 0
\ SICP 1.2.2 tree recursion vs iteration.
: fib-iter ( n -- fib[n] ) 0 1 rot 0 ?do tuck + loop drop ;
: fib-rec ( n -- fib[n] )
  dup 2 < if exit then dup 1- recurse swap 2 - recurse + ;
: main ( -- ) 21 0 do i fib-iter . i fib-rec . cr loop ;
main
