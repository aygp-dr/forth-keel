\ requires: core core-ext
\ layer: 0
: signum ( n -- s ) dup 0< if drop -1 else 0> if 1 else 0 then then ;
." signum: " -5 signum . 0 signum . 7 signum . cr
: classify ( n -- )
  dup 10 < if ." small " drop exit then
  dup 100 < if ." medium " drop exit then
  drop ." large " ;
." classify: " 5 classify 50 classify 500 classify cr
: abs' ( n -- |n| ) dup 0< if negate then ;
." if-then: " -9 abs' . 9 abs' . cr
