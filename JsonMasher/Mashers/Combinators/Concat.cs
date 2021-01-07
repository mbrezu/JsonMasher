using System.Collections.Generic;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Combinators
{
    public class Concat : IJsonMasherOperator, IPathGenerator
    {
        public IJsonMasherOperator First { get; init; }
        public IJsonMasherOperator Second { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
        {
            context = context.PushStack(this);
            context.Tick();
            foreach (var value in First.Mash(json, context))
            {
                yield return value;
            }
            foreach (var value in Second.Mash(json, context))
            {
                yield return value;
            }
        }

        public static IJsonMasherOperator AllParams(params IJsonMasherOperator[] mashers)
            => All(mashers);

        public static IJsonMasherOperator All(IEnumerable<IJsonMasherOperator> mashers)
            => mashers.Fold((m1, m2) => new Concat { First = m1, Second = m2 });

        public IEnumerable<PathAndValue> GeneratePaths(
            JsonPath pathSoFar, Json json, IMashContext context)
        {
            context = context.PushStack(this);
            context.Tick();
            if (First is IPathGenerator pg1)
            {
                foreach (var pathAndValue in pg1.GeneratePaths(pathSoFar, json, context))
                {
                    yield return pathAndValue;
                }
            }
            else
            {
                throw context.PushStack(First).Error("Not a path expression.");
            }
            if (Second is IPathGenerator pg2)
            {
                foreach (var pathAndValue in pg2.GeneratePaths(pathSoFar, json, context))
                {
                    yield return pathAndValue;
                }
            }
            else
            {
                throw context.PushStack(Second).Error("Not a path expression.");
            }
        }
    }
}
