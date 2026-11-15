#!/usr/bin/env python3
"""forthfmt: deterministic formatter and linter for Gforth source files.

Usage:
  forthfmt.py FILE...            print formatted source to stdout
  forthfmt.py -w FILE...         rewrite files in place
  forthfmt.py --check FILE...    exit 1 if any file would change
  forthfmt.py --width N ...      max width for one-line definitions (0 = never join)

Rules:
  - words are lowercased; strings and comments are left as written
  - tokens are separated by one space; trailing whitespace is removed
  - a colon definition with no "\\" comment that fits in --width goes on one line:
        : name ( stack -- effect ) body ;
  - otherwise the header goes on its own line and the body keeps the
    author's line breaks, indented 2 spaces per control-structure level
  - "\\" comments inside a multi-line definition are aligned to one column
  - a "( name ... -- ... )" comment line just before ": name" moves into the header
  - runs of blank lines collapse to one; the file ends with one newline
Lint warnings go to stderr as FILE:LINE: message.
"""
import re
import sys

OPEN = {"if", "begin", "do", "?do", "case", "of"}
MID = {"else", "while"}
CLOSE = {"then", "repeat", "until", "again", "loop", "+loop", "endof", "endcase"}
STRING_WORDS = {'."', 's"', 'c"', 'abort"', 's\\"', '.\\"'}


class Tok:
    __slots__ = ("kind", "text")

    def __init__(self, kind, text):
        self.kind, self.text = kind, text  # kind: word literal comment line string locals

    def render(self):
        return self.text.lower() if self.kind == "word" else self.text

    def is_word(self, *ws):
        return self.kind == "word" and self.text.lower() in ws


def tokenize(line, warn):
    toks, i, n = [], 0, len(line)
    while i < n:
        while i < n and line[i].isspace():
            i += 1
        if i >= n:
            break
        j = i
        while j < n and not line[j].isspace():
            j += 1
        w, lw = line[i:j], line[i:j].lower()
        if w == "\\":
            toks.append(Tok("line", line[i:].rstrip()))
            break
        if w in ("(", ".("):
            k = line.find(")", j)
            if k < 0:
                warn("unclosed ( comment")
                k = n - 1
            if w == ".(":  # printed text: keep it byte for byte
                toks.append(Tok("string", line[i:k + 1]))
            else:
                inner = " ".join(line[j:k].split())
                toks.append(Tok("comment", f"( {inner} )" if inner else "( )"))
            i = k + 1
            continue
        if lw in STRING_WORDS:
            k = line.find('"', j + 1)
            if k < 0:
                warn(f"unclosed {w} string")
                k = n - 1
            toks.append(Tok("string", lw + line[j:k + 1]))
            i = k + 1
            continue
        if w in ("{", "{:"):
            end = "}" if w == "{" else ":}"
            k = line.find(end, j)
            if k < 0:
                warn("unclosed locals")
                k = n - len(end)
            inner = " ".join(line[j:k].split())
            toks.append(Tok("locals", f"{w} {inner} {end}"))
            i = k + len(end)
            continue
        if toks and toks[-1].is_word("char", "[char]"):
            toks.append(Tok("literal", w))  # character literal: keep its case
        else:
            toks.append(Tok("word", w))
        i = j
    return toks


def is_stack_effect(t):
    return t.kind in ("comment", "locals") and "--" in t.text


def join(toks):
    return " ".join(t.render() for t in toks)


