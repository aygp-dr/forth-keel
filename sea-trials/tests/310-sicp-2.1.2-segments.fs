\ requires: core float float-ext memory exception
\ layer: x
\ SICP 2.1.2 points and segments (exercise 2.2), points heap-allocated.
: f.6 ( F: r -- ) 1e6 f* fround f>s . ;
: make-point ( F: x y -- ) ( -- p )
  2 floats allocate throw dup float+ f! dup f! ;
: x-point ( p -- ) ( F: -- x ) f@ ;
: y-point ( p -- ) ( F: -- y ) float+ f@ ;
: .point ( p -- ) ." ( " dup x-point f.6 ." , " y-point f.6 ." )" ;
: midpoint ( p1 p2 -- p )
  over x-point dup x-point f+ 2e f/
  y-point y-point f+ 2e f/ make-point ;
: main ( -- )
  1.5e 4e make-point 3e 7e make-point
  2dup ." start: " drop .point cr
  ." end: " dup .point cr
  ." midpoint: " midpoint .point cr ;
main
