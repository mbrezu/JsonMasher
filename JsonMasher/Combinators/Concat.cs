using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Combinators
{
    public class Concat : IJsonMasherOperator
    {
        public IJsonMasherOperator First { get; init; }
        public IJsonMasherOperator Second { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
            => First.Mash(json, context).Concat(Second.Mash(json, context));

        public IEnumerable<Json> Mash(IEnumerable<Json> seq, IMashContext context)
            => First.Mash(seq, context).Concat(Second.Mash(seq, context));

        public static IJsonMasherOperator AllParams(params IJsonMasherOperator[] mashers)
            => All(mashers);

        public static IJsonMasherOperator All(IEnumerable<IJsonMasherOperator> mashers)
            => mashers.Fold((m1, m2) => new Concat { First = m1, Second = m2 });
    }
}
