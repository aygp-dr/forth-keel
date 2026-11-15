// Compilation on the Rust keel, through its wasm-bindgen shim.
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

// The execution tokens frames will compile into threads at run time. A host
// has to supply these as inline data until the outer interpreter can look a
// name up in a dictionary, which is what `find' will do at M4.
const PLUS = Keel.primitive("+");
const EXIT_XT = EXIT;
const DUP = xtOf("dup");

// A definition wrong enough to corrupt HERE makes every later thread
// unassemblable, which is a failed experiment, not a crash.
try {
  // ---- numbers and the system variables ----------------------------------
  check("zero derives 0 with no literal", run(asm(["zero"])), [0]);
  check("here is address zero, where the map puts HERE", run(asm(["here"])), [0]);

  const herePtr = run(asm(["here", "@"]));
  checks += 1;
  if (!(herePtr.length === 1 && herePtr[0] > 0 && herePtr[0] % CELL === 0)) {
    fails += 1;
    console.log(`FAIL here @ is a cell-aligned dictionary address: got [${herePtr}]`);
  }

  // ---- , is the compiler -------------------------------------------------
  check(
    ", stores a cell that can be fetched back",
    run(asm(["here", "@", ">r", "lit", 1234, ",", "r>", "@"])),
    [1234],
  );
  check(
    ", advances HERE by exactly one cell",
    run(asm(["here", "@", ">r", "lit", 7, ",", "here", "@", "r>", "-"])),
    [CELL],
  );
  check(
    "two commas advance HERE by two cells",
    run(asm(["here", "@", ">r", "lit", 7, ",", "lit", 8, ",", "here", "@", "r>", "-"])),
    [2 * CELL],
  );
  check(
    "allot reserves without writing",
    run(asm(["here", "@", ">r", "lit", 16, "allot", "here", "@", "r>", "-"])),
    [16],
  );

  // ---- frames compiles a new word and runs it ----------------------------
  // Remember HERE, compile the body [+ exit], then execute it on 2 and 3.
  check(
    "a word compiled at run time adds",
    run(asm([
      "here", "@", ">r",
      "lit", PLUS, ",",
      "lit", EXIT_XT, ",",
      "lit", 2, "lit", 3,
      "r>", "execute",
    ])),
    [5],
  );
  // A doubling word: [dup + exit].
  check(
    "a word compiled at run time doubles",
    run(asm([
      "here", "@", ">r",
      "lit", DUP, ",",
      "lit", PLUS, ",",
      "lit", EXIT_XT, ",",
      "lit", 21,
      "r>", "execute",
    ])),
    [42],
  );
  // Two words compiled one after the other, both executed: HERE must have
  // advanced between them, or the second would overwrite the first.
  check(
    "two words compiled in sequence stay distinct",
    run(asm([
      "here", "@", ">r",                        // first body: [dup + exit]
      "lit", DUP, ",", "lit", PLUS, ",", "lit", EXIT_XT, ",",
      "here", "@", ">r",                        // second body: [+ exit]
      "lit", PLUS, ",", "lit", EXIT_XT, ",",
      "lit", 10, "lit", 32, "r>", "execute",    // 10 32 + -> 42
      "lit", 5, "r>", "execute",                // 5 dup + -> 10
    ])),
    [42, 10],
  );
  // The compiled word is a real word: execute it twice.
  check(
    "a compiled word can be executed twice",
    run(asm([
      "here", "@", ">r",
      "lit", DUP, ",", "lit", PLUS, ",", "lit", EXIT_XT, ",",
      "lit", 3, "r>", "dup", ">r", "execute",
      "lit", 100, "r>", "execute",
    ])),
    [6, 200],
  );
} catch (e) {
  const msg = typeof e === "string" ? e : e.message || e;
  console.log(`ABORTED ${msg}`);
  console.log("a definition corrupted the dictionary, so no further thread could be assembled");
  process.exit(1);
}

console.log(`${checks} checks, ${fails} failures`);
process.exit(fails === 0 ? 0 : 1);
