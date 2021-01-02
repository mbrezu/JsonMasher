# Things to do next

- [ ] debugger (extend DebugMashStack)
  - [ ] stepping through
  - [ ] setting breakpoints
  - [ ] include in web site
- [ ] switch to System.Collections.Immutable for Json
- [ ] assignments WIP
  - [ ] more IJsonZipper implementations
  - [ ] assignment (=, not |=) implementation
- [ ] slice selectors (I thought that .[a:b] would be the same as .[range(a; b)], but apparently that's not true; maybe make `<f>:<g>` syntactic sugar for `range(<f>; <g>)` ?
- [ ] has(key), map_values
- [ ] del, to_entries, from_entries, error, transpose
- [ ] min/max/group/sort
- [ ] .. (recurse)
- [ ] documentation
- [ ] nuget packages
- [ ] jq -n '{a : 1 | 2}' (seems I need to be able to parse pipes inside dicts)
- [ ] destructuring bind
