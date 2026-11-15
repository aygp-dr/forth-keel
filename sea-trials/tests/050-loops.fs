\ requires: core core-ext
\ layer: 0
: count-down ( n -- ) begin dup . 1- dup 0= until drop ;
." until: " 5 count-down cr
: halve ( n -- ) begin dup 0> while dup . 2/ repeat drop ;
." while: " 40 halve cr
: squares ( n -- ) 0 do i i * . loop ;
." do loop: " 5 squares cr
: evens ( -- ) 10 0 do i . 2 +loop ;
." +loop: " evens cr
: down ( -- ) 0 10 do i . -2 +loop ;
." -loop: " down cr
: none ( -- ) 5 5 ?do i . loop ." none" ;
." ?do: " none cr
: grid ( -- ) 3 0 do 2 0 do j 10 * i + . loop loop ;
." i j: " grid cr
: upto5 ( -- ) 10 0 do i 5 = if leave then i . loop ;
." leave: " upto5 cr
: find3 ( -- n ) 10 0 do i 3 = if i unloop exit then loop -1 ;
." unloop exit: " find3 . cr
: day ( n -- )
  case
    0 of ." sun " endof
    6 of ." sat " endof
    ." weekday "
  endcase ;
." case: " 0 day 3 day 6 day cr
