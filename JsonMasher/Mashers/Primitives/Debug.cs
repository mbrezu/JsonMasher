using System.Collections.Generic;

namespace JsonMasher.Mashers.Primitives
{
    public class Debug : IJsonMasherOperator
    {
        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            context.Tick(stack);
            context.LogValue(json);
            yield return json;
        }
    }
}
