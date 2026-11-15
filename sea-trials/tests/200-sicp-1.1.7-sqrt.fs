\ requires: core float float-ext
\ layer: x
\ SICP 1.1.7 Newton's method square roots. Results are printed scaled by 10^6.
: f.6 ( F: r -- ) 1e6 f* fround f>s . ;
: improve ( F: g x -- g' x ) fswap fover fover f/ f+ 2e f/ fswap ;
: good-enough? ( F: g x -- g x ) ( -- f ) fover fdup f* fover f- fabs 1e-3 f< ;
: sqrt-iter ( F: g x -- r ) begin good-enough? 0= while improve repeat fdrop ;
: sqrt ( F: x -- r ) 1e fswap sqrt-iter ;
: main ( -- ) 10 1 do i s>f sqrt f.6 cr loop ;
main
