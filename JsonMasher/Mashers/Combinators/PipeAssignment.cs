using System;
using System.Collections.Generic;
using System.Linq;
using JsonMasher.Mashers.Builtins;

namespace JsonMasher.Mashers.Combinators
{
    public class PipeAssignment : IJsonMasherOperator
    {
        public IJsonMasherOperator PathExpression { get; init; }
        public IJsonMasherOperator Masher { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            var newStack = stack.Push(this);
            context.Tick(stack);
            var pathsAndValues = PathFunction.GeneratePaths(PathExpression, json, context, newStack);
            foreach (var pathAndValue in pathsAndValues)
            {
                json = Update(json, pathAndValue.Path, context, newStack);
            }
            yield return json;
        }
        private Json Update(Json json, Path path, IMashContext context, IMashStack stack)
        {
            if (path == Path.Empty)
            {
                return Masher.Mash(json, context, stack).First();
            }
            else
            {
                var pathPart = path.Parts.First();
                return pathPart switch {
                    IntPathPart ip => json.SetElementAt(
                        ip.Value, 
                        Update(json.GetElementAt(ip.Value), path.WithoutFirstPart, context, stack)),
                    StringPathPart sp => json.SetElementAt(
                        sp.Value,
                        Update(json.GetElementAt(sp.Value), path.WithoutFirstPart, context, stack)),
                    SlicePathPart slicePart => throw new NotImplementedException(),
                    _ => throw new InvalidOperationException()
                };
            }
        }
    }
}
