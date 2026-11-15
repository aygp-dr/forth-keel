.RECIPEPREFIX = >
.PHONY: test
test:
> @gforth --version 2>&1 | head -1
> @./check.sh tests/hello.fs tests/hello.out && echo "PASS hello matches"
> @if ./check.sh tests/hello.fs tests/wrong.out > /dev/null; then echo "FAIL check accepted wrong.out"; exit 1; else echo "PASS check rejects wrong.out"; fi
