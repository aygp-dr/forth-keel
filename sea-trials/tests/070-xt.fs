\ requires: core core-ext
\ layer: 0
." ' execute: " 3 4 ' + execute . cr
: apply-twice ( x xt -- y ) dup >r execute r> execute ;
." apply-twice: " 3 ' 1+ apply-twice . 3 ' 2* apply-twice . cr
:noname ( n -- n*n ) dup * ; constant sq
." :noname: " 5 sq execute . cr
: sq-of ( n -- n*n ) ['] dup execute * ;
." [']: " 7 sq-of . cr
create ops ' + , ' - , ' * ,
: op ( a b i -- n ) cells ops + @ execute ;
." dispatch: " 6 3 0 op . 6 3 1 op . 6 3 2 op . cr
