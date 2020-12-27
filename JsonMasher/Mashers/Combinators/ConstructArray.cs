using System.Collections.Generic;

namespace JsonMasher.Mashers.Combinators
{
    public class ConstructArray : IJsonMasherOperator
    {
        public IJsonMasherOperator Elements { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
            => Json.Array(Elements.Mash(json, context)).AsEnumerable();
    }
}