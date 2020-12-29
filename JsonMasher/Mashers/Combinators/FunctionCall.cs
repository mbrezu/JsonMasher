using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Mashers.Combinators
{
    public record FunctionDescriptor();
    public record FunctionName(string Name): FunctionDescriptor;
    public record Builtin(
        Func<List<IJsonMasherOperator>, Json, IMashContext, IEnumerable<Json>> Function,
        int Arity)
    : FunctionDescriptor;

    public class FunctionCall : IJsonMasherOperator
    {
        public FunctionDescriptor Descriptor { get; private set; }
        public List<Thunk> ThunkedArguments { get; private set; }
        public List<IJsonMasherOperator> Arguments { get; private set; }

        public FunctionCall(FunctionDescriptor descriptor, params IJsonMasherOperator[] arguments)
        {
            Descriptor = descriptor;
            Arguments = arguments.ToList();
            ThunkedArguments = arguments.Select(a => new Thunk(a)).ToList();
        }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
            => Descriptor switch {
                FunctionName name => GetThenCallCallable(name.Name, json, context),
                Builtin builtin => RunBuiltin(builtin, json, context),
                _ => throw new InvalidOperationException()
            };

        private IEnumerable<Json> GetThenCallCallable(string name, Json json, IMashContext context)
            => context.GetCallable(name) switch
            {
                Thunk thunk => thunk.Op.Mash(json, context),
                Function func => Call(json, func, context),
                _ => throw new InvalidOperationException()
            };

        private IEnumerable<Json> RunBuiltin(Builtin builtin, Json json, IMashContext context)
            => builtin.Function(Arguments, json, context);

        public static FunctionCall ZeroArity(string name)
            => new FunctionCall(new FunctionName(name));

        private IEnumerable<Json> Call(Json json, Function func, IMashContext context)
        {
            if (ThunkedArguments.Count != func.Arguments.Count)
            {
                throw new InvalidOperationException();
            }
            context.PushEnvironmentFrame();
            for (int i = 0; i < ThunkedArguments.Count; i++)
            {
                context.SetCallable(func.Arguments[i], ThunkedArguments[i]);
            }
            foreach (var result in func.Op.Mash(json, context))
            {
                yield return result;
            }
            context.PopEnvironmentFrame();
        }
    }
}
