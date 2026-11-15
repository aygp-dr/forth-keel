\ requires: core core-ext
\ layer: 0
." true false: " true . false . cr
." and or xor: " 12 10 and . 12 10 or . 12 10 xor . cr
." invert: " 0 invert . cr
." = <>: " 3 3 = . 3 4 = . 3 4 <> . cr
." < >: " 3 4 < . 3 4 > . -1 0 < . cr
." u<: " 3 4 u< . -1 1 u< . cr
." 0= 0< 0<> 0>: " 0 0= . 5 0= . -5 0< . 5 0<> . 5 0> . cr
." lshift rshift: " 1 4 lshift . 256 4 rshift . cr
." within: " 5 1 10 within . 10 1 10 within . cr
