using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class Divide
    {
        public static Builtin Builtin = Utils.MakeBinaryBuiltin(Operator);

        static Json Operator(Json t1, Json t2, IMashContext context, IMashStack stack)
            => (t1.Type, t2.Type) switch
            {
                (JsonValueType.Number, JsonValueType.Number)
                    => Json.Number(t1.GetNumber() / t2.GetNumber()),
                _ => throw context.Error($"Can't divide {t1.Type} and {t2.Type}.", stack)
            };
    }
}
