using System.Collections.Generic;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Combinators.LetMatchers
{
    public class ValueMatcher : IMatcher
    {
        public string Name { get; init; }
        public ValueMatcher(string name) => Name = name;

        public IEnumerable<MatchSet> GetMatches(Json value, IMashContext _)
        {
            var matches = new List<LetMatch> {
                new LetMatch(Name, value)
            };
            return new MatchSet(matches).AsEnumerable();
        }
    }
}
