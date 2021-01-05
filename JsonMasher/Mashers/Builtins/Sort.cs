using System;
using System.Collections.Generic;
using System.Linq;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class Sort
    {
        private static Builtin _builtin_0 = new Builtin(Function_0, 0);

        private static IEnumerable<Json> Function_0(
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

        public static Builtin Builtin_0 => _builtin_0;

        private static Builtin _builtin_1 = new Builtin(Function_1, 1);

        private static IEnumerable<Json> Function_1(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context, IMashStack stack)
            {
                if (json.Type == JsonValueType.Array)
                {
                    var masher = mashers.First();
                    var values = json
                        .EnumerateArray()
                        .Select(e => Tuple.Create(
                            e, 
                            masher.Mash(e, context, stack).FirstOrDefault() ?? e))
                        .OrderBy(x => x.Item2, JsonComparer.Instance)
                        .Select(x => x.Item1);
                    return Json.Array(values).AsEnumerable();
                }
                else
                {
                    throw context.Error($"Can't sort a {json.Type}.", stack, json);
                }
            }

        public static Builtin Builtin_1 => _builtin_1;
    }
}
