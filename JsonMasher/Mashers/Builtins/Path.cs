using System.Collections.Generic;
using System.Linq;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class Path
    {
        private static Builtin _builtin = new Builtin(Function, 1);

        public static IEnumerable<Json> Function(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context)
        {
            var masher = mashers.First();
            foreach (var pathAndValue in GeneratePaths(masher, json, context))
            {
                yield return pathAndValue.Path.ToJsonArray();
            }
        }

        public static IEnumerable<PathAndValue> GeneratePaths(
            IJsonMasherOperator masher, Json json, IMashContext context)
        {
            if (masher is IPathGenerator generator)
            {
                foreach (var pathAndValue in generator.GeneratePaths(JsonPath.Empty, json, context))
                {
                    yield return pathAndValue;
                }
            }
            else
            {
                throw context.PushStack(masher).Error("Not a path expression.");
            }
        }

        public static Builtin Builtin => _builtin;
    }
}
