\ requires: core core-ext
\ layer: 0
\ SICP 1.2.2 counting change with coins 1 5 10 25 50.
create us 1 , 5 , 10 , 25 , 50 ,
: coin ( i -- d ) cells us + @ ;
: spend ( a i -- a-d i ) tuck coin - swap ;
: cc ( a i -- n )
  over 0= if 2drop 1 exit then
  over 0< over 0< or if 2drop 0 exit then
  2dup 1- recurse >r spend recurse r> + ;
: count-change ( a -- n ) 4 cc ;
: main ( -- )
  0 count-change . 10 count-change . 11 count-change . 100 count-change . cr ;
main
