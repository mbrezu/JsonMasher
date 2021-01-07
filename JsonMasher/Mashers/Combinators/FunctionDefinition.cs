using System.Collections.Generic;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Combinators
{
    public class FunctionDefinition : IJsonMasherOperator, IPathGenerator
    {
        public string Name { get; init; }
        public List<string> Arguments { get; init; }
        public IJsonMasherOperator Body { get; init; }

        public IEnumerable<PathAndValue> GeneratePaths(
            JsonPath pathSoFar, Json json, IMashContext context)
        {
            context.Tick();
            context.SetCallable(Name, Arguments, Body);
            yield return new PathAndValue(pathSoFar, json);
        }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
        {
            context.Tick();
            context.SetCallable(Name, Arguments, Body);
            return json.AsEnumerable();
        }
    }
}
