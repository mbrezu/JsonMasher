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
                var path = context.GetPathFromArray(pathAsJson, stack);
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
