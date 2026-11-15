# Experiments

Index of numbered experiments. Each directory is self-contained: `make test`
passes inside it with no sibling dependencies. Numbers are never reused and
directories are never deleted. The authoritative index with requirement IDs
is the Experiments section of `../spec.org`.

| # | Name | Requirement | Status | Result |
|---|------|-------------|--------|--------|
| 000 | [quickstart](000-quickstart/) | R-H-01 | proof passes | gforth runs a program and the output check rejects a wrong expectation |
| 001 | [frames-from-nand](001-frames-from-nand/) | R-F-01 | proof passes | 16 data words defined from `@ ! sp@ 0= + nand` match gforth on 9 values per argument |
| 002 | [tagged-cells](002-tagged-cells/) | R-P-01, R-P-02 | proof passes | fixnum/object low-bit tagging and pairs round-trip |
| 008 | [frames-on-keel](008-frames-on-keel/) | R-F-01, R-K-02 | proof passes | the same literal-free frames source runs on gforth and on the Rust keel, 432 checks each |
| 009 | [control-from-rp](009-control-from-rp/) | R-F-02 | proof passes | `lit branch ?branch >r r> rot` derived from `rp@` on the keel, with no `rp!` |
| 010 | [compile-from-the-map](010-compile-from-the-map/) | R-F-02, R-K-04 | proof passes | `here` and `,` derived from the memory map, and frames compiles a word at run time and executes it |
