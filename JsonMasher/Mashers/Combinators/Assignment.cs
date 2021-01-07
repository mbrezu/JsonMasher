using System;
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

        public IEnumerable<Json> Mash(Json json, IMashContext context)
        {
            context = context.PushStack(this);
            context.Tick();
            var pathsAndValues = Path.GeneratePaths(PathExpression, json, context);
            Exception onError(Json json, JsonPathPart jsonPathPart)
                => context.Error(
                    $"Can't index {json.Type} with {jsonPathPart.ToString()}.",
                    json,
                    jsonPathPart.ToJson());
            if (UseWholeValue)
            {
                foreach (var value in Masher.Mash(json, context))
                {
                    var newJson = json;
                    foreach (var pathAndValue in pathsAndValues)
                    {
                        newJson = newJson.TransformByPath(
                            pathAndValue.Path, leafJson => value, onError);
                    }
                    yield return newJson;
                }
            }
            else
            {
                foreach (var pathAndValue in pathsAndValues)
                {
                    json = json.TransformByPath(
                        pathAndValue.Path,
                        leafJson => Masher.Mash(leafJson, context).First(),
                        onError);
                }
                yield return json;
            }
        }
    }
}
