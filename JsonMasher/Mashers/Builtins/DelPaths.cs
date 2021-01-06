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
            List<IJsonMasherOperator> mashers, Json json, IMashContext context, IMashStack stack)
        {
            foreach (var jsonPaths in mashers[0].Mash(json, context, stack))
            {
                if (jsonPaths.Type != JsonValueType.Array)
                {
                    throw context.Error($"Expected an array of paths.", stack, jsonPaths);
                }
                var paths = jsonPaths
                    .EnumerateArray()
                    .OrderByDescending(x => x, JsonComparer.Instance)
                    .Select(x => context.GetPathFromArray(x, stack));
                var jsonResult = json;
                foreach (var path in paths)
                {
                    jsonResult = Del.DeletePath(jsonResult, path, context, stack);
                }
                yield return jsonResult;
            }
        }

        public static Builtin Builtin => _builtin;
    }
}
