using System;
using System.Linq;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class Minus
    {
        public static Builtin Builtin = Utils.MakeBinaryBuiltin(Operator);
        static Json Operator(Json t1, Json t2)
            => (t1.Type, t2.Type) switch
            {
                (JsonValueType.Number, JsonValueType.Number)
                    => Json.Number(t1.GetNumber() - t2.GetNumber()),
                (JsonValueType.Array, JsonValueType.Array)
                    => Json.Array(t1.EnumerateArray()
                        .Where(e1 => !t2.EnumerateArray().Any(e2 => e1.DeepEqual(e2)))),
                _ => throw new InvalidOperationException()
            };
    }
}
