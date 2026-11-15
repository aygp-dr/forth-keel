\ requires: core core-ext
\ layer: 0
\ SICP 2.1.1 rational numbers as two cells ( n d ), reduced by gcd.
: gcd ( a b -- g ) begin ?dup while tuck mod repeat abs ;
: make-rat ( n d -- n d )
  dup 0< if negate swap negate swap then 2dup gcd tuck / >r / r> ;
: numer ( n d -- n ) drop ;
: denom ( n d -- d ) nip ;
: add-rat ( a b c d -- n d ) rot 2dup * >r rot * rot rot * + r> make-rat ;
: sub-rat ( a b c d -- n d ) >r negate r> add-rat ;
: mul-rat ( a b c d -- n d ) rot * >r * r> make-rat ;
: div-rat ( a b c d -- n d ) swap mul-rat ;
: equal-rat? ( a b c d -- f ) >r * swap r> * = ;
: print-rat ( n d -- ) swap 0 .r ." /" 0 .r cr ;
: one-half ( -- n d ) 1 2 ;
: one-third ( -- n d ) 1 3 ;
: pi-rat ( -- n d ) 22 7 ;
: area ( n d -- n d ) 2dup mul-rat pi-rat mul-rat ;
: main ( -- )
  one-half print-rat
  one-half one-third add-rat print-rat
  one-half one-third mul-rat print-rat
  one-third one-third add-rat print-rat
  one-half one-third sub-rat print-rat
  one-half one-third div-rat print-rat
  2 -4 make-rat print-rat
  one-half 2 4 equal-rat? . cr
  one-half numer . one-half denom . cr
  10 1 area print-rat ;
main
