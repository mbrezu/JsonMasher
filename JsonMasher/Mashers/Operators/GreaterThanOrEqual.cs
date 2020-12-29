using System;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Operators
{
    public class GreaterThanOrEqual
    {
        public static Builtin Builtin = Utils.MakeBinaryBuiltin(Operator);

        static Json Operator(Json t1, Json t2)
            => (t1.Type, t2.Type) switch {
                (JsonValueType.Number, JsonValueType.Number) 
                    => Json.Bool(t1.GetNumber() >= t2.GetNumber()),
                (JsonValueType.String, JsonValueType.String) 
                    => Json.Bool(t1.GetString().CompareTo(t2.GetString()) >= 0),
                _ => throw new InvalidOperationException()
            };
    }
}
