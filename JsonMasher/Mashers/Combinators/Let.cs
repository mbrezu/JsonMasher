using System.Collections.Generic;
using JsonMasher.JsonRepresentation;
using System.Diagnostics;
using System;

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
            context.PushVariablesFrame();
            var newStack = stack.Push(this);
            context.Tick(newStack);
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
                context.PopVariablesFrame();
            }
            else
            {
                throw context.Error("Not a path expression.", newStack.Push(Body));
            }
        }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            context.PushVariablesFrame();
            var newStack = stack.Push(this);
            context.Tick(newStack);
            var resultList = new List<Json>();
            foreach (var jsonValue in Value.Mash(json, context, newStack))
            {
                context.SetVariable(Name, jsonValue);
                foreach (var result in Body.Mash(json, context, newStack))
                {
                    resultList.Add(result);
                }
            }
            context.PopVariablesFrame();
            return resultList;
        }
    }
}
