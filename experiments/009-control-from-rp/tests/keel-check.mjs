// Control flow on the Rust keel, through its wasm-bindgen shim.
//
// Usage: node keel-check.mjs <absolute path to src/frames.fs>
//   KEEL_WASM_PKG overrides where keel_wasm.js is found.
//
// The seed assembler gained two things experiment 008 did not need, both of
// which are the compiler's job and are done here by the host as scaffolding:
//   [ n ... ]  compiles raw cells into the thread, for inline data
//   &exit      inside [ ], the address of this definition's own exit cell
// Frames cannot yet compile either for itself; `if`, `then` and friends are
// immediate words over `here` and `,`, and that is the next step after this.
import { readFileSync, existsSync } from "node:fs";
import { createRequire } from "node:module";

const CELL = 4;
const framesPath = process.argv[2];
const pkg =
  process.env.KEEL_WASM_PKG ||
  `${process.env.HOME}/ghq/github.com/aygp-dr/keel-rs/keel-wasm/pkg/keel_wasm.js`;

if (!framesPath) {
  console.error("keel-check: need the path to src/frames.fs as argv[1]");
  process.exit(2);
}
if (!existsSync(pkg)) {
  console.log(`SKIP keel host: no keel-wasm build at ${pkg}`);
  console.log("     build it with: wasm-pack build <keel-rs>/keel-wasm --target nodejs --out-dir pkg");
  process.exit(0);
}

const { Keel } = createRequire(import.meta.url)(pkg);
const vm = new Keel();
const EXIT = Keel.primitive("exit");
const PRIMS = new Set(Keel.primitiveNames());

function tokenize(source) {
  return source
    .replace(/(^|\s)\\[^\n]*/g, " ")
    .replace(/\(\s[^)]*\)/g, " ")
    .split(/\s+/)
    .filter((t) => t.length > 0);
}

/** Compile every definition, returning a Map of name to execution token. */
function seed(source) {
  const words = new Map();
  const tokens = tokenize(source);
  let i = 0;

  const resolveCall = (token) => {
    if (PRIMS.has(token)) return Keel.primitive(token);
    if (words.has(token)) return words.get(token);
    if (/^-?\d+$/.test(token)) {
      throw new Error(
        `frames used the literal \`${token}' as a word. The keel has no ` +
          `literal primitive; inline data goes in [ ] and is read by lit.`,
      );
    }
    throw new Error(`frames used \`${token}', neither a keel primitive nor an earlier frames word`);
  };

  while (i < tokens.length) {
    if (tokens[i] !== ":") throw new Error(`expected \`:' but found \`${tokens[i]}'`);
    const name = tokens[i + 1];
    if (!name) throw new Error("a definition ended before its name");
    i += 2;

    // Lay the body out as a list of entries first, so &exit can be resolved
    // before anything is written: the exit cell sits one past the body.
    const entries = [];
    while (i < tokens.length && tokens[i] !== ";") {
      const t = tokens[i++];
      if (t === "[") {
        while (i < tokens.length && tokens[i] !== "]") {
          const d = tokens[i++];
          if (d === "&exit") entries.push({ exitRef: true });
          else if (/^-?\d+$/.test(d)) entries.push({ cell: parseInt(d, 10) });
          else throw new Error(`inline [ ] takes integers or &exit, not \`${d}'`);
        }
        if (i >= tokens.length) throw new Error(`unclosed [ in \`${name}'`);
        i += 1;
      } else {
        entries.push({ cell: resolveCall(t) });
      }
    }
    if (i >= tokens.length) throw new Error(`definition of \`${name}' has no \`;'`);
    i += 1;

    // An empty compile reads HERE without allocating, which is how the body
    // base is known before the body exists.
    const base = vm.compile([]);
    const exitAddr = base + entries.length * CELL;
    const cells = entries.map((e) => (e.exitRef ? exitAddr : e.cell));
    cells.push(EXIT);
    const body = vm.compile(cells);
    if (body !== base) throw new Error(`body moved: expected ${base}, got ${body}`);
    words.set(name, body);
  }
  return words;
}

let words;
try {
  words = seed(readFileSync(framesPath, "utf8"));
} catch (e) {
  console.log(`REFUSED ${e.message}`);
  process.exit(1);
}

