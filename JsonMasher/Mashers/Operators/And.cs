using System;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Operators
{
    public class And
    {
        public static Builtin Builtin = Utils.MakeBinaryBuiltin(Operator);

        static Json Operator(Json t1, Json t2)
            => (t1.Type, t2.Type) switch
            {
                (JsonValueType.True or JsonValueType.False, JsonValueType.True or JsonValueType.False)
                    => Json.Bool(t1.GetBool() && t2.GetBool()),
                _ => throw new InvalidOperationException()
            };
    }
}
