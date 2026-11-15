\ Tagged cells: fixnum round trip, predicates, pairs, a 100-element list.
include ../src/cells.fs
variable fails
variable checks
: check ( f -- ) 0= if 1 fails +! then 1 checks +! ;
1 30 lshift 1- constant fix-max
fix-max negate 1- constant fix-min
create fixes 0 , 1 , -1 , 2 , 1000 , -1000 , fix-max , fix-min ,
8 constant #fixes
: fix@ ( i -- n ) cells fixes + @ ;
: roundtrip ( -- ) #fixes 0 do i fix@ >fix fix> i fix@ = check loop ;
: predicates ( -- )
  #fixes 0 do i fix@ >fix dup fix? check dup obj? 0= check pair? 0= check loop ;
: pairs ( -- )
  #fixes 0 do
    i fix@ >fix 7 >fix cons
    dup obj? check dup fix? 0= check dup pair? check
    dup car fix> i fix@ = check cdr fix> 7 = check
  loop ;
: nested ( -- ) 1 >fix 2 >fix cons 3 >fix cons dup car car fix> 1 = check dup car cdr fix> 2 = check cdr fix> 3 = check ;
: iota ( n -- list ) null swap begin dup 0> while tuck >fix swap cons swap 1- repeat drop ;
: sum-list ( list -- n ) 0 swap begin dup null? 0= while dup car fix> rot + swap cdr repeat drop ;
: len ( list -- n ) 0 swap begin dup null? 0= while swap 1+ swap cdr repeat drop ;
: lists ( -- ) 100 iota dup sum-list 5050 = check dup len 100 = check car fix> 1 = check ;
roundtrip predicates pairs nested lists
checks @ . ." checks, " fails @ . ." failures" cr
fails @ 0<> 1 and (bye)
