using System.Collections.Generic;
using System.Linq;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class First
    {
        private static Builtin _builtin = new Builtin(Function, 1);

        private static IEnumerable<Json> Function(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context, IMashStack stack)
            => mashers[0].Mash(json, context, stack).Take(1);

        public static Builtin Builtin => _builtin;
    }
}
