using System.Collections.Generic;

namespace JsonMasher.Mashers.Combinators
{
    public class Selector : IJsonMasherOperator, IPathGenerator
    {
        public IJsonMasherOperator Index { get; init; }
        public bool IsOptional { get; init; }

        public IEnumerable<PathAndValue> GeneratePaths(
            Path pathSoFar, Json json, IMashContext context, IMashStack stack)
        {
            var newStack = stack.Push(this);
            context.Tick(newStack);
            foreach (var indexValue in Index.Mash(json, context, stack))
            {
                if (indexValue.Type == JsonValueType.String)
                {
                    var strIndex = indexValue.GetString();
                    if (json == Json.Null)
                    {
                        yield return new PathAndValue(
                            pathSoFar.Extend(new StringPathPart(strIndex)),
                            json);
                    }
                    else
                    {
                        yield return new PathAndValue(
                            pathSoFar.Extend(new StringPathPart(strIndex)),
                            json.GetElementAt(strIndex));
                    }
                }
                else if (indexValue.Type == JsonValueType.Number)
                {
                    var intIndex = (int)indexValue.GetNumber();
                    if (json == Json.Null)
                    {
                        yield return new PathAndValue(
                            pathSoFar.Extend(new IntPathPart(intIndex)),
                            json);
                    }
                    else
                    {
                        yield return new PathAndValue(
                            pathSoFar.Extend(new IntPathPart(intIndex)),
                            json.GetElementAt(intIndex));
                    }
                }
                else
                {
                    throw context.Error("Not a path expression.", newStack.Push(Index));
                }
            }
        }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            var newStack = stack.Push(this);
            context.Tick(newStack);
            return MashOne(json, context, newStack);
        }

        private IEnumerable<Json> MashOne(Json json, IMashContext context, IMashStack stack)
        {
            foreach (var index in Index.Mash(json, context, stack))
            {
                var value = (index.Type, json.Type) switch {
                    (JsonValueType.Number, JsonValueType.Array) => json.GetElementAt((int)(index.GetNumber())),
                    (JsonValueType.String, JsonValueType.Object) => json.GetElementAt(index.GetString()),
                    _ => !IsOptional 
                        ? throw context.Error($"Can't index {json.Type} with {index.Type}.", stack, json, index)
                        : null
                };
                if (value != null)
                {
                    yield return value;
                }
            }
        }
    }
}
