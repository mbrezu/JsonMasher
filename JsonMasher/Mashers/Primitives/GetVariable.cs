using System.Collections.Generic;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Primitives
{
    public class GetVariable : IJsonMasherOperator
    {
        public string Name { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            var newStack = stack.Push(this);
            context.Tick(newStack);
            return context.GetVariable(Name, newStack).AsEnumerable();
        }
    }
}
