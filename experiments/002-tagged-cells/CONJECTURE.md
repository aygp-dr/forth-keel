# 002 tagged-cells

## Hypothesis

- [H] Representing a fixnum as n*2 (low bit 0) and an object reference as address+1 (low bit 1, objects cell-aligned with a type header) lets planking distinguish fixnums from objects with one `and`, and round-trips every fixnum in the 31-bit signed range of a 32-bit cell.

## Motivation

R-P-01 and R-P-02. The Cheney collector (R-P-06) must tell pointers from integers in every cell; this representation decides how.

## Falsification criteria

- [H] Refuted if `>fix fix>` changes any of 0, 1, -1, 2, 1000, -1000, 2^30-1, -2^30.
- [H] Refuted if `fix?` and `obj?` are both true, or both false, for any fixnum or pair tested.
- [H] Refuted if `car` or `cdr` of a `cons` returns anything but its arguments, for flat and nested pairs.
- [H] Refuted if a 100-element list built with `cons` does not have length 100 and sum 5050.

## Prior art

- [P] SICP 5.3.1 represents typed pointers as a type tag plus an index.
- [P] Low-bit tagging of fixnums is the common Lisp and OCaml representation.

## Scope

In: representation, predicates, `cons car cdr pair? null null?` with a bump allocator on gforth. Out: symbols, closures, the collector, and running on a 32-bit host (the range limits are tested at their 32-bit values on 64-bit gforth).
