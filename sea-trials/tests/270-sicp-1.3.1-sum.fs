\ requires: core
\ layer: 0
\ SICP 1.3.1 procedures as arguments: sum with term and next.
variable 'term
variable 'next
: (sum) ( a b -- n )
  2dup > if 2drop 0 exit then
  over 'term @ execute >r swap 'next @ execute swap recurse r> + ;
: sum ( a b term next -- n ) 'next ! 'term ! (sum) ;
: identity ( x -- x ) ;
: cube ( x -- x^3 ) dup dup * * ;
: inc2 ( x -- x+2 ) 2 + ;
: main ( -- )
  ." sum-integers 1..10: " 1 10 ['] identity ['] 1+ sum . cr
  ." sum-cubes 1..10: " 1 10 ['] cube ['] 1+ sum . cr
  ." sum odd 1..19: " 1 19 ['] identity ['] inc2 sum . cr ;
main
