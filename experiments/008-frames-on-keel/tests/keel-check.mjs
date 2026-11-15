// Host 2 of 2: the Rust keel, through its wasm-bindgen shim.
//
// Usage: node keel-check.mjs <absolute path to src/frames.fs>
//   KEEL_WASM_PKG overrides where keel_wasm.js is found.
//
// The seed assembler below is the whole bootstrap: a frames word is a thread
// of execution tokens in the dictionary, and its body address becomes its own
// token. Nothing here parses Forth at run time and nothing pushes a literal --
// that is the outer interpreter's job, and the outer interpreter is itself a
// frames word (M4). This is the scaffold it replaces.
import { readFileSync, existsSync } from "node:fs";
import { createRequire } from "node:module";

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

// ---------------------------------------------------------------- seed

/** Strip \ line comments and ( ... ) comments, then split into tokens. */
function tokenize(source) {
  return source
    .replace(/(^|\s)\\[^\n]*/g, " ")
    .replace(/\(\s[^)]*\)/g, " ")
    .split(/\s+/)
    .filter((t) => t.length > 0);
}

/**
 * Compile every ": name tokens ;" definition into the dictionary.
 * Returns a Map from frames word name to its execution token.
 */
function seed(source) {
  const words = new Map();
  const resolve = (token) => {
    if (PRIMS.has(token)) return Keel.primitive(token);
    if (words.has(token)) return words.get(token);
    if (/^-?\d+$/.test(token)) {
      throw new Error(
        `frames used the literal \`${token}'. The keel has no literal ` +
          `primitive, so a literal would come from the host, not the keel.`,
      );
    }
    throw new Error(`frames used \`${token}', which is neither a keel primitive nor an earlier frames word`);
  };

  const tokens = tokenize(source);
  let i = 0;
  while (i < tokens.length) {
    if (tokens[i] !== ":") throw new Error(`expected \`:' but found \`${tokens[i]}'`);
    const name = tokens[i + 1];
    if (!name) throw new Error("a definition ended before its name");
    const body = [];
    i += 2;
    while (i < tokens.length && tokens[i] !== ";") body.push(resolve(tokens[i++]));
    if (i >= tokens.length) throw new Error(`definition of \`${name}' has no \`;'`);
    i += 1;
    body.push(EXIT);
    words.set(name, vm.compile(body));
  }
  return words;
}

// A refusal here is a designed outcome, not a crash: report it as one line.
let words;
try {
  words = seed(readFileSync(framesPath, "utf8"));
} catch (e) {
  console.log(`REFUSED ${e.message}`);
  process.exit(1);
}

// A one-token thread per word, so each call costs no dictionary space.
const callers = new Map();
for (const [name, xt] of words) callers.set(name, vm.compile([xt, EXIT]));

// ---------------------------------------------------------------- harness

let checks = 0;
let fails = 0;

function clear() {
  while (vm.depth() > 0) vm.pop();
}

/** Run a frames word over an input stack and return the resulting stack. */
function call(name, inputs) {
  clear();
  for (const v of inputs) vm.push(v | 0);
  vm.run(callers.get(name));
  return Array.from(vm.stack());
}

function same(a, b) {
  return a.length === b.length && a.every((x, i) => x === b[i]);
}

function check(name, inputs, got, want) {
  checks += 1;
  if (!same(got, want)) {
    fails += 1;
    if (fails <= 10) {
      console.log(`FAIL ${name} on [${inputs}]: got [${got}], want [${want}]`);
    }
  }
}

// 32-bit reference semantics, which is what the keel implements.
const VALS = [-1, 0, 1, 2, 7, 255, -256, 2147483647, -2147483648];
const UNARY = { invert: (x) => ~x | 0, negate: (x) => -x | 0 };
const BINARY = {
  and: (a, b) => (a & b) | 0,
  or: (a, b) => (a | b) | 0,
  "-": (a, b) => (a - b) | 0,
  "=": (a, b) => (a === b ? -1 : 0),
  "<>": (a, b) => (a !== b ? -1 : 0),
};
// Exact stacks, which is stronger than experiment 001's fingerprint.
const STACKWORDS = {
  dup: [10, 20, 30, 30],
  drop: [10, 20],
  swap: [10, 30, 20],
  over: [10, 20, 30, 20],
  nip: [10, 30],
  tuck: [10, 30, 20, 30],
  "2dup": [10, 20, 30, 20, 30],
  "2drop": [10],
};

for (const [name, fn] of Object.entries(UNARY)) {
  for (const n of VALS) check(name, [n], call(name, [n]), [fn(n)]);
}
for (const [name, fn] of Object.entries(BINARY)) {
  for (const a of VALS) {
    for (const b of VALS) check(name, [a, b], call(name, [a, b]), [fn(a, b)]);
  }
}
for (const [name, want] of Object.entries(STACKWORDS)) {
  check(name, [10, 20, 30], call(name, [10, 20, 30]), want);
}
check("cell", [], call("cell", []), [4]);

console.log(`${checks} checks, ${fails} failures`);

// Reported apart from the 432 above, which are experiment 001's set.
const one = call("one", []);
if (same(one, [1])) {
  console.log("PASS one derives 1 without a literal");
} else {
  console.log(`FAIL one did not derive 1: got [${one}]`);
  fails += 1;
}

process.exit(fails === 0 ? 0 : 1);
