#!/usr/bin/env python3
"""csv2fs.py PUZZLES.csv OUT.fs -- competition puzzles to the solver's input.

Reads puzzles.csv or practice_puzzles.csv (id, starting_values, targets,
value_limit, registers[, family, design_tier, lower_bound]). Rejects any
puzzle whose register count is not 48, since the solver assumes it.
"""
import csv
import json
import sys


def main():
    src, out = sys.argv[1], sys.argv[2]
    with open(src, newline="") as f, open(out, "w") as o:
        for row in csv.DictReader(f):
            if int(row["registers"]) != 48:
                sys.exit(f"{row['id']}: {row['registers']} registers, expected 48")
            start = json.loads(row["starting_values"])
            targets = json.loads(row["targets"])
            fam = row.get("family") or "scored"
            o.write(f"\\ family {fam}\n")
            o.write(f"puzzle {row['id']}  limit {row['value_limit']}\n")
            o.write("targets: " + " ".join(map(str, targets)) + " ;\n")
            o.write("start: " + " ".join(map(str, start)) + " ;\nsolve\n")


if __name__ == "__main__":
    main()
