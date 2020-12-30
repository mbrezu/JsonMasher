using System.Collections.Generic;
using System.Linq;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class Limit
    {
        private static Builtin _builtin = new Builtin(Function, 2);

        private static IEnumerable<Json> Function(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context)
        {
            foreach (var limitValue in mashers[0].Mash(json, context))
            {
                int limit = (int)limitValue.GetNumber();
                foreach (var result in mashers[1].Mash(json, context).Take(limit))
                {
                    yield return result;
                }
            }
        }

        public static Builtin Builtin => _builtin;
    }
}
