using System.Collections.Generic;
using JsonMasher.JsonRepresentation;
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
                    context.Tick();
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
                        context.Tick();
                        yield return Json.Number(i);
                    }
                }
            }
        }

        private static Builtin _builtin_3 = new Builtin(Function_3 , 3);

        private static IEnumerable<Json> Function_3(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context)
        {
            foreach (var startValue in mashers[0].Mash(json, context))
            {
                foreach (var maxValue in mashers[1].Mash(json, context))
                {
                    foreach (var stepValue in mashers[2].Mash(json, context))
                    {
                        double start = startValue.GetNumber();
                        double max = maxValue.GetNumber();
                        double step = stepValue.GetNumber();
                        for (double i = start; i < max; i += step)
                        {
                            context.Tick();
                            yield return Json.Number(i);
                        }
                    }
                }
            }
        }

        public static Builtin Builtin_1 => _builtin_1;
        public static Builtin Builtin_2 => _builtin_2;
        public static Builtin Builtin_3 => _builtin_3;
    }
}
