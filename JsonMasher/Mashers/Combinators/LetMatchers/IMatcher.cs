using System.Collections.Generic;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Combinators.LetMatchers
{
    public record LetMatch(string Name, Json Value);

    public interface IMatcher
    {
        IEnumerable<LetMatch> GetMatches(Json value, IMashContext context);
    }
}
