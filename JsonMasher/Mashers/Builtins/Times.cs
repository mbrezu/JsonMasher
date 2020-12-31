using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class Times
    {
        public static Builtin Builtin = Utils.MakeBinaryBuiltin(Operator);

        static Json Operator(Json t1, Json t2, IMashContext context, IMashStack stack)
            => (t1.Type, t2.Type) switch
            {
                (JsonValueType.Number, JsonValueType.Number)
                    => Json.Number(t1.GetNumber() * t2.GetNumber()),
                (JsonValueType.String, JsonValueType.Number)
                    => Json.String(t1.GetString().Repeat((int)t2.GetNumber())),
                (JsonValueType.Number, JsonValueType.String)
                    => Json.String(t2.GetString().Repeat((int)t1.GetNumber())),
                _ => throw context.Error($"Can't multiply {t1.Type} and {t2.Type}.", stack)
            };
    }
}
