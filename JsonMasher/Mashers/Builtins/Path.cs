using System.Collections.Generic;
using System.Linq;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class PathFunction
    {
        private static Builtin _builtin = new Builtin(Function, 1);

        private static IEnumerable<Json> Function(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context, IMashStack stack)
        {
            var masher = mashers.First();
            if (masher is IPathGenerator generator)
            {
                foreach (var pathAndValue in generator.GeneratePaths(Path.Empty, json, context, stack))
                {
                    yield return pathAndValue.Path.ToJsonArray();
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
