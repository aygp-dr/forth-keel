#!/usr/bin/env python3
"""docs gate: python3 gates/docs_gate.py [ROOT]

Prints one line per finding as "docs-gate: FAIL <check>: <detail>" and exits 1
on any finding. Prints "docs-gate: PASS (<n> checks)" and exits 0 otherwise.
There is no warning tier.

Checks
  spec-sections   spec.org has every required top-level heading
  links           relative markdown links and org file: links resolve; [[*H]] names a heading
  falsification   every experiments/*/CONJECTURE.md has a Falsification heading with list items
  labels          list items under Hypothesis / Falsification / Evidence headings start with [E] [P] [H] [I]
  derived-header  every :tangle target except *.mmd exists and starts with the derived-file header
  req-refs        every R-X-NN cited outside spec.org is defined once in spec.org's requirement table
  mermaid         every Mermaid diagram parses (tools/mmcheck.mjs)
"""
import os
import re
import subprocess
import sys

REQUIRED_SECTIONS = ["Scope", "Working rules", "Survey and names", "Architecture", "Build sequence", "Requirements",
                     "Experiments", "Resources", "Gates", "Debug toolchain", "Open items", "Review ledger", "Derived files"]
SKIP_DIRS = {".git", "vendor", "node_modules", "fixtures"}
HEADER = "Derived from spec.org"
LABEL = re.compile(r"^\s*[-*] \[(E|P|H|I)\] ")
LABELLED = ("hypothesis", "falsification", "evidence")
REQ = re.compile(r"\bR-[A-Z]-\d\d\b")
TOOLS = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "tools")


def walk(root, exts):
    for d, dirs, files in os.walk(root):
        dirs[:] = sorted(x for x in dirs if x not in SKIP_DIRS)
        for f in sorted(files):
            if f.endswith(exts):
                yield os.path.join(d, f)


def read(p):
    with open(p, encoding="utf-8") as fh:
        return fh.read()


def check_spec_sections(root, spec):
    heads = set(re.findall(r"^\* (.+?)\s*$", spec, re.M))
    return [f"spec.org has no '* {s}' heading" for s in REQUIRED_SECTIONS if s not in heads]


def check_links(root, spec):
    out = []
    for p in walk(root, (".md", ".org")):
        text = read(p)
        base = os.path.dirname(p)
        if p.endswith(".md"):
            targets = re.findall(r"\]\(([^)\s]+)\)", text)
        else:
            targets = re.findall(r"\[\[file:([^\]]+)\]", text)
            for h in re.findall(r"\[\[\*([^\]]+)\]", text):
                if not re.search(r"^\*+ " + re.escape(h) + r"\s*$", text, re.M):
                    out.append(f"{os.path.relpath(p, root)}: [[*{h}]] names no heading")
        for t in targets:
            if re.match(r"^[a-z]+:", t) or t.startswith("#"):
                continue
            t = t.split("#")[0]
            if not os.path.exists(os.path.normpath(os.path.join(base, t))):
                out.append(f"{os.path.relpath(p, root)}: link target {t} does not exist")
    return out


def sections(text):
    """Yield (heading, body) for each markdown heading."""
    parts = re.split(r"^(#+ .*)$", text, flags=re.M)
    for i in range(1, len(parts), 2):
        yield parts[i].lstrip("#").strip(), parts[i + 1]


def experiment_files(root, name):
    d = os.path.join(root, "experiments")
    if not os.path.isdir(d):
        return []
    return [os.path.join(d, e, name) for e in sorted(os.listdir(d))
            if re.match(r"\d{3}-", e) and os.path.isfile(os.path.join(d, e, name))]


def check_falsification(root, spec):
    out = []
    d = os.path.join(root, "experiments")
    exps = [e for e in sorted(os.listdir(d)) if re.match(r"\d{3}-", e)] if os.path.isdir(d) else []
    for e in exps:
        p = os.path.join(d, e, "CONJECTURE.md")
        if not os.path.isfile(p):
            out.append(f"experiments/{e}: no CONJECTURE.md")
            continue
        found = [b for h, b in sections(read(p)) if "falsification" in h.lower()]
        if not found or not re.search(r"^\s*[-*] ", found[0], re.M):
            out.append(f"experiments/{e}/CONJECTURE.md: no falsification criteria")
    return out


def check_labels(root, spec):
    out = []
    for p in experiment_files(root, "CONJECTURE.md") + experiment_files(root, "REFUTATION.md"):
        for h, body in sections(read(p)):
            if not any(k in h.lower() for k in LABELLED):
                continue
            for line in body.splitlines():
                if re.match(r"^\s*[-*] ", line) and not LABEL.match(line):
                    out.append(f"{os.path.relpath(p, root)}: unlabelled item under '{h}': {line.strip()[:60]}")
    return out


def check_derived_header(root, spec):
    out = []
    for t in re.findall(r":tangle (\S+)", spec):
        if t == "no" or t.endswith(".mmd"):
            continue
        p = os.path.join(root, t)
        if not os.path.isfile(p):
            out.append(f"{t}: tangle target missing")
            continue
        head = "".join(read(p).splitlines(True)[:2])
        if HEADER not in head:
            out.append(f"{t}: first two lines lack '{HEADER}'")
    return out


def check_req_refs(root, spec):
    out = []
    defined = re.findall(r"^\|\s*(R-[A-Z]-\d\d)\s*\|", spec, re.M)
    for r in sorted({r for r in defined if defined.count(r) > 1}):
        out.append(f"spec.org defines {r} more than once")
    for p in walk(root, (".md", ".org")):
        if os.path.abspath(p) == os.path.abspath(os.path.join(root, "spec.org")):
            continue
        for r in sorted(set(REQ.findall(read(p)))):
            if r not in defined:
                out.append(f"{os.path.relpath(p, root)} cites {r}, which spec.org does not define")
    return out


def check_mermaid(root, spec):
    files = list(walk(root, (".mmd", ".md", ".org")))
    script = os.path.join(TOOLS, "mmcheck.mjs")
    if not os.path.isdir(os.path.join(TOOLS, "node_modules", "mermaid")):
        return ["tools/node_modules/mermaid missing; run 'make tools'"]
    r = subprocess.run(["node", script, *files], capture_output=True, text=True, cwd=TOOLS)
    if r.returncode == 0:
        return []
    fails = [l for l in r.stdout.splitlines() if l.startswith("mermaid: FAIL")]
    return [l.replace("mermaid: FAIL ", "").replace(os.path.abspath(root) + "/", "") for l in fails] or [r.stdout.strip() or r.stderr.strip()]


CHECKS = [("spec-sections", check_spec_sections), ("links", check_links), ("falsification", check_falsification),
          ("labels", check_labels), ("derived-header", check_derived_header), ("req-refs", check_req_refs),
          ("mermaid", check_mermaid)]


def main():
    root = os.path.abspath(sys.argv[1] if len(sys.argv) > 1 else ".")
    spec_path = os.path.join(root, "spec.org")
    if not os.path.isfile(spec_path):
        print("docs-gate: FAIL spec-sections: spec.org missing")
        return 1
    spec = read(spec_path)
    n = 0
    for name, fn in CHECKS:
        for finding in fn(root, spec):
            print(f"docs-gate: FAIL {name}: {finding}")
            n += 1
    if n:
        print(f"docs-gate: {n} findings")
        return 1
    print(f"docs-gate: PASS ({len(CHECKS)} checks)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
