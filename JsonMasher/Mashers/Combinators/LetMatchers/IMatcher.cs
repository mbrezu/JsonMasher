using System.Collections.Generic;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Combinators.LetMatchers
{
    public record LetMatch(string Name, Json Value);
    public record MatchSet(IReadOnlyCollection<LetMatch> Matches);

    public interface IMatcher
    {
        IEnumerable<MatchSet> GetMatches(Json value, IMashContext context);
        IEnumerable<string> GetAllNames();
    }
}
