using System.Collections.Generic;
using System.Linq;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Primitives
{
    public class Enumerate : IJsonMasherOperator, IPathGenerator
    {
        public bool IsOptional { get; init; }
        public IJsonMasherOperator Target { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            if (Target == null)
            {
                return MashOne(json, context, stack);
            }
            else
            {
                return Target
                    .Mash(json, context, stack)
                    .SelectMany(x => MashOne(x, context, stack));
            }
        }

        private IEnumerable<Json> MashOne(Json json, IMashContext context, IMashStack stack)
        {
            return json.Type switch
            {
                JsonValueType.Array => json.EnumerateArray(),
                JsonValueType.Object => json.EnumerateObject().Select(kv => kv.Value),
                _ => !IsOptional
                    ? throw context.Error($"Can't enumerate {json.Type}.", stack.Push(this), json)
                    : Enumerable.Empty<Json>()
            };
        }

        // All these are the same for testing/FluentAssertions purposes,
        // but different for SourceInformation purposes, so the singletons were removed.
        public override bool Equals(object obj) => obj is Enumerate;
        public override int GetHashCode() => 1;

        public IEnumerable<PathAndValue> GeneratePaths(
            JsonPath pathSoFar, Json json, IMashContext context, IMashStack stack)
            => json.Type switch
            {
                JsonValueType.Array => ArrayGeneratePaths(pathSoFar, json, context, stack),
                JsonValueType.Object => ObjectGeneratePaths(pathSoFar, json, context, stack),
                _ => !IsOptional
                    ? throw context.Error($"Can't enumerate {json.Type}.", stack.Push(this))
                    : Enumerable.Empty<PathAndValue>()
            };

        private IEnumerable<PathAndValue> ArrayGeneratePaths(
            JsonPath pathSoFar, Json json, IMashContext context, IMashStack stack)
        {
            for (int i = 0; i < json.GetLength(); i++)
            {
                yield return new PathAndValue(
                    pathSoFar.Extend(new IntPathPart(i)), json.GetElementAt(i));
            }
        }

        private IEnumerable<PathAndValue> ObjectGeneratePaths(
            JsonPath pathSoFar, Json json, IMashContext context, IMashStack stack)
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
