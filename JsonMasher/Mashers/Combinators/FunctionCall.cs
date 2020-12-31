using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Mashers.Combinators
{
    public interface FunctionDescriptor {};
    public record FunctionName(string Name, int Arity): FunctionDescriptor;
    public record Builtin(
        Func<List<IJsonMasherOperator>, Json, IMashContext, IMashStack, IEnumerable<Json>> Function,
        int Arity)
    : FunctionDescriptor, Callable;

    public class FunctionCall : IJsonMasherOperator
    {
        public FunctionDescriptor Descriptor { get; private set; }
        public List<IJsonMasherOperator> Arguments { get; private set; }

        public FunctionCall(FunctionDescriptor descriptor, params IJsonMasherOperator[] arguments)
        {
            Descriptor = descriptor;
            Arguments = arguments.ToList();
        }

        public FunctionCall(FunctionDescriptor descriptor, List<IJsonMasherOperator> arguments)
        {
            Descriptor = descriptor;
            Arguments = arguments;
        }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            var newStack = stack.Push(this);
            context.Tick(stack);
            var result = Descriptor switch {
                FunctionName name => CallFunctionName(name, json, context, newStack),
                Builtin builtin => RunBuiltin(builtin, json, context, newStack),
                _ => throw new InvalidOperationException()
            };
            return result;
        }

        private IEnumerable<Json> CallFunctionName(
            FunctionName name, Json json, IMashContext context, IMashStack stack)
            => context.GetCallable(name) switch
            {
                IJsonMasherOperator op => op.Mash(json, context, stack),
                Function func => CallFunction(json, func, context, stack),
                Builtin builtin => RunBuiltin(builtin, json, context, stack),
                _ => throw new InvalidOperationException()
            };

        private IEnumerable<Json> RunBuiltin(
            Builtin builtin, Json json, IMashContext context, IMashStack stack)
        {
            if (builtin.Arity != Arguments.Count)
            {
                throw new InvalidOperationException();
            }
            return builtin.Function(Arguments, json, context, stack);
        }

        public static FunctionCall ZeroArity(string name)
            => new FunctionCall(new FunctionName(name, 0));

        private IEnumerable<Json> CallFunction(
            Json json, Function func, IMashContext context, IMashStack stack)
        {
            if (Arguments.Count != func.Arguments.Count)
            {
                throw new InvalidOperationException();
            }
            context.PushEnvironmentFrame();
            for (int i = 0; i < Arguments.Count; i++)
            {
                context.SetCallable(func.Arguments[i], Arguments[i]);
            }
            foreach (var result in func.Op.Mash(json, context, stack))
            {
                yield return result;
            }
            context.PopEnvironmentFrame();
        }
    }
}
