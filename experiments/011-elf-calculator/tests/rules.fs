\ One puzzle per rule. Each program is the worked example with exactly one
\ instruction made illegal, so the only way a line can pass is if the
\ replayer ignores that rule. Expected codes are in rules.out.
puzzle exact          limit 9  targets: 1 2 5 8 ;  start: 3 ;
0 0 1 r/  0 1 2 r-  0 2 3 r+  0 3 4 r+  done
puzzle register-48    limit 9  targets: 1 2 5 8 ;  start: 3 ;
0 0 48 r/  0 1 2 r-  0 2 3 r+  0 3 4 r+  done
puzzle undefined      limit 9  targets: 1 2 5 8 ;  start: 3 ;
0 5 1 r/  0 1 2 r-  0 2 3 r+  0 3 4 r+  done
puzzle subtract-eq    limit 9  targets: 1 2 5 8 ;  start: 3 ;
0 0 1 r/  0 0 2 r-  0 2 3 r+  0 3 4 r+  done
puzzle subtract-neg   limit 9  targets: 1 2 5 8 ;  start: 3 ;
0 0 1 r/  1 0 2 r-  0 2 3 r+  0 3 4 r+  done
puzzle inexact        limit 9  targets: 1 2 5 8 ;  start: 3 ;
0 0 1 r/  0 1 2 r-  0 2 3 r/  0 3 4 r+  done
puzzle over-limit     limit 7  targets: 1 2 5 ;  start: 3 ;
0 0 1 r/  0 1 2 r-  0 2 3 r+  0 3 4 r+  done
puzzle mul-wrap       limit 1000000000000000000  targets: 4294967296 ;  start: 4294967296 ;
0 0 1 r*  done
puzzle uncovered      limit 9  targets: 1 2 5 8 ;  start: 3 ;
0 0 1 r/  0 1 2 r-  0 2 3 r+  done
puzzle start-covers   limit 9  targets: 3 6 ;  start: 3 ;
0 0 1 r+  done
puzzle dest-is-source limit 9  targets: 6 ;  start: 3 ;
0 0 0 r+  done
puzzle at-limit       limit 1000000000000000000  targets: 1000000000000000000 ;  start: 1000000000 ;
0 0 1 r*  done
