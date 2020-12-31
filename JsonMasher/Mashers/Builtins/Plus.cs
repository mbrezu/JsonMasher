using System.Linq;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class Plus
    {
        public static Builtin Builtin = Utils.MakeBinaryBuiltin(Operator);
        
        static Json Operator(Json t1, Json t2, IMashContext context, IMashStack stack)
            => (t1.Type, t2.Type) switch
            {
                (JsonValueType.Number, JsonValueType.Number)
                    => Json.Number(t1.GetNumber() + t2.GetNumber()),
                (JsonValueType.String, JsonValueType.String)
                    => Json.String(t1.GetString() + t2.GetString()),
                (JsonValueType.Array, JsonValueType.Array)
                    => Json.Array(t1.EnumerateArray().Concat(t2.EnumerateArray())),
                (JsonValueType.Object, JsonValueType.Object)
                    => Json.Object(t1.EnumerateObject().Concat(t2.EnumerateObject())),
                _ => throw context.Error($"Can't add {t1.Type} and {t2.Type}.", stack, t1, t2)
            };
    }
}
