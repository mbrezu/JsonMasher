using System.Collections.Generic;
using System.Linq;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class Path
    {
        private static Builtin _builtin = new Builtin(Function, 1);

        public static IEnumerable<Json> Function(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context, IMashStack stack)
        {
            var masher = mashers.First();
            foreach (var pathAndValue in GeneratePaths(masher, json, context, stack))
            {
                yield return pathAndValue.Path.ToJsonArray();
            }
        }

        public static IEnumerable<PathAndValue> GeneratePaths(
            IJsonMasherOperator masher, Json json, IMashContext context, IMashStack stack)
        {
            if (masher is IPathGenerator generator)
            {
                foreach (var pathAndValue in generator.GeneratePaths(JsonPath.Empty, json, context, stack))
                {
                    yield return pathAndValue;
                }
            }
            else
            {
                throw context.Error("Not a path expression.", stack.Push(masher));
            }
        }

        public static Builtin Builtin => _builtin;
    }
}
