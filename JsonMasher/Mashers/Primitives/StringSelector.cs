using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Mashers.Primitives
{
    public class StringSelector : IJsonMasherOperator, IPathGenerator
    {
        public string Key { get; init; }
        public bool IsOptional { get; init; }


        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
            => json.Type switch {
                JsonValueType.Object => json.GetElementAt(Key).AsEnumerable(),
                _ => !IsOptional
                    ? throw context.Error($"Can't index {json.Type} with a string.", stack.Push(this), json)
                    : Enumerable.Empty<Json>()
            };

        public IEnumerable<PathAndValue> GeneratePaths(
            Path pathSoFar, Json json, IMashContext context, IMashStack stack)
        {
            yield return new PathAndValue(
                pathSoFar.Extend(new StringPathPart(Key)),
                json.Type == JsonValueType.Null ? json : Mash(json, context, stack).First());
        }
    }
}
