using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class GreaterThanOrEqual
    {
        public static Builtin Builtin = Utils.MakeBinaryBuiltin(Operator);

        static Json Operator(Json t1, Json t2, IMashContext context, IMashStack stack)
            => Json.Bool(JsonComparer.Instance.Compare(t1, t2) >= 0);
    }
}
