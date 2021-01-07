using System.Collections.Generic;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Combinators
{
    public class Compose : IJsonMasherOperator, IPathGenerator
    {
        public IJsonMasherOperator First { get; init; }
        public IJsonMasherOperator Second { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
        {
            context = context.PushStack(this);
            context.Tick();
            foreach (var temp in First.Mash(json, context))
            {
                foreach (var result in Second.Mash(temp, context))
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
            JsonPath pathSoFar, Json json, IMashContext context)
        {
            context = context.PushStack(this);
            context.Tick();
            if (First is IPathGenerator pg1)
            {
                if (Second is IPathGenerator pg2)
                {
                    foreach (var pathAndValue1 in pg1.GeneratePaths(pathSoFar, json, context))
                    {
                        foreach (var pathAndValue2 in pg2.GeneratePaths(
                            pathAndValue1.Path, pathAndValue1.Value, context))
                        {
                            yield return pathAndValue2;
                        }
                    }
                }
                else
                {
                    throw context.PushStack(Second).Error("Not a path expression.");
                }
            }
            else
            {
                throw context.PushStack(First).Error("Not a path expression.");
            }
        }
    }
}
