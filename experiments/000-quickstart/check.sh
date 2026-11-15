#!/usr/bin/env bash
# check.sh FILE EXPECTED: exit 0 when gforth's output for FILE equals EXPECTED byte for byte.
set -u
diff -u "$2" <(gforth "$1" -e bye 2>&1)
