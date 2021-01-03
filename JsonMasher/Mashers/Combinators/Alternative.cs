using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Mashers.Combinators
{
    public class Alternative : IJsonMasherOperator
    {
        public IJsonMasherOperator First { get; init; }
        public IJsonMasherOperator Second { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            var newStack = stack.Push(this);
            context.Tick(newStack);
            var values = First.Mash(json, context, newStack).Where(v => v.GetBool());
            if (values.Any()) 
            {
                return values;
            }
            else
            {
                return Second.Mash(json, context, newStack);
            }
        }
    }
}
