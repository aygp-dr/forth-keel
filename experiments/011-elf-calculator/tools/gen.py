#!/usr/bin/env python3
"""gen.py SEED N OUT.fs -- synthetic puzzles in the solver's input format.

The scored puzzles sit behind the competition's rules, so until they are
downloaded the experiment runs on these. The family names are the Data
page's; the generators are guesses at what those families mean, not the
competition's generators. Each puzzle header carries its family in a
comment so report.py can group by it.
"""
import random
import sys

LIMIT = 10**18


def tiny(r):
    start = [r.randint(2, 9)]
    targets = r.sample(range(1, 200), r.randint(3, 6))
    return start, targets, 1000


def independent(r):
    start = [r.randint(2, 20) for _ in range(r.randint(1, 3))]
    top = 10 ** r.randint(6, 18)
    return start, [r.randint(2, top) for _ in range(r.randint(5, 30))], LIMIT


def shared(r):
    start = [r.randint(2, 20) for _ in range(r.randint(1, 3))]
    base = r.randint(10**9, 10**15)
    step = r.randint(2, 10**6)
    return start, [base + k * step + r.randint(0, 3) for k in range(r.randint(10, 40))], LIMIT


def multiplicative(r):
    start = r.sample([2, 3, 5, 7, 11, 13], r.randint(2, 4))
    targets = set()
    while len(targets) < r.randint(5, 25):
        v = 1
        while True:
            f = r.choice(start)
            if v * f > 10**15:
                break
            v *= f
            if r.random() < 0.15:
                break
        targets.add(v)
    return start, sorted(targets), LIMIT


def mixed(r):
    s1, t1, _ = independent(r)
    s2, t2, _ = multiplicative(r)
    return s1 + s2, t1 + t2, LIMIT


FAMILIES = [tiny, independent, shared, multiplicative, mixed]


def main():
    seed, n, out = int(sys.argv[1]), int(sys.argv[2]), sys.argv[3]
    r = random.Random(seed)
    with open(out, "w") as f:
        for k in range(1, n + 1):
            fam = FAMILIES[(k - 1) % len(FAMILIES)]
            start, targets, limit = fam(r)
            targets = sorted(set(t for t in targets if 1 <= t <= limit))
            f.write(f"\\ family {fam.__name__}\n")
            f.write(f"puzzle synth_{k:04d}  limit {limit}\n")
            f.write("targets: " + " ".join(map(str, targets)) + " ;\n")
            f.write("start: " + " ".join(map(str, start)) + " ;\nsolve\n")


if __name__ == "__main__":
    main()
