\ requires: core core-ext string
\ layer: x
." .q: " ." hello" cr
." type: " s" abc" type cr
." length: " s" abcde" nip . cr
." char: " char A . cr
: zed ( -- c ) [char] z ;
." [char]: " zed . cr
." emit: " 70 emit 111 emit 114 emit 116 emit 104 emit cr
." spaces: [" 3 spaces ." ]" cr
." .r: [" 42 5 .r ." ]" cr
." c@ string: " s" hello" drop c@ . cr
." compare: " s" abc" s" abc" compare . s" abc" s" abd" compare . s" b" s" a" compare . cr
