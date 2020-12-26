using System.Collections.Generic;

namespace JsonMasher.Combinators
{
    public class ConstructArray : IJsonMasherOperator
    {
        public IJsonMasherOperator ElementsMasher { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
            => Json.Array(ElementsMasher.Mash(json, context)).AsEnumerable();
    }
}
