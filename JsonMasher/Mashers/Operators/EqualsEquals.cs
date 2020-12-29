using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Operators
{
    public class EqualsEquals
    {
        public static Builtin Builtin = Utils.MakeBinaryBuiltin(Operator);

        static Json Operator(Json t1, Json t2)
            => t1.DeepEqual(t2) ? Json.True : Json.False;
    }
}
