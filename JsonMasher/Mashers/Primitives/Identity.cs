using System.Collections.Generic;

namespace JsonMasher.Mashers.Primitives
{
    public class Identity : IJsonMasherOperator
    {
        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            context.Tick(stack);
            return json.AsEnumerable();
        }

        // All these are the same for testing/FluentAssertions purposes,
        // but different for SourceInformation purposes, so the singletons were removed.
        public override bool Equals(object obj) => obj is Identity;
        public override int GetHashCode() => 1;
    }
}
