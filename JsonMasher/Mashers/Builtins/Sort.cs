using System.Collections.Generic;
using System.Linq;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class Sort
    {
        private static Builtin _builtin = new Builtin(Function , 0);

        private static IEnumerable<Json> Function(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context, IMashStack stack)
            {
                if (json.Type == JsonValueType.Array)
                {
                    return Json.Array(json.EnumerateArray().OrderBy(x => x, JsonComparer.Instance))
                        .AsEnumerable();
                }
                else
                {
                    throw context.Error($"Can't sort a {json.Type}.", stack, json);
                }
            }

        public static Builtin Builtin => _builtin;
    }
}
