using System.Collections.Generic;

namespace JsonMasher.Mashers.Combinators
{
    public class Concat : IJsonMasherOperator, IPathGenerator
    {
        public IJsonMasherOperator First { get; init; }
        public IJsonMasherOperator Second { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            var newStack = stack.Push(this);
            context.Tick(newStack);
            foreach (var value in First.Mash(json, context, newStack))
            {
                yield return value;
            }
            foreach (var value in Second.Mash(json, context, newStack))
            {
                yield return value;
            }
        }

        public static IJsonMasherOperator AllParams(params IJsonMasherOperator[] mashers)
            => All(mashers);

        public static IJsonMasherOperator All(IEnumerable<IJsonMasherOperator> mashers)
            => mashers.Fold((m1, m2) => new Concat { First = m1, Second = m2 });

        public IEnumerable<PathAndValue> GeneratePaths(
            Path pathSoFar, Json json, IMashContext context, IMashStack stack)
        {
            var newStack = stack.Push(this);
            context.Tick(newStack);
            if (First is IPathGenerator pg1)
            {
                foreach (var pathAndValue in pg1.GeneratePaths(pathSoFar, json, context, newStack))
                {
                    yield return pathAndValue;
                }
            }
            else
            {
                throw context.Error("Not a path expression.", newStack.Push(First));
            }
            if (Second is IPathGenerator pg2)
            {
                foreach (var pathAndValue in pg2.GeneratePaths(pathSoFar, json, context, newStack))
                {
                    yield return pathAndValue;
                }
            }
            else
            {
                throw context.Error("Not a path expression.", newStack.Push(Second));
            }
        }
    }
}
