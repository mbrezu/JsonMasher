using System;
using System.Collections.Generic;
using System.Linq;
using JsonMasher.Mashers.Builtins;

namespace JsonMasher.Mashers.Combinators
{
    public interface FunctionDescriptor {};
    public record FunctionName(string Name, int Arity): FunctionDescriptor;
    public record Builtin(
        Func<List<IJsonMasherOperator>, Json, IMashContext, IMashStack, IEnumerable<Json>> Function,
        int Arity)
    : FunctionDescriptor, Callable;

    public class FunctionCall : IJsonMasherOperator, IJsonZipper
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

        public ZipStage ZipDown(Json json, IMashContext context, IMashStack stack)
        {
            var newStack = stack.Push(this);
            if (Descriptor is FunctionName name)
            {
                return ZipDownFunctionName(name, json, context, newStack);
            }
            else
            {
                throw context.Error($"Not a path expression.", newStack);
            }
        }

        private IEnumerable<Json> CallFunctionName(
            FunctionName name, Json json, IMashContext context, IMashStack stack)
            => context.GetCallable(name, stack) switch
            {
                IJsonMasherOperator op => op.Mash(json, context, stack),
                Function func => CallFunction(json, func, context, stack),
                Builtin builtin => RunBuiltin(builtin, json, context, stack),
                _ => throw new InvalidOperationException()
            };

        private ZipStage ZipDownFunctionName(
            FunctionName name, Json json, IMashContext context, IMashStack stack)
            => context.GetCallable(name, stack) switch
            {
                IJsonZipper op => op.ZipDown(json, context, stack),
                Function func => ZipDownFunction(json, func, context, stack),
                Builtin b when b == Empty.Builtin
                    => new ZipStage(_ => json, Enumerable.Empty<Json>()),
                _ => throw context.Error($"Not a path expression.", stack)
            };

        private ZipStage ZipDownFunction(
            Json json, Function func, IMashContext context, IMashStack stack)
        {
            if (func.Op is IJsonZipper zipper)
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
                var result = zipper.ZipDown(json, context, stack);
                context.PopEnvironmentFrame();
                return result;
            }
            else
            {
                throw context.Error($"Not a path expression.", stack);
            }
        }

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
