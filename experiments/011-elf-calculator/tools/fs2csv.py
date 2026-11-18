#!/usr/bin/env python3
"""fs2csv.py SOLVED.fs OUT.csv -- solver output to a submission CSV.

Each "i j d r+" becomes ["+", i, j, d]. The csv module does the quoting the
Evaluation page asks for.
"""
import csv
import json
import sys


def main():
    rows, pid, prog = [], None, []
    in_list = False
    for line in open(sys.argv[1]):
        words = line.split()
        if not words:
            continue
        if words[0] == "puzzle":
            pid, prog = words[1], []
            continue
        if words[0] in ("targets:", "start:", "\\"):
            continue
        for k, w in enumerate(words):
            if w == "done":
                rows.append((pid, json.dumps(prog, separators=(",", ":"))))
            elif w in ("r+", "r-", "r*", "r/"):
                i, j, d = (int(x) for x in words[k - 3:k])
                prog.append([w[1], i, j, d])
    with open(sys.argv[2], "w", newline="") as f:
        w = csv.writer(f)
        w.writerow(["id", "program"])
        w.writerows(rows)


if __name__ == "__main__":
    main()
