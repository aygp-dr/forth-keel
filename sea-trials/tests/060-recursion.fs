\ requires: core core-ext
\ layer: 0
: fact ( n -- n! ) dup 2 < if drop 1 else dup 1- recurse * then ;
." recurse: " 0 fact . 5 fact . 10 fact . cr
: ack ( m n -- a )
  over 0= if nip 1+ exit then
  dup 0= if drop 1- 1 recurse exit then
  over swap 1- recurse swap 1- swap recurse ;
." ackermann: " 0 0 ack . 1 2 ack . 2 3 ack . cr
defer odd?
: even? ( n -- f ) dup 0= if drop true else 1- odd? then ;
:noname ( n -- f ) dup 0= if drop false else 1- even? then ; is odd?
." defer is: " 10 even? . 7 even? . 7 odd? . cr
