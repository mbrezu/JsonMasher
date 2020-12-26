using System;
using System.Collections.Generic;

namespace JsonMasher.Combinators
{
    public class FunctionCall : IJsonMasherOperator
    {
        public string Name { get; init; }
        public List<IJsonMasherOperator> Arguments { get; init; }
        public IEnumerable<Json> Mash(Json json, IMashContext context)
            => context.GetCallable(Name) switch
            {
                Thunk thunk => thunk.Op.Mash(json, context),
                Function func => Call(json, func, context),
                _ => throw new InvalidOperationException()
            };

        private static List<IJsonMasherOperator> _noArgs = new();
        public static FunctionCall ZeroArity(string name)
            => new FunctionCall {
                Name = name,
                Arguments = _noArgs
            };

        private IEnumerable<Json> Call(Json json, Function func, IMashContext context)
        {
            if (Arguments.Count != func.Arguments.Count)
            {
                throw new InvalidOperationException();
            }
            context.PushEnvironmentFrame();
            for (int i = 0; i < Arguments.Count; i++)
            {
                context.SetCallable(func.Arguments[i], new Thunk(Arguments[i]));
            }
            foreach (var result in func.Op.Mash(json, context))
            {
                yield return result;
            }
            context.PopEnvironmentFrame();
        }
    }
}
