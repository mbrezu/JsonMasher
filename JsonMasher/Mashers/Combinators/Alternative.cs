using System.Collections.Generic;
using System.Linq;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Combinators
{
    public class Alternative : IJsonMasherOperator, IPathGenerator
    {
        public IJsonMasherOperator First { get; init; }
        public IJsonMasherOperator Second { get; init; }

        public IEnumerable<PathAndValue> GeneratePaths(JsonPath pathSoFar, Json json, IMashContext context)
        {
            context = context.PushStack(this);
            context.Tick();
            if (First is IPathGenerator firstPathGenerator)
            {
                IEnumerable<PathAndValue> paths = json == Json.Null
                    ? Enumerable.Empty<PathAndValue>()
                    : firstPathGenerator.GeneratePaths(pathSoFar, json, context).ToList();
                if (paths.Select(p => p.Value).Where(p => p.GetBool()).Any())
                {
                    return paths;
                }
                else if (Second is IPathGenerator secondPathGenerator)
                {
                    return secondPathGenerator.GeneratePaths(pathSoFar, json, context);
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

        public IEnumerable<Json> Mash(Json json, IMashContext context)
        {
            context = context.PushStack(this);
            context.Tick();
            var values = First.Mash(json, context).Where(v => v.GetBool());
            if (values.Any()) 
            {
                return values;
            }
            else
            {
                return Second.Mash(json, context);
            }
        }
    }
}
