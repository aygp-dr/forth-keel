\ requires: core core-ext
\ layer: 0
." dup: " 1 dup . . cr
." drop: " 1 2 drop . cr
." swap: " 1 2 swap . . cr
." over: " 1 2 over . . . cr
." rot: " 1 2 3 rot . . . cr
." nip: " 1 2 nip . depth . cr
." tuck: " 1 2 tuck . . . cr
." ?dup 0: " 0 ?dup depth . drop cr
." ?dup 5: " 5 ?dup . . cr
." 2dup: " 1 2 2dup . . . . cr
." 2drop: " 1 2 3 2drop . cr
." 2swap: " 1 2 3 4 2swap . . . . cr
." 2over: " 1 2 3 4 2over . . . . . . cr
." pick: " 10 20 30 2 pick . . . . cr
." roll: " 10 20 30 2 roll . . . cr
." depth: " depth . 1 2 3 depth . 2drop drop cr
