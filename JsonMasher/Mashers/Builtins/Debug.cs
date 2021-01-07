using System.Collections.Generic;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public static class Debug
    {
        private static Builtin _builtin = new Builtin(Function, 0);

        private static IEnumerable<Json> Function(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context)
        {
            context.Tick();
            context.LogValue(Json.ArrayParams(Json.String("DEBUG"), json));
            yield return json;
        }

        public static Builtin Builtin => _builtin;
    }
}
