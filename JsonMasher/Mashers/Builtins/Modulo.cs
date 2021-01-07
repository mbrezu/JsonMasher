using System;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class Modulo
    {
        public static Builtin Builtin = Utils.MakeBinaryBuiltin(Operator);

        static Json Operator(Json t1, Json t2, IMashContext context)
            => (t1.Type, t2.Type) switch
            {
                (JsonValueType.Number, JsonValueType.Number)
                    => Json.Number((int)t1.GetNumber() % (int)t2.GetNumber()),
                _ => throw context.Error($"Can't multiply {t1.Type} and {t2.Type}.", t1, t2)
            };
    }
}
