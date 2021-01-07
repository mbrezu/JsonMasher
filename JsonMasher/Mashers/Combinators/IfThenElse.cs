using System.Collections.Generic;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Combinators
{
    public class IfThenElse : IJsonMasherOperator, IPathGenerator
    {
        public IJsonMasherOperator Cond { get; init; }
        public IJsonMasherOperator Then { get; init; }
        public IJsonMasherOperator Else { get; init; }

        public IEnumerable<PathAndValue> GeneratePaths(
            JsonPath pathSoFar, Json json, IMashContext context)
        {
            context = context.PushStack(this);
            context.Tick();
            var condSequence = Cond.Mash(json, context);
            foreach (var condValue in condSequence)
            {
                if (condValue.GetBool())
                {
                    if (Then is IPathGenerator pathGenerator)
                    {
                        foreach (var pathAndValue in pathGenerator.GeneratePaths(
                            pathSoFar, json, context))
                        {
                            yield return pathAndValue;
                        }
                    }
                    else
                    {
                        throw context.PushStack(Then).Error("Not a path expression.");
                    }
                }
                else
                {
                    if (Else is IPathGenerator pathGenerator)
                    {
                        foreach (var pathAndValue in pathGenerator.GeneratePaths(
                            pathSoFar, json, context))
                        {
                            yield return pathAndValue;
                        }
                    }
                    else
                    {
                        throw context.PushStack(Else).Error("Not a path expression.");
                    }
                }
            }
        }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
        {
            context = context.PushStack(this);
            context.Tick();
            var condSequence = Cond.Mash(json, context);
            foreach (var condValue in condSequence)
            {
                var resultSequence = condValue.GetBool()
                    ? Then.Mash(json, context)
                    : Else.Mash(json, context);
                foreach (var result in resultSequence)
                {
                    yield return result;
                }
            }
        }
    }
}
