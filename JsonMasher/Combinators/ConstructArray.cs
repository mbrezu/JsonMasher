using System.Collections.Generic;

namespace JsonMasher.Combinators
{
    public class ConstructArray : IJsonMasherOperator
    {
        public IJsonMasherOperator Elements { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
            => Json.Array(Elements.Mash(json, context)).AsEnumerable();
    }
}
