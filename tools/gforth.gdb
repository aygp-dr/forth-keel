# Derived from spec.org @ v0.1. The spec governs; if this file disagrees with it, this file is the bug. Do not add facts here that are not in the spec — add them to the spec first.
# gforth helpers for gdb. Load them with:
#   gdb -x tools/gforth.gdb --args gforth yourfile.fs -e bye
# then: fstart, fbreak +, fstack, frstack, fprims.
set pagination off
set print pretty on

python
import gdb

# Length of prim_names[] in gforth 0.7.3. Reading stops early if the build
# has fewer, so an overestimate is harmless.
PRIM_COUNT = 334


def _prim_names():
    """Map each primitive name to its index in prim_names[]."""
    names = gdb.parse_and_eval("prim_names")
    out = {}
    for i in range(PRIM_COUNT):
        try:
            out[names[i].string()] = i
        except gdb.error:
            break
    return out


def _cells(pointer, count):
    """Yield (index, value) for count cells from pointer, stopping at unmapped memory."""
    for i in range(count):
        try:
            yield i, int(pointer[i])
        except gdb.MemoryError:
            return


class Fstart(gdb.Command):
    """Run until gforth's primitive table exists, so fbreak can resolve names.

vm_prims[] is filled in only after the image is loaded, so resolving a
primitive before that reads uninitialised memory."""

    def __init__(self):
        super(Fstart, self).__init__("fstart", gdb.COMMAND_RUNNING)

    def invoke(self, arg, from_tty):
        gdb.execute("tbreak gforth_go")
        gdb.execute("run " + arg)


class Fbreak(gdb.Command):
    """Break on a Forth primitive by name: fbreak <word> [if <condition>]

Resolves the word through prim_names[] to its label address in vm_prims[].
Every primitive is a label inside the single gforth_engine function, so this
is an address breakpoint, not a function one. Run fstart first. A condition
may follow, which is how to stop on one specific case:

    fbreak + if sp[0]==3 && sp[1]==2"""

    def __init__(self):
        super(Fbreak, self).__init__("fbreak", gdb.COMMAND_BREAKPOINTS)

    def invoke(self, arg, from_tty):
        parts = arg.strip().split(None, 1)
        if not parts:
            print("usage: fbreak <forth-word> [if <condition>]")
            return
        name, cond = parts[0], (parts[1] if len(parts) > 1 else "")
        index = _prim_names().get(name)
        if index is None:
            print('fbreak: "%s" is not a primitive in this build; try fprims' % name)
            return
        try:
            addr = int(gdb.parse_and_eval("vm_prims")[index])
        except gdb.error as e:
            print("fbreak: cannot read vm_prims (%s); run fstart first" % e)
            return
        gdb.execute(("break *%d %s" % (addr, cond)).strip())
        print('fbreak: "%s" is primitive %d at %#x' % (name, index, addr))


class Fstack(gdb.Command):
    """Print cells from the data stack pointer: fstack [count]

sp[0] is the top of stack in a build made with the spec's debug flags. Stop
inside gforth_engine first, for instance at an fbreak breakpoint."""

    def __init__(self):
        super(Fstack, self).__init__("fstack", gdb.COMMAND_DATA)

    def invoke(self, arg, from_tty):
        count = int(arg) if arg.strip() else 8
        try:
            sp = gdb.selected_frame().read_var("sp")
        except (gdb.error, ValueError):
            print("fstack: no sp in scope; stop inside gforth_engine first")
            return
        print("sp = %#x" % int(sp))
        for i, v in _cells(sp, count):
            print("  sp[%d] = %d%s" % (i, v, "   <- top" if i == 0 else ""))


class Frstack(gdb.Command):
    """Print cells from the return stack pointer: frstack [count]"""

    def __init__(self):
        super(Frstack, self).__init__("frstack", gdb.COMMAND_DATA)

    def invoke(self, arg, from_tty):
        count = int(arg) if arg.strip() else 8
        try:
            rp = gdb.selected_frame().read_var("rp")
        except (gdb.error, ValueError):
            print("frstack: no rp in scope; stop inside gforth_engine first")
            return
        print("rp = %#x" % int(rp))
        for i, v in _cells(rp, count):
            print("  rp[%d] = %#x%s" % (i, v, "   <- top" if i == 0 else ""))


class Fprims(gdb.Command):
    """List the primitives this gforth was built with: fprims [substring]"""

    def __init__(self):
        super(Fprims, self).__init__("fprims", gdb.COMMAND_DATA)

    def invoke(self, arg, from_tty):
        pattern = arg.strip()
        try:
            names = _prim_names()
        except gdb.error as e:
            print("fprims: cannot read prim_names (%s)" % e)
            return
        for name, index in sorted(names.items(), key=lambda kv: kv[1]):
            if not pattern or pattern in name:
                print("%4d  %s" % (index, name))


Fstart()
Fbreak()
Fstack()
Frstack()
Fprims()
end
