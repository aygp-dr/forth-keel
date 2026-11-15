\ requires: core core-ext
\ layer: 0
\ SICP 1.2.6 smallest divisor, prime?, expmod, Fermat test on a Carmichael number.
: divides? ( a b -- f ) swap mod 0= ;
: find-divisor ( n d -- d' )
  2dup dup * < if drop exit then
  2dup swap divides? if nip exit then
  1+ recurse ;
: smallest-divisor ( n -- d ) 2 find-divisor ;
: prime? ( n -- f ) dup smallest-divisor = ;
: expmod ( base exp m -- r )
  over 0= if 2drop drop 1 exit then
  over 2 mod 0= if
    >r 2/ r@ recurse dup * r> mod
  else
    >r over swap 1- r@ recurse * r> mod
  then ;
: fermat-all? ( n -- f )
  true swap dup 2 ?do i over dup expmod i <> if nip false swap leave then loop drop ;
: main ( -- )
  ." smallest-divisor: " 199 smallest-divisor . 1999 smallest-divisor . 19999 smallest-divisor . cr
  ." primes < 50: " 50 2 do i prime? if i . then loop cr
  ." expmod: " 2 10 1000 expmod . 3 200 50 expmod . 2 560 561 expmod . cr
  ." 561 fermat-all?: " 561 fermat-all? . 561 prime? . cr
  ." 97 fermat-all?: " 97 fermat-all? . 97 prime? . cr ;
main
