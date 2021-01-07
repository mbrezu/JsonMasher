using System;
using System.Collections.Generic;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class JsonType
    {
        private static Builtin _builtin = new Builtin(Function, 0);

        private static IEnumerable<Json> Function(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context, IMashStack stack)
            => (json.Type switch {
                JsonValueType.Undefined => Json.String("undefined"),
                JsonValueType.Null => Json.String("null"),
                JsonValueType.True or JsonValueType.False => Json.String("boolean"),
                JsonValueType.Number => Json.String("number"),
                JsonValueType.String => Json.String("string"),
                JsonValueType.Array => Json.String("array"),
                JsonValueType.Object => Json.String("object"),
                _ => throw new InvalidOperationException()
            }).AsEnumerable();

        public static Builtin Builtin => _builtin;
    }
}
