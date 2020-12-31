using System.Collections.Generic;
using System.Linq;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class Limit
    {
        private static Builtin _builtin = new Builtin(Function, 2);

        private static IEnumerable<Json> Function(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context, IMashStack stack)
        {
            foreach (var limitValue in mashers[0].Mash(json, context, stack))
            {
                if (limitValue.Type != JsonValueType.Number)
                {
                    throw context.Error($"Can't use {limitValue.Type} as limit.", stack, limitValue);
                }
                int limit = (int)limitValue.GetNumber();
                foreach (var result in mashers[1].Mash(json, context, stack).Take(limit))
                {
                    yield return result;
                }
            }
        }

        public static Builtin Builtin => _builtin;
    }
}
