using System.Collections.Generic;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Combinators.LetMatchers
{
    public class ValueMatcher : IMatcher
    {
        public string Name { get; init; }
        public ValueMatcher(string name) => Name = name;

        public IEnumerable<LetMatch> GetMatches(Json value, IMashContext _)
        {
            return new LetMatch(Name, value).AsEnumerable();
        }
    }
}
