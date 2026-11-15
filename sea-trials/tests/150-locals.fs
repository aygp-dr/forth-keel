\ requires: core gforth-locals
\ layer: x
\ Gforth { } locals; Forth-2012 systems use {: :} instead.
: add3 { a b c -- n } a b + c + ;
." locals: " 1 2 3 add3 . cr
: cross { a b c d -- n } a d * b c * - ;
." cross: " 1 2 3 4 cross . cr
