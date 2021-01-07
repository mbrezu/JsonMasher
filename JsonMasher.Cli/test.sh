#!/bin/sh

# Used for debugging multi-line examples.

read -r -d '' PROGRAM << EOM
def combinations:
    if length == 0 then [] else
        .[0][] as \$x
          | (.[1:] | combinations) as \$y
          | [\$x] + \$y
    end;
[[1, 2], [3, 4]] | combinations
EOM

dotnet run -- -nd "$PROGRAM"
#jq -c -n "$PROGRAM"