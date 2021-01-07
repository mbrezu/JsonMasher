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
            JsonPath pathSoFar, Json json, IMashContext context)
        {
            var newContext = context.PushVariablesFrame().PushStack(this);
            newContext.Tick();
            if (Body is IPathGenerator pathGenerator)
            {
                foreach (var jsonValue in Value.Mash(json, newContext))
                {
                    newContext.SetVariable(Name, jsonValue);
                    foreach (var result in pathGenerator.GeneratePaths(pathSoFar, json, newContext))
                    {
                        yield return result;
                    }
                }
            }
            else
            {
                throw newContext.PushStack(Body).Error("Not a path expression.");
            }
        }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
        {
            var newContext = context.PushVariablesFrame().PushStack(this);
            newContext.Tick();
            foreach (var jsonValue in Value.Mash(json, newContext))
            {
                newContext.SetVariable(Name, jsonValue);
                foreach (var result in Body.Mash(json, newContext))
                {
                    yield return result;
                }
            }
        }
    }
}
