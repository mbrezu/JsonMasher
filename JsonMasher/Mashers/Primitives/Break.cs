using System.Collections.Generic;
using JsonMasher.Compiler;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Primitives
{
    public class Break : IJsonMasherOperator
    {
        public string Label { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
        {
            throw new JsonBreakException(Label);
        }
    }
}
