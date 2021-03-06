using System;
using System.Collections.Generic;
using System.Linq;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Builtins;

namespace JsonMasher.Mashers.Combinators
{
    public interface FunctionDescriptor {};
    public record FunctionName(string Name, int Arity): FunctionDescriptor;
    public record Builtin(
        Func<List<IJsonMasherOperator>, Json, IMashContext, IEnumerable<Json>> Function,
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

        public IEnumerable<Json> Mash(Json json, IMashContext context)
        {
            context = context.PushStack(this);
            context.Tick();
            var result = Descriptor switch {
                FunctionName name => CallFunctionName(name, json, context),
                Builtin builtin => RunBuiltin(builtin, json, context),
                _ => throw new InvalidOperationException()
            };
            return result;
        }

        private IEnumerable<Json> CallFunctionName(
            FunctionName name, Json json, IMashContext context)
            => context.GetCallable(name) switch
            {
                IJsonMasherOperator op => op.Mash(json, context),
                Function func => CallFunction(json, func, context),
                Builtin builtin => RunBuiltin(builtin, json, context),
                _ => throw new InvalidOperationException()
            };

        private IEnumerable<Json> RunBuiltin(
            Builtin builtin, Json json, IMashContext context)
        {
            if (builtin.Arity != Arguments.Count)
            {
                throw new InvalidOperationException();
            }
            return builtin.Function(Arguments, json, context);
        }

        public static FunctionCall ZeroArity(string name)
            => new FunctionCall(new FunctionName(name, 0));

        private IEnumerable<Json> CallFunction(
            Json json, Function func, IMashContext context)
        {
            if (Arguments.Count != func.Arguments.Count)
            {
                throw new InvalidOperationException();
            }
            var newContext = context.PushCallablesFrame();
            for (int i = 0; i < Arguments.Count; i++)
            {
                newContext.SetCallable(
                    func.Arguments[i],
                    ContextProvider.Wrap(context, Arguments[i]));
                                        // ^ make the arguments evaluate in the current context.
            }
            foreach (var result in func.Op.Mash(json, newContext))
            {
                yield return result;
            }
        }

        public IEnumerable<PathAndValue> GeneratePaths(
            JsonPath pathSoFar, Json json, IMashContext context)
        {
            context = context.PushStack(this);
            context.Tick();
            var result = Descriptor switch {
                FunctionName name => GeneratePathsFunctionName(pathSoFar, name, json, context),
                _ => throw context.Error("Not a path expression.")
            };
            return result;
        }

        private IEnumerable<PathAndValue> GeneratePathsFunctionName(
            JsonPath pathSoFar, FunctionName name, Json json, IMashContext context)
            => context.GetCallable(name) switch
            {
                IPathGenerator pg => pg.GeneratePaths(pathSoFar, json, context),
                Function func => GeneratePathsFunction(pathSoFar, json, func, context),
                Builtin b when b == Empty.Builtin => Enumerable.Empty<PathAndValue>(),
                _ => throw context.Error("Not a path expression.")
            };

        private IEnumerable<PathAndValue> GeneratePathsFunction(
            JsonPath pathSoFar, Json json, Function func, IMashContext context)
        {
            if (Arguments.Count != func.Arguments.Count)
            {
                throw new InvalidOperationException();
            }
            var newContext = context.PushCallablesFrame();
            for (int i = 0; i < Arguments.Count; i++)
            {
                newContext.SetCallable(
                    func.Arguments[i],
                    ContextProvider.Wrap(context, Arguments[i]));
                                        // ^ make the arguments evaluate in the current context.
            }
            if (func.Op is IPathGenerator pathGenerator)
            {
                foreach (var pathAndValue in pathGenerator.GeneratePaths(
                    pathSoFar, json, newContext))
                {
                    yield return pathAndValue;
                }
            }
            else
            {
                throw newContext.Error("Not a path expression.");
            }
        }
    }
}
