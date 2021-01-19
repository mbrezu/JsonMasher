using System.Collections.Generic;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Combinators.LetMatchers
{
    public class AllMatcher : IMatcher
    {
        public string Name { get; init; }
        public AllMatcher(string name) => Name = name;

        public IEnumerable<LetMatch> GetMatches(Json value)
        {
            yield return new LetMatch(Name, value);
        }
    }
}
