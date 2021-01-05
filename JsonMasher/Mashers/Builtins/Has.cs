using System.Collections.Generic;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class Has
    {
        private static Builtin _builtin = new Builtin(Function, 1);

        private static IEnumerable<Json> Function(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context, IMashStack stack)
        {
            foreach (var value in mashers[0].Mash(json, context, stack))
            {
                yield return HasIndex(json, value, context, stack);
            }
        }

        public static Json HasIndex(Json json, Json index, IMashContext context, IMashStack stack)
        {
            return (json.Type, index.Type) switch
            {
                (JsonValueType.Array, JsonValueType.Number)
                    => Json.Bool(json.ContainsKey((int)index.GetNumber())),
                (JsonValueType.Object, JsonValueType.String)
                    => Json.Bool(json.ContainsKey(index.GetString())),
                _ => throw context.Error(
                    $"Can't index {json.Type} with {index.Type}.", stack, json, index)
            };
        }

        public static Builtin Builtin => _builtin;
    }
}
