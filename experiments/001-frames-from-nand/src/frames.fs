\ Layer 0 data words defined from keel primitives only.
\ Host (gforth) supplies : ; ( \ and number parsing; nand is emulated as and invert.
\ While frames are compiled, the search order holds only frames-wl and keel-wl.
wordlist constant keel-wl
wordlist constant frames-wl
: end-frames ( -- ) only forth definitions ;
keel-wl set-current
' @ alias @
' ! alias !
' sp@ alias sp@
' rp@ alias rp@
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
: dup ( x -- x x ) sp@ @ ;
: invert ( x -- ~x ) dup nand ;
: and ( x y -- x&y ) nand invert ;
: negate ( n -- -n ) invert 1 + ;
: - ( a b -- a-b ) negate + ;
: cell ( -- n ) sp@ sp@ - ;
: drop ( x -- ) dup - + ;
: over ( a b -- a b a ) sp@ cell + @ ;
: swap ( a b -- b a ) over over sp@ cell cell + cell + + ! sp@ cell + ! ;
: or ( x y -- x|y ) invert swap invert and invert ;
: nip ( a b -- b ) swap drop ;
: tuck ( a b -- b a b ) swap over ;
: 2dup ( a b -- a b a b ) over over ;
: 2drop ( a b -- ) drop drop ;
: = ( a b -- f ) - 0= ;
: <> ( a b -- f ) = invert ;
end-frames
