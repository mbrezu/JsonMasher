using System.Collections.Generic;
using JsonMasher.Compiler;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Combinators
{
    public class Label : IJsonMasherOperator
    {
        public string Name { get; init; }
        public IJsonMasherOperator Body { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
        {
            context = context.PushStack(this);
            context.Tick();
            var result = new List<Json>();
            try
            {
                foreach (var value in Body.Mash(json, context))
                {
                    result.Add(value);
                }
            }
            catch (JsonBreakException ex)
            {
                if (ex.Label != Name)
                {
                    throw new JsonMasherException("Breaking out of nested labels not supported.");
                }
            }
            return result;
        }
    }
}
