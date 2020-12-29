using System;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Operators
{
    public class Not
    {
        public static Builtin Builtin = Utils.MakeUnaryBuiltin(Operator);

        static Json Operator(Json t1)
            => t1.Type switch
            {
                JsonValueType.True or JsonValueType.False => Json.Bool(!t1.GetBool()),
                _ => throw new InvalidOperationException()
            };
    }
}
