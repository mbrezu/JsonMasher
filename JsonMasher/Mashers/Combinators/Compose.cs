using System.Collections.Generic;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Combinators
{
    public class Compose : IJsonMasherOperator, IPathGenerator
    {
        public IJsonMasherOperator First { get; init; }
        public IJsonMasherOperator Second { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            var newStack = stack.Push(this);
            context.Tick(stack);
            foreach (var temp in First.Mash(json, context, newStack))
            {
                foreach (var result in Second.Mash(temp, context, newStack))
                {
                    yield return result;
                }
            }
        }

        public static IJsonMasherOperator AllParams(params IJsonMasherOperator[] mashers)
            => All(mashers);

        public static IJsonMasherOperator All(IEnumerable<IJsonMasherOperator> mashers)
            => mashers.Fold((m1, m2) => new Compose { First = m1, Second = m2 });

        public IEnumerable<PathAndValue> GeneratePaths(
            JsonPath pathSoFar, Json json, IMashContext context, IMashStack stack)
        {
            var newStack = stack.Push(this);
            context.Tick(stack);
            if (First is IPathGenerator pg1)
            {
                if (Second is IPathGenerator pg2)
                {
                    foreach (var pathAndValue1 in pg1.GeneratePaths(pathSoFar, json, context, stack))
                    {
                        foreach (var pathAndValue2 in pg2.GeneratePaths(
                            pathAndValue1.Path, pathAndValue1.Value, context, stack))
                        {
                            yield return pathAndValue2;
                        }
                    }
                }
                else
                {
                    throw context.Error("Not a path expression.", newStack.Push(Second));
                }
            }
            else
            {
                throw context.Error("Not a path expression.", newStack.Push(First));
            }
        }
    }
}
