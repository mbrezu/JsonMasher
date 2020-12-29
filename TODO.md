# Things to do next

- [ ] get rid of thunk and make IJsonMasherOperator callable
- [ ] parse functions definitions and function calls
- [ ] jq -n '{a : 1 | 2}' (seems I need to be able to parse pipes inside dicts)
- [ ] switch to System.Collections.Immutable for Json
- [ ] pretty printing (FancyPen?) for Json values
- [ ] pretty printing operators
- [ ] assignments WIP
- [ ] lexer/parser tokens position information for errors
- [ ] using Object.ReferenceEquals and RuntimeHelpers.GetHashCode for keys on IJsonMasherOperator
- [ ] has(key), keys, map, map_values
- [ ] del, to_entries, from_entries, select, error, transpose, range
- [ ] min/max/group/sort
- [ ] .. (recurse)
- [ ] error handling
- [ ] stack trace
- [ ] documentation
- [ ] nuget packages
