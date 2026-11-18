.RECIPEPREFIX = >
.PHONY: test rules toy solve cross negative report kaggle clean
GF   = gforth
ELF  = src/elf.fs
OUT  = out
SEED = 2026
N    = 100

test: rules toy solve cross negative

# The replayer against the Data page's worked example and one puzzle per rule.
rules:
> @$(GF) $(ELF) tests/rules.fs -e bye | diff -u tests/rules.out - && echo "PASS rules: 12 puzzles, each rule rejected once"

toy:
> @$(GF) $(ELF) tests/toy.fs -e bye | diff -u tests/toy.out - && echo "PASS worked example replays in 4"
> @$(GF) -e 'true constant reuse?' $(ELF) src/solve.fs tests/toy-solve.fs -e bye > $(OUT).toy.fs; \
  $(GF) $(ELF) $(OUT).toy.fs -e bye | diff -u tests/toy.out - && echo "PASS reuse solver finds the 4-instruction program"; \
  rc=$$?; rm -f $(OUT).toy.fs; exit $$rc

$(OUT):
> @mkdir -p $(OUT)

# Both solver variants on the synthetic set; every program must replay ok.
solve: | $(OUT)
> @python3 tools/gen.py $(SEED) $(N) $(OUT)/synth.fs
> @for v in baseline reuse; do \
    flag=false; [ $$v = reuse ] && flag=true; \
    $(GF) -e "$$flag constant reuse?" $(ELF) src/solve.fs $(OUT)/synth.fs -e bye > $(OUT)/solved-$$v.fs || exit 1; \
    $(GF) $(ELF) $(OUT)/solved-$$v.fs -e bye > $(OUT)/result-$$v.txt; \
    bad=$$(grep -vc ' ok ' $(OUT)/result-$$v.txt); \
    [ "$$bad" = 0 ] || { echo "FAIL $$v: $$bad programs rejected"; grep -v ' ok ' $(OUT)/result-$$v.txt | head; exit 1; }; \
    echo "PASS $$v: $(N) programs, $$(awk '{s+=$$5} END {print s}' $(OUT)/result-$$v.txt) instructions"; \
  done

# The submission CSV, replayed by an independent implementation, must agree.
cross: solve
> @for v in baseline reuse; do \
    python3 tools/fs2csv.py $(OUT)/solved-$$v.fs $(OUT)/submission-$$v.csv; \
    py=$$(python3 tools/replay.py $(OUT)/synth.fs $(OUT)/submission-$$v.csv | awk '{print $$2}'); \
    gf=$$(awk '{s+=$$5} END {print s}' $(OUT)/result-$$v.txt); \
    [ "$$py" = "$$gf" ] || { echo "FAIL $$v: gforth $$gf, python $$py"; exit 1; }; \
  done; echo "PASS submission CSVs replay to the same totals in python"

# Each control weakens one rule; the harness must notice every time.
negative: cross
> @tmp=$$(mktemp -d); trap 'rm -rf $$tmp' EXIT; \
  sed 's/2dup mod if 2drop e-divide fail 1 exit then \/ ;/\/ ;/' $(ELF) > $$tmp/divide.fs; \
  sed 's/over vlimit @ u> if 2drop e-limit fail exit then//' $(ELF) > $$tmp/limit.fs; \
  sed 's/2dup > 0= if 2drop e-subtract fail 1 exit then - ;/- ;/' $(ELF) > $$tmp/subtract.fs; \
  sed 's/dup written + c@ 0= if drop e-undefined fail 1 exit then//' $(ELF) > $$tmp/undefined.fs; \
  for m in divide limit subtract undefined; do \
    cmp -s $(ELF) $$tmp/$$m.fs && { echo "FAIL control $$m: sed matched nothing"; exit 1; }; \
    if $(GF) $$tmp/$$m.fs tests/rules.fs -e bye 2>&1 | diff -q tests/rules.out - > /dev/null; then \
      echo "FAIL control $$m was accepted"; exit 1; fi; \
  done; \
  sed '2s/"+"/"-"/' $(OUT)/submission-reuse.csv > $$tmp/bad.csv; \
  if python3 tools/replay.py $(OUT)/synth.fs $$tmp/bad.csv > /dev/null; then echo "FAIL control csv was accepted"; exit 1; fi; \
  echo "PASS 5 negative controls rejected (divide, limit, subtract, undefined, csv)"

# Dashboard data. Run after test.
report: solve
> @python3 tools/report.py $(OUT)/synth.fs baseline=$(OUT)/result-baseline.txt reuse=$(OUT)/result-reuse.txt > $(OUT)/results.json
> @echo "wrote $(OUT)/results.json"

# With the competition data in data/ (needs the rules accepted on Kaggle):
kaggle: | $(OUT)
> @test -f data/puzzles.csv || { echo "data/puzzles.csv missing: join santa-2026 and download it"; exit 1; }
> @python3 tools/csv2fs.py data/puzzles.csv $(OUT)/scored.fs
> @$(GF) -e 'true constant reuse?' $(ELF) src/solve.fs $(OUT)/scored.fs -e bye > $(OUT)/solved-scored.fs
> @python3 tools/fs2csv.py $(OUT)/solved-scored.fs $(OUT)/submission.csv
> @python3 tools/replay.py $(OUT)/scored.fs $(OUT)/submission.csv

clean:
> rm -rf $(OUT)
