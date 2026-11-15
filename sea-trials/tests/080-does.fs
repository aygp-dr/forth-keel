\ requires: core
\ layer: 0
: my-const ( n "name" -- ) create , does> ( -- n ) @ ;
42 my-const answer
." does my-const: " answer . cr
: counter ( "name" -- ) create 0 , does> ( -- n ) dup 1 swap +! @ ;
counter c1
counter c2
." does counter: " c1 . c1 . c2 . c1 . cr
: array ( n "name" -- ) create cells allot does> ( i -- addr ) swap cells + ;
5 array a
10 2 a !
20 4 a !
." does array: " 2 a @ . 4 a @ . cr
