.RECIPEPREFIX = >
.PHONY: test
test:
> @set -e; for d in [0-9][0-9][0-9]-*/; do echo "== $$d"; $(MAKE) -s -C $$d test; done
