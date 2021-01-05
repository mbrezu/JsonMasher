using System.Collections.Generic;

namespace JsonMasher.Mashers
{
    public record PathAndValue(JsonPath Path, Json Value);

    public interface IPathGenerator
    {
        IEnumerable<PathAndValue> GeneratePaths(
            JsonPath pathSoFar, Json json, IMashContext context, IMashStack stack);
    }
}
