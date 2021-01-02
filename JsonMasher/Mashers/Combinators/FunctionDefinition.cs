using System.Collections.Generic;

namespace JsonMasher.Mashers.Combinators
{
    public class FunctionDefinition : IJsonMasherOperator
    {
        public string Name { get; init; }
        public List<string> Arguments { get; init; }
        public IJsonMasherOperator Body { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            context.Tick(stack);
            context.SetCallable(Name, Arguments, Body);
            return json.AsEnumerable();
        }
    }
}
