using System;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class Modulo
    {
        public static Builtin Builtin = Utils.MakeBinaryBuiltin(Operator);

        static Json Operator(Json t1, Json t2, IMashContext context, IMashStack stack)
            => (t1.Type, t2.Type) switch
            {
                (JsonValueType.Number, JsonValueType.Number)
                    => Json.Number((int)t1.GetNumber() % (int)t2.GetNumber()),
                _ => throw context.Error($"Can't multiply {t1.Type} and {t2.Type}.", stack, t1, t2)
            };
    }
}