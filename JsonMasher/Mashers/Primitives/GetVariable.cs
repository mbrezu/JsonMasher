using System.Collections.Generic;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Primitives
{
    public class GetVariable : IJsonMasherOperator
    {
        public string Name { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
        {
            context = context.PushStack(this);
            context.Tick();
            return context.GetVariable(Name).AsEnumerable();
        }
    }
}
