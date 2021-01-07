using System.Collections.Generic;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class Has
    {
        private static Builtin _builtin = new Builtin(Function, 1);

        private static IEnumerable<Json> Function(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context)
        {
            foreach (var value in mashers[0].Mash(json, context))
            {
                yield return HasIndex(json, value, context);
            }
        }

        public static Json HasIndex(Json json, Json index, IMashContext context)
        {
            return (json.Type, index.Type) switch
            {
                (JsonValueType.Array, JsonValueType.Number)
                    => Json.Bool(json.ContainsKey((int)index.GetNumber())),
                (JsonValueType.Object, JsonValueType.String)
                    => Json.Bool(json.ContainsKey(index.GetString())),
                _ => throw context.Error(
                    $"Can't index {json.Type} with {index.Type}.", json, index)
            };
        }

        public static Builtin Builtin => _builtin;
    }
}
