using System.Collections.Generic;

namespace JsonMasher.Mashers.Primitives
{
    public class GetVariable : IJsonMasherOperator
    {
        public string Name { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
            => context.GetVariable(Name).AsEnumerable();
    }
}