const xtOf = (name) => {
  if (PRIMS.has(name)) return Keel.primitive(name);
  if (words.has(name)) return words.get(name);
  throw new Error(`no word \`${name}'`);
};

/** Hand-assemble a thread: strings are words, numbers are raw cells. */
function asm(items) {
  const cells = items.map((x) => (typeof x === "string" ? xtOf(x) : x));
  cells.push(EXIT);
  return vm.compile(cells);
}

let checks = 0;
let fails = 0;

/**
 * Run a thread and return the resulting stack, or an error marker. A wrong
 * definition typically makes the kernel fault -- an unaligned fetch, or the
 * step limit on a runaway jump -- and that is a failed check, not a crash.
 */
function run(body, inputs = []) {
  while (vm.depth() > 0) vm.pop();
  for (const v of inputs) vm.push(v | 0);
  try {
    vm.run(body);
  } catch (e) {
    return [`kernel error: ${typeof e === "string" ? e : e.message || e}`];
  }
  return Array.from(vm.stack());
}

function check(label, got, want) {
  checks += 1;
  const same = got.length === want.length && got.every((x, i) => x === want[i]);
  if (!same) {
    fails += 1;
    console.log(`FAIL ${label}: got [${got}], want [${want}]`);
  }
}

// ---- lit: inline data the keel has no primitive to push ------------------
check("lit pushes the inline cell", run(asm(["lit", 42])), [42]);
check("lit twice", run(asm(["lit", 7, "lit", -9])), [7, -9]);
check("lit resumes past its cell", run(asm(["lit", 1, "lit", 2, "+"])), [3]);

// ---- branch: an unconditional jump over compiled code -------------------
// 0:lit 1:1 2:branch 3:offset 4:lit 5:999 6:exit. The offset is measured from
// the offset cell, so reaching exit from cell 3 is 3 cells on.
check("branch skips the code it jumps over", run(asm(["lit", 1, "branch", 3 * CELL, "lit", 999])), [1]);

// ---- ?branch: taken when the flag is false ------------------------------
// 0:lit 1:7 2:lit 3:flag 4:?branch 5:offset 6:lit 7:999 8:exit
const qbranch = (flag) => asm(["lit", 7, "lit", flag, "?branch", 3 * CELL, "lit", 999]);
check("?branch branches on false", run(qbranch(0)), [7]);
check("?branch falls through on true", run(qbranch(-1)), [7, 999]);
check("?branch falls through on any non-zero", run(qbranch(5)), [7, 999]);

// ---- a conditional, which is what if/else/then compile to ---------------
// 0:?branch 1:->6 2:lit 3:9 4:branch 5:->8 6:lit 7:7 8:exit
const conditional = asm(["?branch", 5 * CELL, "lit", 9, "branch", 3 * CELL, "lit", 7]);
check("conditional takes the zero arm", run(conditional, [0]), [7]);
check("conditional takes the non-zero arm", run(conditional, [1]), [9]);
check("conditional, non-zero again", run(conditional, [-1]), [9]);

// ---- >r and r>: growing the return stack with no rp! -------------------
check(">r then r> round-trips", run(asm(["lit", 5, ">r", "lit", 6, "r>"])), [6, 5]);
check(">r r> leaves the caller returnable", run(asm(["lit", 1, ">r", "r>", "lit", 2, "+"])), [3]);
check("nested >r r>", run(asm(["lit", 1, ">r", "lit", 2, ">r", "r>", "r>"])), [2, 1]);

// ---- rot, which needs the return stack --------------------------------
check("rot", run(asm(["rot"]), [10, 20, 30]), [20, 30, 10]);
check("rot three times is identity", run(asm(["rot", "rot", "rot"]), [10, 20, 30]), [10, 20, 30]);

// ---- a loop: begin ... until, with a backward offset -------------------
// 0:one 1:- 2:dup 3:0= 4:?branch 5:back-to-0 6:exit
const countdown = asm(["one", "-", "dup", "0=", "?branch", -5 * CELL]);
check("countdown from 5 reaches 0", run(countdown, [5]), [0]);
check("countdown from 1 reaches 0", run(countdown, [1]), [0]);
check("countdown from 100 reaches 0", run(countdown, [100]), [0]);

console.log(`${checks} checks, ${fails} failures`);
process.exit(fails === 0 ? 0 : 1);
