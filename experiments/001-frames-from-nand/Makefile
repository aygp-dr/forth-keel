.RECIPEPREFIX = >
.PHONY: test
test:
> @cd tests && gforth test.fs
> @cd tests && if gforth isolation.fs -e bye > ../isolation.log 2>&1; then echo "FAIL isolation: * compiled inside frames"; exit 1; fi
> @grep -q "Undefined word" isolation.log && echo "PASS isolation: * is undefined inside frames"
> @rm -f isolation.log
