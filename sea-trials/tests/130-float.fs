\ requires: core float float-ext memory exception
\ layer: x
\ floats are printed as rounded integers scaled by 10^6, so the oracle
\ does not depend on how an implementation formats f.
: f.6 ( F: r -- ) 1e6 f* fround f>s . ;
." f+: " 1.5e 2.5e f+ f.6 cr
." f- f*: " 5e 3e f- f.6 2e 3.5e f* f.6 cr
." f/: " 1e 3e f/ f.6 cr
." fsqrt: " 2e fsqrt f.6 cr
." fabs fnegate: " -2.5e fabs f.6 2.5e fnegate f.6 cr
." s>f f>s: " 7 s>f f.6 7.9e f>s . cr
." fdup fswap fover: " 1e 2e fswap f.6 f.6 1e fdup f* f.6 1e 2e fover f.6 f.6 f.6 cr
." f< f0=: " 1e 2e f< . 2e 1e f< . 0e f0= . cr
1 floats allocate throw constant fcell
3.25e fcell f!
." f! f@: " fcell f@ f.6 cr
fcell free throw
