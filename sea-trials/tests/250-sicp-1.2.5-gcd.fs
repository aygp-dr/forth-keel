\ requires: core core-ext
\ layer: 0
\ SICP 1.2.5 Euclid's algorithm.
: gcd ( a b -- g ) dup 0= if drop exit then tuck mod recurse ;
: main ( -- ) 206 40 gcd . 40 206 gcd . 17 5 gcd . 48 18 gcd . 7 0 gcd . cr ;
main
