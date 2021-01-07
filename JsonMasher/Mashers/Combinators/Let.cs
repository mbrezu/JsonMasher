using System.Collections.Generic;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Combinators
{
    public class Let : IJsonMasherOperator, IPathGenerator
    {
        public string Name { get; init; }
        public IJsonMasherOperator Value { get; init; }
        public IJsonMasherOperator Body { get; init; }

        public IEnumerable<PathAndValue> GeneratePaths(
            JsonPath pathSoFar, Json json, IMashContext context, IMashStack stack)
        {
            var newContext = context.PushVariablesFrame();
            var newStack = stack.Push(this);
            newContext.Tick(newStack);
            if (Body is IPathGenerator pathGenerator)
            {
                foreach (var jsonValue in Value.Mash(json, newContext, newStack))
                {
                    newContext.SetVariable(Name, jsonValue);
                    foreach (var result in pathGenerator.GeneratePaths(pathSoFar, json, newContext, newStack))
                    {
                        yield return result;
                    }
                }
            }
            else
            {
                throw newContext.Error("Not a path expression.", newStack.Push(Body));
            }
        }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            var newContext = context.PushVariablesFrame();
            var newStack = stack.Push(this);
            newContext.Tick(newStack);
            foreach (var jsonValue in Value.Mash(json, newContext, newStack))
            {
                newContext.SetVariable(Name, jsonValue);
                foreach (var result in Body.Mash(json, newContext, newStack))
                {
                    yield return result;
                }
            }
        }
    }
}
