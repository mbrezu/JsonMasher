using System.Collections.Generic;

namespace JsonMasher.Combinators
{
    public class Compose : IJsonMasherOperator
    {
        public IJsonMasherOperator First { get; init; }
        public IJsonMasherOperator Second { get; init; }

        public IEnumerable<Json> Mash(IEnumerable<Json> seq, IMashContext context)
        {
            foreach (var json in seq)
            {
                foreach (var result in Mash(json, context))
                {
                    yield return result;
                }
            }
        }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
        {
            foreach (var temp in First.Mash(json, context))
            {
                foreach (var result in Second.Mash(temp, context))
                {
                    yield return result;
                }
            }
        }

        public static IJsonMasherOperator AllParams(params IJsonMasherOperator[] mashers)
            => All(mashers);

        public static IJsonMasherOperator All(IEnumerable<IJsonMasherOperator> mashers)
            => mashers.Fold((m1, m2) => new Compose { First = m1, Second = m2 });
    }
}
