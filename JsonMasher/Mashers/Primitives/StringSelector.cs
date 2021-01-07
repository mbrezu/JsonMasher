using System.Collections.Generic;
using System.Linq;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Primitives
{
    public class StringSelector : IJsonMasherOperator, IPathGenerator
    {
        public string Key { get; init; }
        public bool IsOptional { get; init; }


        public IEnumerable<Json> Mash(Json json, IMashContext context)
            => json.Type switch {
                JsonValueType.Object => json.GetElementAt(Key).AsEnumerable(),
                _ => !IsOptional
                    ? throw context.PushStack(this).Error($"Can't index {json.Type} with a string.", json)
                    : Enumerable.Empty<Json>()
            };

        public IEnumerable<PathAndValue> GeneratePaths(
            JsonPath pathSoFar, Json json, IMashContext context)
        {
            yield return new PathAndValue(
                pathSoFar.Extend(new StringPathPart(Key)),
                json.Type == JsonValueType.Null ? json : Mash(json, context).First());
        }
    }
}
