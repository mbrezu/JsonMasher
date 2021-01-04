using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Mashers.Primitives
{
    public class Enumerate : IJsonMasherOperator
    {
        public bool IsOptional { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
            => json.Type switch
            {
                JsonValueType.Array => json.EnumerateArray(),
                JsonValueType.Object => json.EnumerateObject().Select(kv => kv.Value),
                _ => !IsOptional
                    ? throw context.Error($"Can't enumerate {json.Type}.", stack.Push(this), json)
                    : Enumerable.Empty<Json>()
            };

        // All these are the same for testing/FluentAssertions purposes,
        // but different for SourceInformation purposes, so the singletons were removed.
        public override bool Equals(object obj) => obj is Enumerate;
        public override int GetHashCode() => 1;
    }
}
