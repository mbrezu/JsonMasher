using System.Collections.Generic;
using System.Linq;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Combinators.LetMatchers
{
    public record ObjectMatcherProperty(IJsonMasherOperator Operator, IMatcher Matcher);

    public class ObjectMatcher : IMatcher
    {

        private ObjectMatcherProperty[] _properties;
        public IReadOnlyCollection<ObjectMatcherProperty> Properties => _properties;

        public ObjectMatcher(params ObjectMatcherProperty[] properties) => _properties = properties;

        public IEnumerable<MatchSet> GetMatches(Json value, IMashContext context)
        {
            if (value == null || value.Type != JsonValueType.Object)
            {
                throw context.Error($"Expected an object, not {value?.Type}", value);
            }
            return GetMatchesImpl(value, context, new List<LetMatch>(), 0);
        }

        private IEnumerable<MatchSet> GetMatchesImpl(
            Json value, IMashContext context, List<LetMatch> matches, int level)
        {
            if (level == _properties.Length)
            {
                yield return new MatchSet(matches.ToList());
            }
            else
            {
                var property = _properties[level];
                foreach (var key in property.Operator.Mash(value, context))
                {
                    if (key.Type != JsonValueType.String)
                    {
                        throw context.Error($"Expected a string, not {key?.Type}", key);
                    }
                    var propertyValue = value.ContainsKey(key.GetString())
                        ? value.GetElementAt(key.GetString())
                        : Json.Null;
                    foreach (var matchSet in property.Matcher.GetMatches(propertyValue, context))
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

        public IEnumerable<string> GetAllNames()
        {
            foreach (var property in _properties)
            {
                foreach (var name in property.Matcher.GetAllNames())
                {
                    yield return name;
                }
            }
        }
    }
}
