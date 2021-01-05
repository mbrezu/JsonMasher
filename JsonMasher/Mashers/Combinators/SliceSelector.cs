using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Mashers.Combinators
{
    public class SliceSelector : IJsonMasherOperator, IPathGenerator
    {
        public IJsonMasherOperator From { get; init; }
        public IJsonMasherOperator To { get; init; }
        public bool IsOptional { get; init; }

        public IEnumerable<PathAndValue> GeneratePaths(
            Path pathSoFar, Json json, IMashContext context, IMashStack stack)
        {
            var newStack = stack.Push(this);

            foreach (var fromTo in GetFromsAndTos(json, context, newStack))
            {
                context.Tick(newStack);
                yield return new PathAndValue(
                    pathSoFar.Extend(new SlicePathPart(fromTo.Item1, fromTo.Item2)),
                    json == Json.Null ? json : GetSlice(json, fromTo));
            }
        }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            var newStack = stack.Push(this);
            bool skip = false;
            if (json.Type != JsonValueType.Array)
            {
                if (!IsOptional)
                {
                    throw context.Error($"Cannot slice a {json.Type}.", newStack, json);
                }
                else
                {
                    skip = true;
                }
            }
            if (!skip)
            {
                foreach (var fromTo in GetFromsAndTos(json, context, newStack))
                {
                    context.Tick(newStack);
                    yield return GetSlice(json, fromTo);
                }
            }
        }

        public static Json GetSlice(Json json, Tuple<int, int> fromTo)
        {
            var slice = new List<Json>();
            for (int i = (int)fromTo.Item1; i < fromTo.Item2; i++)
            {
                slice.Add(json.GetElementAt(i));
            }
            return Json.Array(slice);
        }

        private IEnumerable<Tuple<int, int>> GetFromsAndTos(
            Json json, IMashContext context, IMashStack stack)
        {
            foreach (var from in Froms(json, context, stack))
            {
                if (from.Type != JsonValueType.Number)
                {
                    context.Error($"Index must be a number, not {from.Type}.", stack, from);
                }
                foreach (var to in Tos(json, context, stack))
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
                        start += json.GetLength();
                    }
                    if (end < 0)
                    {
                        end += json.GetLength();
                    }
                    yield return Tuple.Create(start, end);
                }
            }
        }

        private IEnumerable<Json> Froms(Json json, IMashContext context, IMashStack stack)
            => From == null
                ? Json.Number(0).AsEnumerable()
                : From.Mash(json, context, stack);

        private IEnumerable<Json> Tos(Json json, IMashContext context, IMashStack stack)
            => To == null
                ? ExtractLength(json)
                : To.Mash(json, context, stack);

        private static IEnumerable<Json> ExtractLength(Json json)
        {
            if (json.Type == JsonValueType.Array)
            {
                return Json.Number(json.GetLength()).AsEnumerable();
            }
            else
            {
                return Enumerable.Empty<Json>();
            }
        }
    }
}
