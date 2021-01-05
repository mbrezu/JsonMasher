using System;
using System.Collections.Generic;
using System.Linq;
using JsonMasher.Mashers.Builtins;

namespace JsonMasher.Mashers.Combinators
{
    public class Assignment : IJsonMasherOperator
    {
        public IJsonMasherOperator PathExpression { get; init; }
        public IJsonMasherOperator Masher { get; init; }
        public bool UseWholeValue { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            var newStack = stack.Push(this);
            context.Tick(stack);
            var pathsAndValues = PathFunction.GeneratePaths(PathExpression, json, context, newStack);
            var wholeValue = json;
            foreach (var pathAndValue in pathsAndValues)
            {
                json = Update(wholeValue, json, pathAndValue.Path, context, newStack);
            }
            yield return json;
        }

        private Json Update(
            Json wholeValue, Json json, Path path, IMashContext context, IMashStack stack)
        {
            if (path == Path.Empty)
            {
                return Masher.Mash(UseWholeValue ? wholeValue : json, context, stack).First();
            }
            else
            {
                var pathPart = path.Parts.First();
                return pathPart switch {
                    IntPathPart ip => json.SetElementAt(
                        ip.Value, 
                        Update(wholeValue, json.GetElementAt(ip.Value), path.WithoutFirstPart, context, stack)),
                    StringPathPart sp => json.SetElementAt(
                        sp.Value,
                        Update(wholeValue, json.GetElementAt(sp.Value), path.WithoutFirstPart, context, stack)),
                    SlicePathPart slicePart => UpdateRange(
                        wholeValue, json, slicePart.Start, slicePart.End, path.WithoutFirstPart, context, stack),
                    _ => throw new InvalidOperationException()
                };
            }
        }

        private Json UpdateRange(
            Json wholeValue, Json json, int start, int end, Path path, IMashContext context, IMashStack stack)
        {
            var slice = SliceSelector.GetSlice(UseWholeValue ? wholeValue : json, Tuple.Create(start, end));
            slice = Update(wholeValue, slice, path, context, stack);
            return Json.Array(
                json.EnumerateArray().Take(start)
                    .Concat(slice.EnumerateArray())
                    .Concat(json.EnumerateArray().Skip(end)));
        }
    }
}
