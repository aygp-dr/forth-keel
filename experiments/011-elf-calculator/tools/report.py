#!/usr/bin/env python3
"""report.py PUZZLES.fs NAME=RESULTS.txt ... -- per-puzzle JSON for the dashboard.

PUZZLES.fs supplies each puzzle's family. Each RESULTS.txt is the verifier's
RESULT lines for one solver variant. Prints JSON on stdout.
"""
import json
import sys


def main():
    family, order = {}, []
    fam = None
    for line in open(sys.argv[1]):
        if line.startswith("\\ family"):
            fam = line.split()[2]
        elif line.startswith("puzzle"):
            pid = line.split()[1]
            family[pid] = fam
            order.append(pid)
    puzzles = {p: {"id": p, "family": family[p]} for p in order}
    variants = []
    for arg in sys.argv[2:]:
        name, path = arg.split("=", 1)
        variants.append(name)
        for line in open(path):
            w = line.split()
            if not w or w[0] != "RESULT":
                continue
            p = puzzles[w[1]]
            p[name] = {"ok": w[2] == "ok", "code": int(w[3]), "steps": int(w[4])}
            p["targets"], p["lower_bound"] = int(w[6]), int(w[7])
    json.dump({"variants": variants, "puzzles": list(puzzles.values())}, sys.stdout, indent=1)


if __name__ == "__main__":
    main()
