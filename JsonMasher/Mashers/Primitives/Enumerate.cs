using System.Collections.Generic;
using System.Linq;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Primitives
{
    public class Enumerate : IJsonMasherOperator, IPathGenerator
    {
        public bool IsOptional { get; init; }
        public IJsonMasherOperator Target { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
        {
            if (Target == null)
            {
                return MashOne(json, context);
            }
            else
            {
                return Target
                    .Mash(json, context)
                    .SelectMany(x => MashOne(x, context));
            }
        }

        private IEnumerable<Json> MashOne(Json json, IMashContext context)
        {
            return json.Type switch
            {
                JsonValueType.Array => json.EnumerateArray(),
                JsonValueType.Object => json.EnumerateObject().Select(kv => kv.Value),
                _ => !IsOptional
                    ? throw context.PushStack(this).Error($"Can't enumerate {json.Type}.", json)
                    : Enumerable.Empty<Json>()
            };
        }

        // All these are the same for testing/FluentAssertions purposes,
        // but different for SourceInformation purposes, so the singletons were removed.
        public override bool Equals(object obj) => obj is Enumerate;
        public override int GetHashCode() => 1;

        public IEnumerable<PathAndValue> GeneratePaths(
            JsonPath pathSoFar, Json json, IMashContext context)
            => json.Type switch
            {
                JsonValueType.Array => ArrayGeneratePaths(pathSoFar, json, context),
                JsonValueType.Object => ObjectGeneratePaths(pathSoFar, json, context),
                _ => !IsOptional
                    ? throw context.PushStack(this).Error($"Can't enumerate {json.Type}.")
                    : Enumerable.Empty<PathAndValue>()
            };

        private IEnumerable<PathAndValue> ArrayGeneratePaths(
            JsonPath pathSoFar, Json json, IMashContext context)
        {
            for (int i = 0; i < json.GetLength(); i++)
            {
                yield return new PathAndValue(
                    pathSoFar.Extend(new IntPathPart(i)), json.GetElementAt(i));
            }
        }

        private IEnumerable<PathAndValue> ObjectGeneratePaths(
            JsonPath pathSoFar, Json json, IMashContext context)
        {
            foreach (var kv in json.EnumerateObject())
            {
                yield return new PathAndValue(
                    pathSoFar.Extend(new StringPathPart(kv.Key)),
                    kv.Value);
            }
        }
    }
}
