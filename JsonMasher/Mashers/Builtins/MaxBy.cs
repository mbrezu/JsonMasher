using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class MaxBy
    {
        private static Builtin _builtin 
            = new Builtin(
                (mashers, json, context) => MinBy.ExtractMinMax(mashers, json, context, false),
                1);

        public static Builtin Builtin => _builtin;
    }
}
