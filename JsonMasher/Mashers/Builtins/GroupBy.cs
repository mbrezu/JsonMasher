using System;
using System.Linq;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class GroupBy
    {
        private static Builtin _builtin 
            = new Builtin((mashers, json, context) =>
        {
            if (json == null || json.Type != JsonValueType.Array)
            {
                throw context.Error(
                    $"Cannot group by over {json?.Type}, need an array.", json);
            }
            var keys = mashers[0].Mash(json, context).FirstOrDefault();
            if (keys == null || keys.Type != JsonValueType.Array)
            {
                throw context.Error(
                    $"Cannot group by with {json?.Type} as keys, need an array.", keys);
            }
            var values = json.EnumerateArray()
                .Zip(keys.EnumerateArray(), (v, k) => Tuple.Create(k, v))
                .GroupBy(
                    kv => kv.Item1,
                    (key, values) => Tuple.Create(key, values.Select(v => v.Item2)),
                    JsonComparer.Instance)
                .OrderBy(v => v.Item1, JsonComparer.Instance)
                .Select(v => Json.Array(v.Item2));
            return Json.Array(values).AsEnumerable();
        }, 1);

        public static Builtin Builtin => _builtin;
    }
}
