\ requires: core core-ext
\ layer: 0
\ SICP 3.1.1 local state: make-withdraw with a private balance cell.
: make-withdraw ( balance "name" -- )
  create ,
  does> ( amount addr -- balance | -1 )
  2dup @ > if 2drop -1 exit then
  swap negate swap tuck +! @ ;
100 make-withdraw w1
100 make-withdraw w2
: main ( -- )
  ." w1: " 25 w1 . 25 w1 . 60 w1 . 15 w1 . cr
  ." w2 independent: " 70 w2 . 40 w2 . cr ;
main
