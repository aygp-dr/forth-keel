\ requires: core exception
\ layer: x
: risky ( -- ) 7 throw ;
: safe ( -- n ) 42 ;
." catch throw: " ' risky catch . cr
." catch ok: " ' safe catch . . cr
: nested ( -- ) ['] risky catch 100 + throw ;
." rethrow: " ' nested catch . cr
." throw 0: " 0 throw ." continued" cr
