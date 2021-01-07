using System.Collections.Generic;
using System.Linq;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class DelPaths
    {
        private static Builtin _builtin = new Builtin(Function, 1);

        private static IEnumerable<Json> Function(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context)
        {
            foreach (var jsonPaths in mashers[0].Mash(json, context))
            {
                if (jsonPaths.Type != JsonValueType.Array)
                {
                    throw context.Error($"Expected an array of paths.", jsonPaths);
                }
                var paths = jsonPaths
                    .EnumerateArray()
                    .OrderByDescending(x => x, JsonComparer.Instance)
                    .Select(x => context.GetPathFromArray(x));
                var jsonResult = json;
                foreach (var path in paths)
                {
                    jsonResult = DeletePath(jsonResult, path, context);
                }
                yield return jsonResult;
            }
        }

        private static Json DeletePath(Json json, JsonPath path, IMashContext context)
            => json.TransformByPath(
                path,
                leafJson => null,
                (json, pathPart) => context.Error(
                    $"Can't index {json.Type} with {pathPart.ToString()}.",
                    json,
                    pathPart.ToJson()));

        public static Builtin Builtin => _builtin;
    }
}
