using System;
using System.Collections.Generic;
using System.Linq;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class MinBy
    {
        private static Builtin _builtin 
            = new Builtin(
                (mashers, json, context) => MinBy.ExtractMinMax(mashers, json, context, true),
                1);

        public static IEnumerable<Json> ExtractMinMax(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context, bool min)
        {
            if (json == null || json.Type != JsonValueType.Array)
            {
                throw context.Error(
                    $"Cannot find minimum for {json?.Type}, need an array.", json);
            }
            var keys = mashers[0].Mash(json, context).FirstOrDefault();
            if (keys == null || keys.Type != JsonValueType.Array)
            {
                throw context.Error(
                    $"Cannot find minimum with {json?.Type} as keys, need an array.", keys);
            }
            var query = json.EnumerateArray()
                .Zip(keys.EnumerateArray(), (v, k) => Tuple.Create(k, v));
            if (min)
            {
                query = query.OrderBy(x => x.Item1, JsonComparer.Instance); // there is no MinBy...
            }
            else
            {
                query = query.OrderByDescending(x => x.Item1, JsonComparer.Instance); // there is no MaxBy...
            }
            var value = query.Take(1).Select(x => x.Item2).FirstOrDefault();
            if (value == null)
            {
                return Json.Null.AsEnumerable();
            }
            else
            {
                return value.AsEnumerable();
            }
        }

        public static Builtin Builtin => _builtin;
    }
}
