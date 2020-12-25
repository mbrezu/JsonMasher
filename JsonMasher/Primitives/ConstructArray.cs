using System.Collections.Generic;

namespace JsonMasher.Primitives
{
    public class ConstructArray : IJsonMasherOperator
    {
        public IJsonMasherOperator ElementsMasher { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
            => Json.Array(ElementsMasher.Mash(json, context)).AsEnumerable();

        public IEnumerable<Json> Mash(IEnumerable<Json> seq, IMashContext context)
            => Json.Array(ElementsMasher.Mash(seq, context)).AsEnumerable();
    }
}
