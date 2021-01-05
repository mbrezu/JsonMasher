using System.Collections.Generic;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class In
    {
        private static Builtin _builtin = new Builtin(Function, 1);

        private static IEnumerable<Json> Function(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context, IMashStack stack)
        {
            foreach (var value in mashers[0].Mash(json, context, stack))
            {
                yield return Has.HasIndex(value, json, context, stack);
            }
        }

        public static Builtin Builtin => _builtin;
    }
}
