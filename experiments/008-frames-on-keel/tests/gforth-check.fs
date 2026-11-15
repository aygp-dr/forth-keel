\ Host 1 of 2: gforth. Compare every frames word with the gforth word of the
\ same name, over 9 values per argument. Same 432 checks as experiment 001,
\ against a frames source that now uses no number literals.
9 constant #vals
create vals -1 , 0 , 1 , 2 , 7 , 255 , -256 , -1 1 rshift , -1 1 rshift invert ,
: v ( i -- n ) cells vals + @ ;
variable fails
variable checks
variable h1
: f' ( "name" -- xt ) parse-name frames-wl search-wordlist 0= abort" not in frames-wl" ;
: check ( f -- ) 0= if 1 fails +! then 1 checks +! ;
: unary ( xt-f xt-h -- ) #vals 0 do i v 2 pick execute i v 2 pick execute = check loop 2drop ;
: binary ( xt-f xt-h -- )
  #vals 0 do #vals 0 do i v j v 3 pick execute i v j v 3 pick execute = check loop loop 2drop ;
: fp ( x1 .. xn -- h ) 0 depth 1- 0 ?do 31 * + loop ;
: stackword ( xt-f xt-h -- ) >r >r 10 20 30 r> execute fp h1 ! 10 20 30 r> execute fp h1 @ = check ;
f' invert ' invert unary
f' negate ' negate unary
f' and ' and binary
f' or ' or binary
f' - ' - binary
f' = ' = binary
f' <> ' <> binary
f' dup ' dup stackword
f' drop ' drop stackword
f' swap ' swap stackword
f' over ' over stackword
f' nip ' nip stackword
f' tuck ' tuck stackword
f' 2dup ' 2dup stackword
f' 2drop ' 2drop stackword
f' cell execute 1 cells = check
checks @ . ." checks, " fails @ . ." failures" cr
\ The literal-free derivation, reported separately so the 432 above stay the
\ same set experiment 001 ran.
f' one constant one-xt
: check-one ( -- )
  one-xt execute 1 = if ." PASS one derives 1 without a literal" cr
  else ." FAIL one did not derive 1" cr 1 fails +! then ;
check-one
fails @ 0<> 1 and (bye)
