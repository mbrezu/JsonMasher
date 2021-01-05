using System.Collections.Generic;
using System.Linq;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Builtins;

namespace JsonMasher.Mashers.Combinators
{
    public class Assignment : IJsonMasherOperator
    {
        public IJsonMasherOperator PathExpression { get; init; }
        public IJsonMasherOperator Masher { get; init; }
        public bool UseWholeValue { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            var newStack = stack.Push(this);
            context.Tick(stack);
            var pathsAndValues = Path.GeneratePaths(PathExpression, json, context, newStack);
            var wholeValue = json;
            foreach (var pathAndValue in pathsAndValues)
            {
                json = Update(wholeValue, json, pathAndValue.Path, context, newStack);
            }
            yield return json;
        }

        private Json Update(
            Json wholeValue, Json json, JsonPath path, IMashContext context, IMashStack stack)
            => json.TransformLeaf(
                path,
                leafJson => Masher.Mash(UseWholeValue ? wholeValue : leafJson, context, stack).First(),
                (json, pathPart) => context.Error(
                    $"Can't index {json.Type} with {pathPart.ToString()}.",
                    stack,
                    json,
                    new JsonPath(pathPart).ToJsonArray()));
    }
}
