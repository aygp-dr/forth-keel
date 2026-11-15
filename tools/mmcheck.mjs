// mmcheck: parse every Mermaid diagram in the given files; exit 1 on any syntax error.
// Accepts .mmd files, ```mermaid fences in .md, and #+begin_src mermaid blocks in .org.
import { JSDOM } from "jsdom";
import { readFileSync } from "node:fs";
const dom = new JSDOM("<!doctype html><html><body></body></html>");
globalThis.window = dom.window; globalThis.document = dom.window.document;
const { default: mermaid } = await import("mermaid");
mermaid.initialize({ startOnLoad: false });
const blocks = (path) => {
  const s = readFileSync(path, "utf8");
  if (path.endsWith(".mmd")) return [[path, s]];
  const re = path.endsWith(".org")
    ? /#\+begin_src mermaid[^\n]*\n([\s\S]*?)#\+end_src/gi
    : /```mermaid\n([\s\S]*?)```/g;
  return [...s.matchAll(re)].map((m, i) => [`${path}#${i + 1}`, m[1]]);
};
let n = 0, bad = 0;
for (const f of process.argv.slice(2)) for (const [id, src] of blocks(f)) {
  n++;
  try { await mermaid.parse(src); } catch (e) { bad++; console.log(`mermaid: FAIL ${id}: ${String(e.message).split("\n")[0]}`); }
}
console.log(bad ? `mermaid: ${bad}/${n} diagrams failed` : `mermaid: PASS ${n} diagrams`);
process.exit(bad || n === 0 ? 1 : 0);
