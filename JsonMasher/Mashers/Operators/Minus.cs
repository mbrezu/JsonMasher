using System;
using System.Linq;

namespace JsonMasher.Mashers.Operators
{
    public class Minus
    {
        public static Json Operator(Json t1, Json t2)
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
