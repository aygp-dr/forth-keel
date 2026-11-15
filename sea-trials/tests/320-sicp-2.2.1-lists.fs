\ requires: core core-ext
\ layer: 0
\ SICP 2.2.1 lists of cons cells in a fixed pool; null is 0.
8192 constant pool-cells
create pool pool-cells cells allot
variable hp
pool hp !
0 constant null
: null? ( list -- f ) 0= ;
: cons ( car cdr -- pair ) hp @ >r swap r@ ! r@ cell+ ! r@ 2 cells + hp ! r> ;
: car ( pair -- car ) @ ;
: cdr ( pair -- cdr ) cell+ @ ;
: l-append ( l1 l2 -- l3 )
  over null? if nip exit then
  >r dup car swap cdr r> recurse cons ;
: l-length ( list -- n )
  0 swap begin dup null? 0= while swap 1+ swap cdr repeat drop ;
: l-ref ( list n -- x ) 0 ?do cdr loop car ;
: l-map ( list xt -- list' )
  over null? if drop exit then
  >r dup car r@ execute swap cdr r> recurse cons ;
: l-reverse ( list -- list' )
  null swap begin dup null? 0= while dup car rot cons swap cdr repeat drop ;
: .list ( list -- )
  ." ( " begin dup null? 0= while dup car . cdr repeat drop ." )" ;
: square ( n -- n^2 ) dup * ;
variable la
variable lb
: main ( -- )
  1 2 3 null cons cons cons la !
  4 5 null cons cons lb !
  ." la: " la @ .list cr
  ." lb: " lb @ .list cr
  ." car cdr: " la @ car . la @ cdr .list cr
  ." append: " la @ lb @ l-append .list cr
  ." length: " la @ l-length . null l-length . cr
  ." l-ref 2: " la @ 2 l-ref . cr
  ." map square: " la @ ['] square l-map .list cr
  ." reverse: " la @ lb @ l-append l-reverse .list cr
  ." la unchanged: " la @ .list cr ;
main
