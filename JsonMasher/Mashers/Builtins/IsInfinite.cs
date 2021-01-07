using System.Collections.Generic;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class IsInfinite
    {
        private static Builtin _builtin = new Builtin(Function, 0);

        private static IEnumerable<Json> Function(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context)
            => Json.Bool(json.Type == JsonValueType.Number && double.IsInfinity(json.GetNumber()))
                .AsEnumerable();

        public static Builtin Builtin => _builtin;
    }
}
