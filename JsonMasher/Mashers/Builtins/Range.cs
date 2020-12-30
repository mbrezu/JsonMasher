using System.Collections.Generic;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class Range
    {
        private static Builtin _builtin = new Builtin(Function , 1);

        private static IEnumerable<Json> Function(
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

        public static Builtin Builtin => _builtin;
    }
}
