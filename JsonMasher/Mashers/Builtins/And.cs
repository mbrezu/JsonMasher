using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class And
    {
        public static Builtin Builtin = Utils.MakeBinaryBuiltin(Operator);

        static Json Operator(Json t1, Json t2, IMashContext context, IMashStack stack)
            => (t1.Type, t2.Type) switch
            {
                (JsonValueType.True or JsonValueType.False, JsonValueType.True or JsonValueType.False)
                    => Json.Bool(t1.GetBool() && t2.GetBool()),
                _ => throw context.Error(
                    $"Can't 'and' {t1.Type} and {t2.Type} (need two booleans).", stack, t1, t2)
            };
    }
}
