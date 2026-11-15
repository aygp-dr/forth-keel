\ requires: core core-ext
\ layer: 0
: r-test ( -- n ) 5 >r r@ r@ + r> + ;
." >r r@ r>: " r-test . cr
: 2r-test ( -- ) 1 2 2>r 3 2r> . . . ;
." 2>r 2r>: " 2r-test cr
: hide ( a b -- b ) >r drop r> ;
." hide: " 1 2 hide . cr
