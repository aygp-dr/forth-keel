#!/usr/bin/env python3
"""replay.py PUZZLES.fs SUBMISSION.csv -- second, independent rule check.

Written from the Data page's rules without reference to elf.fs, so that a
mistake would have to be made twice to pass. Reads the puzzles from the
solver-input file and the programs from the submission CSV (as the
competition would), and prints "TOTAL <instructions> <puzzles>" or exits 1
on the first illegal program.
"""
import csv
import json
import sys


def puzzles(path):
    out, cur = {}, None
    for line in open(path):
        w = line.split()
        if not w:
            continue
        if w[0] == "puzzle":
            cur = out[w[1]] = {"limit": int(w[3])}
        elif w[0] == "targets:":
            cur["targets"] = [int(x) for x in w[1:-1]]
        elif w[0] == "start:":
            cur["start"] = [int(x) for x in w[1:-1]]
    return out


def check(p, prog):
    if len(prog) > 4096:
        return "more than 4096 instructions"
    reg = dict(enumerate(p["start"]))
    seen = set(reg.values())
    for n, (op, i, j, d) in enumerate(prog, 1):
        if op not in "+-*/" or not all(0 <= r < 48 for r in (i, j, d)):
            return f"step {n}: bad instruction"
        if i not in reg or j not in reg:
            return f"step {n}: undefined source"
        a, b = reg[i], reg[j]
        if op == "+":
            v = a + b
        elif op == "-":
            if a <= b:
                return f"step {n}: {a} - {b} not positive"
            v = a - b
        elif op == "*":
            v = a * b
        else:
            if a % b:
                return f"step {n}: {a} / {b} inexact"
            v = a // b
        if v > p["limit"]:
            return f"step {n}: {v} over limit"
        reg[d] = v
        seen.add(v)
    missing = set(p["targets"]) - seen
    return f"{len(missing)} targets never produced" if missing else None


def main():
    ps = puzzles(sys.argv[1])
    total = 0
    rows = list(csv.DictReader(open(sys.argv[2], newline="")))
    if sorted(r["id"] for r in rows) != sorted(ps):
        sys.exit("submission ids do not match the puzzles")
    for row in rows:
        prog = json.loads(row["program"])
        err = check(ps[row["id"]], prog)
        if err:
            print(f"FAIL {row['id']}: {err}")
            sys.exit(1)
        total += len(prog)
    print(f"TOTAL {total} {len(rows)}")


if __name__ == "__main__":
    main()
