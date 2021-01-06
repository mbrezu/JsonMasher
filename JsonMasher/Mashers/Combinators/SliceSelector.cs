using System;
using System.Collections.Generic;
using System.Linq;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Combinators
{
    public class SliceSelector : IJsonMasherOperator, IPathGenerator
    {
        public IJsonMasherOperator From { get; init; }
        public IJsonMasherOperator To { get; init; }
        public bool IsOptional { get; init; }
        public IJsonMasherOperator Target { get; init; }

        public IEnumerable<PathAndValue> GeneratePaths(
            JsonPath pathSoFar, Json json, IMashContext context, IMashStack stack)
        {
            var newStack = stack.Push(this);

            foreach (var fromTo in GetFromsAndTos(json, json, context, newStack))
            {
                context.Tick(newStack);
                yield return new PathAndValue(
                    pathSoFar.Extend(new SlicePathPart(fromTo.Item1, fromTo.Item2)),
                    json == Json.Null ? json : json.GetSliceAt(fromTo.Item1, fromTo.Item2));
            }
        }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            var newStack = stack.Push(this);
            if (Target == null)
            {
                return MashOne(json, json, context, stack);
            }
            else
            {
                return Target
                    .Mash(json, context, newStack)
                    .SelectMany(target => MashOne(target, json, context, stack));
            }
        }

        private IEnumerable<Json> MashOne(Json target, Json index, IMashContext context, IMashStack stack)
        {
            if (target.Type != JsonValueType.Array)
            {
                if (!IsOptional)
                {
                    throw context.Error($"Cannot slice a {target.Type}.", stack, target);
                }
                else
                {
                    return Enumerable.Empty<Json>();
                }
            }
            return GetFromsAndTos(target, index, context, stack).Select(fromTo => {
                context.Tick(stack);
                return target.GetSliceAt(fromTo.Item1, fromTo.Item2);
            });
        }

        private IEnumerable<Tuple<int, int>> GetFromsAndTos(
            Json target, Json index, IMashContext context, IMashStack stack)
        {
            foreach (var from in Froms(target, index, context, stack))
            {
                if (from.Type != JsonValueType.Number)
                {
                    context.Error($"Index must be a number, not {from.Type}.", stack, from);
                }
                foreach (var to in Tos(target, index, context, stack))
                {
                    if (from.Type != JsonValueType.Number)
                    {
                        context.Error($"Index must be a number, not {from.Type}.", stack, from);
                    }
                    context.Tick(stack);
                    int start = (int)from.GetNumber();
                    int end = (int)to.GetNumber();
                    if (start < 0)
                    {
                        start += target.GetLength();
                    }
                    if (end < 0)
                    {
                        end += target.GetLength();
                    }
                    yield return Tuple.Create(start, end);
                }
            }
        }

        private IEnumerable<Json> Froms(Json target, Json index, IMashContext context, IMashStack stack)
            => From == null
                ? Json.Number(0).AsEnumerable()
                : From.Mash(index, context, stack);

        private IEnumerable<Json> Tos(Json target, Json index, IMashContext context, IMashStack stack)
            => To == null
                ? ExtractLength(target)
                : To.Mash(index, context, stack);

        private static IEnumerable<Json> ExtractLength(Json json)
        {
            if (json.Type == JsonValueType.Array)
            {
                return Json.Number(json.GetLength()).AsEnumerable();
            }
            else
            {
                return Json.Number(0).AsEnumerable();
            }
        }
    }
}
