using System;
using System.Linq;

namespace JsonMasher.Mashers.Operators
{
    public class Plus
    {
        public static Json Operator(Json t1, Json t2)
            => (t1.Type, t2.Type) switch
            {
                (JsonValueType.Number, JsonValueType.Number)
                    => Json.Number(t1.GetNumber() + t2.GetNumber()),
                (JsonValueType.String, JsonValueType.String)
                    => Json.String(t1.GetString() + t2.GetString()),
                (JsonValueType.Array, JsonValueType.Array)
                    => Json.Array(t1.EnumerateArray().Concat(t2.EnumerateArray())),
                (JsonValueType.Object, JsonValueType.Object)
                    => Json.Object(t1.EnumerateObject().Concat(t2.EnumerateObject())),
                _ => throw new InvalidOperationException()
            };
    }
}
