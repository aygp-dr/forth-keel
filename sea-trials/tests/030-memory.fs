\ requires: core core-ext
\ layer: 0
variable v
5 v !
." ! @: " v @ . cr
3 v +!
." +!: " v @ . cr
42 constant answer
." constant: " answer . cr
10 value x
." value: " x . 20 to x x . cr
create arr 1 , 2 , 3 ,
." create ,: " arr @ . arr cell+ @ . arr 2 cells + @ . cr
." allot: " here 3 cells allot here swap - 1 cells / . cr
create b 4 allot
65 b c!
." c! c@: " b c@ . cr
b 4 char z fill
." fill type: " b 4 type cr
create dst 3 cells allot
arr dst 3 cells move
." move: " dst 2 cells + @ . cr
