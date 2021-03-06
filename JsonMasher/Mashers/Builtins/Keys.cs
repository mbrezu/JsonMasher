using System.Collections.Generic;
using System.Linq;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class Keys
    {
        private static Builtin _builtin = new Builtin(Function, 0);

        private static IEnumerable<Json> Function(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context)
            => json.Type switch
            {
                JsonValueType.Object
                    => Json.Array(ObjectKeys(json)).AsEnumerable(),
                JsonValueType.Array
                    => Json.Array(ArrayKeys(json)).AsEnumerable(),
                _ => throw context.Error($"{json.Type} has no keys.", json)
            };

        private static IEnumerable<Json> ObjectKeys(Json json)
            => json.EnumerateObject().Select(kv => Json.String(kv.Key));

        private static IEnumerable<Json> ArrayKeys(Json json)
            => Enumerable.Range(0, json.GetLength()).Select(n => Json.Number(n));

        public static Builtin Builtin => _builtin;
    }
}
