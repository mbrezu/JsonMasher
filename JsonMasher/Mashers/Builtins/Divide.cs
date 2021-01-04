using System.Linq;
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
                (JsonValueType.String, JsonValueType.String)
                    => Json.Array(t1.GetString().Split(t2.GetString()).Select(e => Json.String(e))),
                _ => throw context.Error($"Can't divide {t1.Type} and {t2.Type}.", stack, t1, t2)
            };
    }
}
