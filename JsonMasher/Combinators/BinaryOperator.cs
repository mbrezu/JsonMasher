using System;
using System.Collections.Generic;

// TODO: extend this into n-ary operators/functions with
// code like the one in ConstructObject.
namespace JsonMasher.Combinators
{
    public class BinaryOperator : IJsonMasherOperator
    {
        public IJsonMasherOperator First { get; init; }
        public IJsonMasherOperator Second { get; init; }
        public Func<Json, Json, Json> Function { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
        {
            foreach (var t1 in First.Mash(json, context))
            {
                foreach (var t2 in Second.Mash(json, context))
                {
                    yield return Function(t1, t2);
                }
            }
        }

        public IEnumerable<Json> Mash(IEnumerable<Json> seq, IMashContext context)
        {
            foreach (var t1 in First.Mash(seq, context))
            {
                foreach (var t2 in Second.Mash(seq, context))
                {
                    yield return Function(t1, t2);
                }
            }
        }
    }
}
