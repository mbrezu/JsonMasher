# Things to do next

- [ ] error handling
  - [ ] runtime positions for errors
  - [ ] stack trace
    - [ ] pretty printing operators?
- [ ] switch to System.Collections.Immutable for Json
- [ ] pretty printing (FancyPen?) for Json values
- [ ] assignments WIP
- [ ] using Object.ReferenceEquals and RuntimeHelpers.GetHashCode for keys on IJsonMasherOperator
- [ ] slice selectors (I thought that .[a:b] would be the same as .[range(a; b)], but apparently that's not true; maybe make `<f>:<g>` syntactic sugar for `range(<f>; <g>)` ?
- [ ] has(key), keys, map, map_values
- [ ] del, to_entries, from_entries, select, error, transpose, range
- [ ] min/max/group/sort
- [ ] .. (recurse)
- [ ] documentation
- [ ] nuget packages
- [ ] jq -n '{a : 1 | 2}' (seems I need to be able to parse pipes inside dicts)
- [ ] destructuring bind
- [ ] benchmarks (https://benchmarkdotnet.org/articles/guides/getting-started.html)