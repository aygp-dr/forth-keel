\ Layer 1 tagged cells: fixnum = n*2 (low bit 0); object = addr+1 (low bit 1).
\ Objects start with a header cell holding a type code.
1 constant t-pair
2 constant t-symbol
: >fix ( n -- v ) 2* ;
: fix> ( v -- n ) 2/ ;
: fix? ( v -- f ) 1 and 0= ;
: obj? ( v -- f ) 1 and 0<> ;
: >obj ( addr -- v ) 1+ ;
: obj> ( v -- addr ) 1- ;
: type-of ( v -- t ) dup obj? if obj> @ else drop 0 then ;
4096 constant heap-cells
create heap heap-cells cells allot
variable hp
heap hp !
: alloc ( n -- addr ) cells hp @ swap hp +! ;
0 >fix constant null
: cons ( car cdr -- v ) 3 alloc >r t-pair r@ ! r@ 2 cells + ! r@ cell+ ! r> >obj ;
: pair? ( v -- f ) type-of t-pair = ;
: car ( v -- x ) obj> cell+ @ ;
: cdr ( v -- x ) obj> 2 cells + @ ;
: null? ( v -- f ) null = ;
