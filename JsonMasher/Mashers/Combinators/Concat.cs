using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Mashers.Combinators
{
    public class Concat : IJsonMasherOperator
    {
        public IJsonMasherOperator First { get; init; }
        public IJsonMasherOperator Second { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            var newStack = stack.Push(this);
            context.Tick(stack);
            return First.Mash(json, context, newStack)
                .Concat(Second.Mash(json, context, newStack));
        }

        public static IJsonMasherOperator AllParams(params IJsonMasherOperator[] mashers)
            => All(mashers);

        public static IJsonMasherOperator All(IEnumerable<IJsonMasherOperator> mashers)
            => mashers.Fold((m1, m2) => new Concat { First = m1, Second = m2 });
    }
}
