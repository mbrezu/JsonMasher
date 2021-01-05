using System.Collections.Generic;

namespace JsonMasher.Mashers.Combinators
{
    public class Let : IJsonMasherOperator, IPathGenerator
    {
        public string Name { get; init; }
        public IJsonMasherOperator Value { get; init; }
        public IJsonMasherOperator Body { get; init; }

        public IEnumerable<PathAndValue> GeneratePaths(
            Path pathSoFar, Json json, IMashContext context, IMashStack stack)
        {
            context.PushEnvironmentFrame();
            var newStack = stack.Push(this);
            context.Tick(stack);
            if (Body is IPathGenerator pathGenerator)
            {
                foreach (var jsonValue in Value.Mash(json, context, newStack))
                {
                    context.SetVariable(Name, jsonValue);
                    foreach (var result in pathGenerator.GeneratePaths(pathSoFar, json, context, newStack))
                    {
                        yield return result;
                    }
                }
                context.PopEnvironmentFrame();
            }
            else
            {
                throw context.Error("Not a path expression.", newStack.Push(Body));
            }
        }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            context.PushEnvironmentFrame();
            var newStack = stack.Push(this);
            context.Tick(stack);
            foreach (var jsonValue in Value.Mash(json, context, newStack))
            {
                context.SetVariable(Name, jsonValue);
                foreach (var result in Body.Mash(json, context, newStack))
                {
                    yield return result;
                }
            }
            context.PopEnvironmentFrame();
        }
    }
}
