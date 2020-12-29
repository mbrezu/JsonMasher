using System;

namespace JsonMasher.Mashers.Operators
{
    public class Divide
    {
        public static Json Operator(Json t1, Json t2)
            => (t1.Type, t2.Type) switch
            {
                (JsonValueType.Number, JsonValueType.Number)
                    => Json.Number(t1.GetNumber() / t2.GetNumber()),
                _ => throw new InvalidOperationException()
            };
    }
}
