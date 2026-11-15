\ requires: core memory exception
\ layer: x
3 cells allocate throw constant p
11 p ! 22 p cell+ ! 33 p 2 cells + !
." allocate: " p @ . p cell+ @ . p 2 cells + @ . cr
p 5 cells resize throw constant q
." resize keeps: " q 2 cells + @ . cr
." free: " q free . cr
." allocate ior: " 1 cells allocate . free . cr
