using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class GreaterThan
    {
        public static Builtin Builtin = Utils.MakeBinaryBuiltin(Operator);

        static Json Operator(Json t1, Json t2, IMashContext context, IMashStack stack)
            => (t1.Type, t2.Type) switch {
                (JsonValueType.Number, JsonValueType.Number) 
                    => Json.Bool(t1.GetNumber() > t2.GetNumber()),
                (JsonValueType.String, JsonValueType.String) 
                    => Json.Bool(t1.GetString().CompareTo(t2.GetString()) > 0),
                _ => throw context.Error($"Can't compare {t1.Type} and {t2.Type}.", stack)
            };
    }
}
