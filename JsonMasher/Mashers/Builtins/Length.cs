using System.Collections.Generic;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class Length
    {
        private static Builtin _builtin = new Builtin(Function, 0);

        private static IEnumerable<Json> Function(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context)
            => json.Type switch {
                JsonValueType.Array or JsonValueType.Object or JsonValueType.String
                    => Json.Number(json.GetLength()).AsEnumerable(),
                _ => Json.Number(1).AsEnumerable()
            };

        public static Builtin Builtin => _builtin;
    }
}
