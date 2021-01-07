using System;
using System.Collections.Generic;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class Or
    {
        private static Builtin _builtin = new Builtin(Function, 2);

        private static IEnumerable<Json> Function(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context)
        {
            if (mashers.Count != 2) {
                throw new InvalidOperationException();
            }
            foreach (var t1 in mashers[0].Mash(json, context))
            {
                if (t1.GetBool())
                {
                    context.Tick();
                    yield return Json.True;
                }
                else
                {
                    foreach (var t2 in mashers[1].Mash(json, context))
                    {
                        context.Tick();
                        yield return Json.Bool(t2.GetBool());
                    }
                }
            }
        }

        public static Builtin Builtin = _builtin;
    }
}
