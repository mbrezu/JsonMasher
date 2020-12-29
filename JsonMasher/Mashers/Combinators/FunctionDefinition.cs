using System.Collections.Generic;

namespace JsonMasher.Mashers.Combinators
{
    public class FunctionDefinition : IJsonMasherOperator
    {
        public string Name { get; init; }
        public List<string> Arguments { get; init; }
        public IJsonMasherOperator Body { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
        {
            if (Arguments == null || Arguments.Count == 0) {
                context.SetCallable(Name, Body);
            }
            else
            {
                context.SetCallable(Name, new Function(Body, Arguments));
            }
            return json.AsEnumerable();
        }
    }
}
