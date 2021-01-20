using System.Collections.Generic;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators.LetMatchers;

namespace JsonMasher.Mashers.Combinators
{
    public class Let : IJsonMasherOperator, IPathGenerator
    {
        public IMatcher Matcher { get; init; }
        public IJsonMasherOperator Value { get; init; }
        public IJsonMasherOperator Body { get; init; }

        public IEnumerable<PathAndValue> GeneratePaths(
            JsonPath pathSoFar, Json json, IMashContext context)
        {
            context = context.PushStack(this);
            context.Tick();
            if (Body is IPathGenerator pathGenerator)
            {
                foreach (var jsonValue in Value.Mash(json, context))
                {
                    foreach (var matchSet in Matcher.GetMatches(jsonValue, context))
                    {
                        var newContext = context.PushVariablesFrame();
                        foreach (var match in matchSet.Matches)
                        {
                            newContext.SetVariable(match.Name, match.Value);
                        }
                        foreach (var result in pathGenerator.GeneratePaths(pathSoFar, json, newContext))
                        {
                            yield return result;
                        }
                    }
                }
            }
            else
            {
                throw context.PushStack(Body).Error("Not a path expression.");
            }
        }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
        {
            context = context.PushStack(this);
            context.Tick();
            foreach (var jsonValue in Value.Mash(json, context))
            {
                foreach (var matchSet in Matcher.GetMatches(jsonValue, context))
                {
                    var newContext = context.PushVariablesFrame();
                    foreach (var match in matchSet.Matches)
                    {
                        newContext.SetVariable(match.Name, match.Value);
                    }
                    foreach (var result in Body.Mash(json, newContext))
                    {
                        yield return result;
                    }
                }
            }
        }
    }
}
