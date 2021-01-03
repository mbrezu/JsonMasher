using System.Collections.Generic;
using System.Linq;
using JsonMasher.Compiler;

namespace JsonMasher.Mashers.Combinators
{
    public class ErrorSuppression : IJsonMasherOperator
    {
        public IJsonMasherOperator Body { get; set; }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            var newStack = stack.Push(this);
            context.Tick(newStack);
            var result = new List<Json>();
            try
            {
                foreach (var value in Body.Mash(json, context, newStack))
                {
                    result.Add(value);
                }
            }
            catch (JsonMasherException)
            {
            }
            return result;
        }
    }
}
