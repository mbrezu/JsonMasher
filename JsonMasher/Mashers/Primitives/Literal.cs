using System.Collections.Generic;

namespace JsonMasher.Mashers.Primitives
{
    public class Literal : IJsonMasherOperator
    {
        public Json Value { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
            => Value.AsEnumerable();
    }
}
