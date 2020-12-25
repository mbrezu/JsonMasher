using System.Collections.Generic;

namespace JsonMasher.Primitives
{
    public class Literal : IJsonMasherOperator
    {
        public Json Value { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
            => Value.AsEnumerable();

        public IEnumerable<Json> Mash(IEnumerable<Json> seq, IMashContext context)
            => Value.AsEnumerable();
    }
}
