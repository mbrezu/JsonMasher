using System;
using System.Collections.Generic;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class Utils
    {
        internal static Builtin MakeBinaryBuiltin(
            Func<Json, Json, IMashContext, IMashStack, Json> function)
            => new Builtin(MakeBinaryFunction(function), 2);

        private static Func<List<IJsonMasherOperator>, Json, IMashContext, IMashStack, IEnumerable<Json>> 
        MakeBinaryFunction(Func<Json, Json, IMashContext, IMashStack, Json> function)
        {
            IEnumerable<Json> result(
                List<IJsonMasherOperator> mashers, Json json, IMashContext context, IMashStack stack)
            {
                if (mashers.Count != 2) {
                    throw new InvalidOperationException();
                }
                foreach (var t1 in mashers[0].Mash(json, context, stack))
                {
                    foreach (var t2 in mashers[1].Mash(json, context, stack))
                    {
                        context.Tick(stack);
                        yield return function(t1, t2, context, stack);
                    }
                }

            }
            return result;
        }

        internal static Builtin MakeUnaryBuiltin(Func<Json, IMashContext, IMashStack, Json> function)
            => new Builtin(MakeUnaryFunction(function), 1);

        private static Func<List<IJsonMasherOperator>, Json, IMashContext, IMashStack, IEnumerable<Json>> 
        MakeUnaryFunction(Func<Json, IMashContext, IMashStack, Json> function)
        {
            IEnumerable<Json> result(
                List<IJsonMasherOperator> mashers, Json json, IMashContext context, IMashStack stack)
            {
                if (mashers.Count != 1) {
                    throw new InvalidOperationException();
                }
                foreach (var t1 in mashers[0].Mash(json, context, stack))
                {
                    context.Tick(stack);
                    yield return function(t1, context, stack);
                }
            }
            return result;
        }
    }
}
