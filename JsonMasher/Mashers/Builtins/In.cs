using System.Collections.Generic;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class In
    {
        private static Builtin _builtin = new Builtin(Function, 1);

        private static IEnumerable<Json> Function(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context)
        {
            foreach (var value in mashers[0].Mash(json, context))
            {
                yield return Has.HasIndex(value, json, context);
            }
        }

        public static Builtin Builtin => _builtin;
    }
}
