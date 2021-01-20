using System.Collections.Generic;
using System.Linq;
using JsonMasher.Compiler;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Combinators.LetMatchers
{
    public class AlternativeMatcher : IMatcher
    {
        public IMatcher First { get; init; }
        public IMatcher Second { get; init; }

        public IEnumerable<string> GetAllNames()
            => First.GetAllNames().Union(Second.GetAllNames());

        public IEnumerable<MatchSet> GetMatches(Json value, IMashContext context)
        {
            var allNames = GetAllNames();
            try 
            {
                return First.GetMatches(value, context).Select(ms => Extend(ms, allNames));
            }
            catch (JsonMasherException)
            {
                return Second.GetMatches(value, context).Select(ms => Extend(ms, allNames));
            }
        }

        private MatchSet Extend(MatchSet matchSet, IEnumerable<string> allNames)
        {
            var namesToAdd = allNames.Except(matchSet.Matches.Select(x => x.Name)).ToArray();
            if (namesToAdd.Length == 0)
            {
                return matchSet;
            }
            else
            {
                var matches = matchSet.Matches.ToList();
                foreach (var name in namesToAdd)
                {
                    matches.Add(new LetMatch(name, Json.Null));
                }
                return new MatchSet(matches);
            }
        }
    }
}
