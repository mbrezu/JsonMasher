using System.Collections.Generic;
using System.Linq;
using JsonMasher.Compiler;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class GetPath
    {
        private static Builtin _builtin = new Builtin(Function, 1);

        private static IEnumerable<Json> Function(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context, IMashStack stack)
        {
            var result = new List<Json>();
            foreach (var pathAsJson in mashers[0].Mash(json, context, stack))
            {
                if (pathAsJson.Type != JsonValueType.Array)
                {
                    throw context.Error($"Can't use {pathAsJson.Type} as a path.", stack, pathAsJson);
                }
                JsonPath path = null;
                try
                {
                    path = JsonPath.FromParts(pathAsJson.EnumerateArray().Select(x => x.GetPathPart()));
                }
                catch (JsonMasherException ex)
                {
                    throw context.Error(ex.Message, stack, ex.Values.ToArray());
                }
                json.TransformByPath(
                    path,
                    leafJson => {
                        result.Add(leafJson);
                        return leafJson;
                    },
                    (json, pathPart) => context.Error(
                        $"Can't index {json.Type} with {pathPart.ToString()}.",
                        stack,
                        json,
                        pathPart.ToJson()));
            }
            return result;
        }

        public static Builtin Builtin => _builtin;
    }
}
