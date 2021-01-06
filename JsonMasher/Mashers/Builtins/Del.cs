using System.Collections.Generic;
using JsonMasher.Mashers.Combinators;
using System.Linq;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Builtins
{
    public class Del
    {
        private static Builtin _builtin = new Builtin(Function, 1);

        private static IEnumerable<Json> Function(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context, IMashStack stack)
        {
            if (mashers[0] is IPathGenerator pathGenerator)
            {
                var paths = pathGenerator
                    .GeneratePaths(JsonPath.Empty, json, context, stack)
                    .Select(pv => new { Key = pv.Path.ToJsonArray(), Path = pv.Path })
                    .OrderByDescending(x => x.Key, JsonComparer.Instance)
                    .Select(x => x.Path);
                foreach (var path in paths)
                {
                    json = DeletePath(json, path, context, stack);
                }
                yield return json;
            }
            else
            {
                throw context.Error("Not a path expression.", stack.Push(mashers[0]));
            }
        }

        public static Json DeletePath(Json json, JsonPath path, IMashContext context, IMashStack stack)
            => json.TransformByPath(
                path,
                leafJson => null,
                (json, pathPart) => context.Error(
                    $"Can't index {json.Type} with {pathPart.ToString()}.",
                    stack,
                    json,
                    pathPart.ToJson()));

        public static Builtin Builtin => _builtin;
    }
}
