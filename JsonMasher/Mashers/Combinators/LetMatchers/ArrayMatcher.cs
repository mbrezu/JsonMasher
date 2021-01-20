using System.Collections.Generic;
using System.Linq;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Combinators.LetMatchers
{
    public class ArrayMatcher : IMatcher
    {
        IMatcher[] _elements;
        public IReadOnlyCollection<IMatcher> Elements => _elements;

        public ArrayMatcher(params IMatcher[] elements) => _elements = elements;

        public IEnumerable<MatchSet> GetMatches(Json value, IMashContext context)
        {
            if (value == null || value.Type != JsonValueType.Array)
            {
                throw context.Error($"Expected an array, not {value?.Type}", value);
            }
            return GetMatchesImpl(value, context, new List<LetMatch>(), 0);
        }

        private IEnumerable<MatchSet> GetMatchesImpl(
            Json value, IMashContext context, List<LetMatch> matches, int level)
        {
            if (level == _elements.Length)
            {
                yield return new MatchSet(matches.ToList());
            }
            else
            {
                var elementValue = level < value.GetLength() ? value.GetElementAt(level) : Json.Null;
                foreach (var matchSet in _elements[level].GetMatches(elementValue, context))
                {
                    var oldLength = matches.Count;
                    matches.AddRange(matchSet.Matches);
                    foreach (var resultSet in GetMatchesImpl(value, context, matches, level + 1))
                    {
                        yield return resultSet;
                    }
                    matches.RemoveRange(oldLength, matches.Count - oldLength);
                }
            }
        }
    }
}
