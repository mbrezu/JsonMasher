using System.Collections.Generic;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Combinators
{
    public class ConstructArray : IJsonMasherOperator
    {
        public IJsonMasherOperator Elements { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            context.Tick(stack);
            if (Elements != null)
            {
                return Json.Array(Elements.Mash(json, context, stack.Push(this))).AsEnumerable();
            }
            else
            {
                return Json.ArrayParams().AsEnumerable();
            }
        }
    }
}
