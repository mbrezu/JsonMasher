using System;
using System.Collections.Generic;

// TODO: extend this into n-ary operators/functions with
// code like the one in ConstructObject.
namespace JsonMasher.Mashers.Combinators
{
    public class BinaryOperator : IJsonMasherOperator
    {
        public IJsonMasherOperator First { get; init; }
        public IJsonMasherOperator Second { get; init; }
        public Func<Json, Json, Json> Operator { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
        {
            foreach (var t1 in First.Mash(json, context))
            {
                foreach (var t2 in Second.Mash(json, context))
                {
                    yield return Operator(t1, t2);
                }
            }
        }
    }
}
