using System.Collections.Generic;
using System.Linq;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Combinators
{
    public class Selector : IJsonMasherOperator, IPathGenerator
    {
        public IJsonMasherOperator Index { get; init; }
        public IJsonMasherOperator Target { get; init; }
        public bool IsOptional { get; init; }

        public IEnumerable<PathAndValue> GeneratePaths(
            JsonPath pathSoFar, Json json, IMashContext context)
        {
            context = context.PushStack(this);
            context.Tick();
            foreach (var indexValue in Index.Mash(json, context))
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
                    else if (json.Type == JsonValueType.Object)
                    {
                        yield return new PathAndValue(
                            pathSoFar.Extend(new StringPathPart(strIndex)),
                            json.GetElementAt(strIndex));
                    }
                    else
                    {
                        throw context.Error($"Can't index a {json.Type} with a {indexValue.Type} key.", json, indexValue);
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
                    else if (json.Type == JsonValueType.Array)
                    {
                        yield return new PathAndValue(
                            pathSoFar.Extend(new IntPathPart(intIndex)),
                            json.GetElementAt(intIndex));
                    }
                    else
                    {
                        throw context.Error($"Can't index a {json.Type} with a {indexValue.Type} key.", json, indexValue);
                    }
                }
                else
                {
                    throw context.PushStack(Index).Error("Not a path expression.");
                }
            }
        }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
        {
            context = context.PushStack(this);
            context.Tick();
            if (Target == null)
            {
                return MashOne(json, json, context);
            }
            else
            {
                return Target
                    .Mash(json, context)
                    .SelectMany(x => MashOne(x, json, context));
            }
        }

        private IEnumerable<Json> MashOne(Json target, Json index, IMashContext context)
        {
            foreach (var indexValue in Index.Mash(index, context))
            {
                var value = (indexValue.Type, target.Type) switch {
                    (JsonValueType.Number, JsonValueType.Array)
                        => target.GetElementAt((int)(indexValue.GetNumber())),
                    (JsonValueType.String, JsonValueType.Object)
                        => target.GetElementAt(indexValue.GetString()),
                    _ => !IsOptional 
                        ? throw context.Error(
                            $"Can't index {target.Type} with {indexValue.Type}.", target, indexValue)
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
