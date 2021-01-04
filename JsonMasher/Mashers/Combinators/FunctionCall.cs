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

    public class FunctionCall : IJsonMasherOperator, IPathGenerator
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
            => context.GetCallable(name, stack) switch
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

        public IEnumerable<PathAndValue> GeneratePaths(
            Path pathSoFar, Json json, IMashContext context, IMashStack stack)
        {
            var newStack = stack.Push(this);
            context.Tick(stack);
            var result = Descriptor switch {
                FunctionName name => GeneratePathsFunctionName(
                    pathSoFar, name, json, context, newStack),
                Builtin b when b == Recurse.Builtin
                    => Recurse.GeneratePaths(pathSoFar, json),
                _ => throw context.Error("Not a path expression.", newStack)
            };
            return result;
        }

        private IEnumerable<PathAndValue> GeneratePathsFunctionName(
            Path pathSoFar, FunctionName name, Json json, IMashContext context, IMashStack stack)
            => context.GetCallable(name, stack) switch
            {
                IPathGenerator pg => pg.GeneratePaths(pathSoFar, json, context, stack),
                Function func => GeneratePathsFunction(pathSoFar, json, func, context, stack),
                Builtin b when b == Empty.Builtin => Enumerable.Empty<PathAndValue>(),
                _ => throw context.Error("Not a path expression.", stack)
            };

        private IEnumerable<PathAndValue> GeneratePathsFunction(
            Path pathSoFar, Json json, Function func, IMashContext context, IMashStack stack)
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
            if (func.Op is IPathGenerator pathGenerator)
            {
                foreach (var pathAndValue in pathGenerator.GeneratePaths(
                    pathSoFar, json, context, stack))
                {
                    yield return pathAndValue;
                }
            }
            else
            {
                throw context.Error("Not a path expression.", stack);
            }
            context.PopEnvironmentFrame();
        }
    }
}