class Formatter:
    def __init__(self, path, width):
        self.path, self.width, self.warnings = path, width, []

    def warn(self, lineno, msg):
        self.warnings.append(f"{self.path}:{lineno}: {msg}")

    def parse(self, text):
        """Split source into forms: ('blank',), ('top', lineno, toks), ('def', lineno, [toks...])."""
        forms, cur, start = [], None, 0
        for lineno, raw in enumerate(text.splitlines(), 1):
            toks = tokenize(raw, lambda m, ln=lineno: self.warn(ln, m))
            if cur is not None:
                cur.append(toks)
                if any(t.is_word(";") for t in toks):
                    forms.append(("def", start, cur))
                    cur = None
                continue
            if not toks:
                forms.append(("blank",))
            elif toks[0].is_word(":"):
                if any(t.is_word(";") for t in toks):
                    forms.append(("def", lineno, [toks]))
                else:
                    cur, start = [toks], lineno
            else:
                forms.append(("top", lineno, toks))
        if cur is not None:
            self.warn(start, "definition has no closing ;")
            forms.append(("def", start, cur))
        return forms

    def format_def(self, lineno, lines, prev_comment):
        first = lines[0]
        name = first[1].render() if len(first) > 1 else "?"
        header, k = first[:2], 2
        while k < len(first) and first[k].kind in ("comment", "locals"):
            header.append(first[k])
            k += 1
        body = [first[k:]] + lines[1:]
        # drop ";" and anything after it on the last line
        last = body[-1]
        semi = next((i for i, t in enumerate(last) if t.is_word(";")), len(last))
        if semi + 1 < len(last):
            self.warn(lineno, f"{name}: text after ; was dropped")
        body[-1] = last[:semi]
        body = [b for b in body if b]

        used_prev = False
        if not any(is_stack_effect(t) for t in header):
            if prev_comment is not None and is_stack_effect(prev_comment):
                inner = prev_comment.text[1:-1].split()
                if inner and inner[0].lower() == name:
                    inner = inner[1:]
                header.insert(2, Tok("comment", "( " + " ".join(inner) + " )"))
                used_prev = True
            else:
                self.warn(lineno, f"{name}: no stack-effect comment")
        self.lint_body(lineno, name, header[2:] + [t for b in body for t in b])

        has_line_comment = any(t.kind == "line" for b in body for t in b)
        one = join(header) + (" " + join([t for b in body for t in b]) if body else "") + " ;"
        if not has_line_comment and 0 < self.width and len(one) <= self.width:
            return [one], used_prev

        out, depth = [join(header)], 0
        rows = []
        for b in body:
            lead = b[0]
            d = depth - 1 if lead.kind == "word" and lead.text.lower() in (CLOSE | MID) else depth
            for t in b:
                if t.kind == "word":
                    lw = t.text.lower()
                    if lw in OPEN:
                        depth += 1
                    elif lw in CLOSE:
                        depth -= 1
            depth = max(depth, 0)
            code = [t for t in b if t.kind != "line"]
            com = [t for t in b if t.kind == "line"]
            rows.append(["  " * (1 + max(d, 0)), code, com])
        if not rows:
            rows.append(["  ", [], []])
        rows[-1][1] = rows[-1][1] + [Tok("word", ";")]
        col = max((len(ind + join(c)) for ind, c, m in rows if m and c), default=0) + 2
        for ind, code, com in rows:
            s = ind + join(code)
            if com:
                s = (s.ljust(col) if code else s) + com[0].text
            out.append(s.rstrip())
        return out, used_prev

    def lint_body(self, lineno, name, toks):
        words = [t.text.lower() for t in toks if t.kind == "word"]
        depth = 0
        for w in words:
            depth += (w in OPEN) - (w in CLOSE)
            if depth < 0:
                break
        if depth != 0:
            self.warn(lineno, f"{name}: unbalanced control structure")
        if words.count(">r") != words.count("r>"):
            self.warn(lineno, f"{name}: >r and r> counts differ")
        if any(t.kind == "locals" and t.text.startswith("{ ") for t in toks):
            self.warn(lineno, f"{name}: {{ }} locals are Gforth-only; Forth-2012 uses {{: :}}")

    def format(self, text):
        forms = self.parse(text)
        out = []
        for idx, f in enumerate(forms):
            if f[0] == "blank":
                if out and out[-1] != "":
                    out.append("")
            elif f[0] == "top":
                out.append(join(f[2]))
            else:
                prev = None
                if out and out[-1] and idx > 0 and forms[idx - 1][0] == "top":
                    toks = forms[idx - 1][2]
                    if len(toks) == 1 and toks[0].kind == "comment":
                        prev = toks[0]
                lines, used = self.format_def(f[1], f[2], prev)
                if used:
                    out.pop()
                out.extend(lines)
        while out and out[-1] == "":
            out.pop()
        while out and out[0] == "":
            out.pop(0)
        return "\n".join(out) + "\n"


def main(argv):
    write = check = False
    width, files = 80, []
    it = iter(argv)
    for a in it:
        if a in ("-w", "--write"):
            write = True
        elif a == "--check":
            check = True
        elif a == "--width":
            width = int(next(it))
        else:
            files.append(a)
    if not files:
        print(__doc__, file=sys.stderr)
        return 2
    status = 0
    for path in files:
        src = open(path).read()
        f = Formatter(path, width)
        dst = f.format(src)
        for w in f.warnings:
            print(w, file=sys.stderr)
        if check:
            if dst != src:
                print(f"would reformat {path}", file=sys.stderr)
                status = 1
        elif write:
            if dst != src:
                open(path, "w").write(dst)
        else:
            sys.stdout.write(dst)
    return status


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
