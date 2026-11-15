\ gforth as a host: expose exactly the six keel primitives frames may use,
\ plus : ; ( \ which the keel cannot provide until frames has a compiler.
\ nand is emulated as `and invert`, as in experiment 001.
\ The Makefile concatenates this, src/frames.fs and gforth-end.fs, so frames
\ never needs `include` and the search order can stay narrow throughout.
wordlist constant keel-wl
wordlist constant frames-wl
: end-frames ( -- ) only forth definitions ;
keel-wl set-current
' @ alias @
' ! alias !
' sp@ alias sp@
' 0= alias 0=
' + alias +
: nand ( x y -- z ) and invert ;
' : alias :
\ ; is compile-only in gforth, so wrap it instead of aliasing it
: ; postpone ; ; immediate
' ( alias ( immediate
' \ alias \ immediate
' end-frames alias end-frames
frames-wl set-current
keel-wl frames-wl 2 set-order
