using System.Collections.Generic;
using System.Linq;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Combinators
{
    public class Alternative : IJsonMasherOperator, IPathGenerator
    {
        public IJsonMasherOperator First { get; init; }
        public IJsonMasherOperator Second { get; init; }

        public IEnumerable<PathAndValue> GeneratePaths(JsonPath pathSoFar, Json json, IMashContext context, IMashStack stack)
        {
            var newStack = stack.Push(this);
            context.Tick(newStack);
            if (First is IPathGenerator firstPathGenerator)
            {
                IEnumerable<PathAndValue> paths = json == Json.Null
                    ? Enumerable.Empty<PathAndValue>()
                    : firstPathGenerator.GeneratePaths(pathSoFar, json, context, stack).ToList();
                if (paths.Select(p => p.Value).Where(p => p.GetBool()).Any())
                {
                    return paths;
                }
                else if (Second is IPathGenerator secondPathGenerator)
                {
                    return secondPathGenerator.GeneratePaths(pathSoFar, json, context, stack);
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

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            var newStack = stack.Push(this);
            context.Tick(newStack);
            var values = First.Mash(json, context, newStack).Where(v => v.GetBool());
            if (values.Any()) 
            {
                return values;
            }
            else
            {
                return Second.Mash(json, context, newStack);
            }
        }
    }
}
