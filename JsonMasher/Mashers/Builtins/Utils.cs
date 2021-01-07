using System;
using System.Collections.Generic;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class Utils
    {
        internal static Builtin MakeBinaryBuiltin(
            Func<Json, Json, IMashContext, Json> function)
            => new Builtin(MakeBinaryFunction(function), 2);

        private static Func<List<IJsonMasherOperator>, Json, IMashContext, IEnumerable<Json>> 
        MakeBinaryFunction(Func<Json, Json, IMashContext, Json> function)
        {
            IEnumerable<Json> result(
                List<IJsonMasherOperator> mashers, Json json, IMashContext context)
            {
                if (mashers.Count != 2) {
                    throw new InvalidOperationException();
                }
                foreach (var t1 in mashers[0].Mash(json, context))
                {
                    foreach (var t2 in mashers[1].Mash(json, context))
                    {
                        context.Tick();
                        yield return function(t1, t2, context);
                    }
                }
            }
            return result;
        }

        internal static Builtin MakeUnaryBuiltin(Func<Json, IMashContext, Json> function)
            => new Builtin(MakeUnaryFunction(function), 1);

        private static Func<List<IJsonMasherOperator>, Json, IMashContext, IEnumerable<Json>> 
        MakeUnaryFunction(Func<Json, IMashContext, Json> function)
        {
            IEnumerable<Json> result(
                List<IJsonMasherOperator> mashers, Json json, IMashContext context)
            {
                if (mashers.Count != 1) {
                    throw new InvalidOperationException();
                }
                foreach (var t1 in mashers[0].Mash(json, context))
                {
                    context.Tick();
                    yield return function(t1, context);
                }
            }
            return result;
        }
    }
}
