using System.Collections.Generic;

namespace JsonMasher.Mashers.Combinators
{
    public class IfThenElse : IJsonMasherOperator, IPathGenerator
    {
        public IJsonMasherOperator Cond { get; init; }
        public IJsonMasherOperator Then { get; init; }
        public IJsonMasherOperator Else { get; init; }

        public IEnumerable<PathAndValue> GeneratePaths(
            JsonPath pathSoFar, Json json, IMashContext context, IMashStack stack)
        {
            var newStack = stack.Push(this);
            context.Tick(newStack);
            var condSequence = Cond.Mash(json, context, newStack);
            foreach (var condValue in condSequence)
            {
                if (condValue.GetBool())
                {
                    if (Then is IPathGenerator pathGenerator)
                    {
                        foreach (var pathAndValue in pathGenerator.GeneratePaths(
                            pathSoFar, json, context, stack))
                        {
                            yield return pathAndValue;
                        }
                    }
                    else
                    {
                        throw context.Error("Not a path expression.", newStack.Push(Then));
                    }
                }
                else
                {
                    if (Else is IPathGenerator pathGenerator)
                    {
                        foreach (var pathAndValue in pathGenerator.GeneratePaths(
                            pathSoFar, json, context, stack))
                        {
                            yield return pathAndValue;
                        }
                    }
                    else
                    {
                        throw context.Error("Not a path expression.", newStack.Push(Else));
                    }
                }
            }
        }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            var newStack = stack.Push(this);
            context.Tick(newStack);
            var condSequence = Cond.Mash(json, context, newStack);
            foreach (var condValue in condSequence)
            {
                var resultSequence = condValue.GetBool()
                    ? Then.Mash(json, context, newStack)
                    : Else.Mash(json, context, newStack);
                foreach (var result in resultSequence)
                {
                    yield return result;
                }
            }
        }
    }
}
