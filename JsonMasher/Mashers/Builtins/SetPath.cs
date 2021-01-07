using System.Collections.Generic;
using System.Linq;
using JsonMasher.Compiler;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class SetPath
    {
        private static Builtin _builtin = new Builtin(Function, 2);

        private static IEnumerable<Json> Function(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context)
        {
            foreach (var value in mashers[1].Mash(json, context))
            {
                foreach (var pathAsJson in mashers[0].Mash(json, context))
                {
                    var path = context.GetPathFromArray(pathAsJson);
                    yield return json.TransformByPath(
                        path,
                        leafJson => value,
                        (json, pathPart) => context.Error(
                            $"Can't index {json.Type} with {pathPart.ToString()}.",
                            json,
                            pathPart.ToJson()));
                }
            }
        }

        public static Builtin Builtin => _builtin;
    }
}
