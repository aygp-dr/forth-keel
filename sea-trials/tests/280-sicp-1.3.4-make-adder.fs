\ requires: core
\ layer: 0
\ SICP 1.3.4 procedures as returned values, modeled with create does>.
: make-adder ( n "name" -- ) create , does> ( x -- x+n ) @ + ;
5 make-adder add5
100 make-adder add100
: compose ( x xt1 xt2 -- y ) >r execute r> execute ;
: main ( -- )
  ." add5: " 10 add5 . cr
  ." add100: " 10 add100 . cr
  ." compose: " 1 ['] add5 ['] add100 compose . cr ;
main
