\ Negative test: a frames definition that uses a word outside keel-wl and frames-wl must not compile.
include ../src/frames.fs
keel-wl frames-wl 2 set-order frames-wl set-current
: uses-star ( a b -- n ) * ;
