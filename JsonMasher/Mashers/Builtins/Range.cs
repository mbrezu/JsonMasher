using System.Collections.Generic;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class Range
    {
        private static Builtin _builtin_1 = new Builtin(Function_1 , 1);

        private static IEnumerable<Json> Function_1(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context)
        {
            foreach (var maxValue in mashers[0].Mash(json, context))
            {
                for (int i = 0; i < maxValue.GetNumber(); i++)
                {
                    yield return Json.Number(i);
                }
            }
        }

        private static Builtin _builtin_2 = new Builtin(Function_2 , 2);

        private static IEnumerable<Json> Function_2(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context)
        {
            foreach (var startValue in mashers[0].Mash(json, context))
            {
                foreach (var maxValue in mashers[1].Mash(json, context))
                {
                    int start = (int)startValue.GetNumber();
                    int max = (int)maxValue.GetNumber();
                    for (int i = start; i < max; i++)
                    {
                        yield return Json.Number(i);
                    }
                }
            }
        }

        public static Builtin Builtin_1 => _builtin_1;
        public static Builtin Builtin_2 => _builtin_2;
    }
}
