using System.Collections.Generic;

namespace JsonMasher.Mashers.Combinators
{
    public class ConstructArray : IJsonMasherOperator
    {
        public IJsonMasherOperator Elements { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
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
