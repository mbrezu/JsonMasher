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
            JsonPath pathSoFar, Json json, IMashContext context)
        {
            context = context.PushStack(this);

            foreach (var fromTo in GetFromsAndTos(json, json, context))
            {
                context.Tick();
                yield return new PathAndValue(
                    pathSoFar.Extend(new SlicePathPart(fromTo.Item1, fromTo.Item2)),
                    json == Json.Null ? json : json.GetSliceAt(fromTo.Item1, fromTo.Item2));
            }
        }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
        {
            context = context.PushStack(this);
            if (Target == null)
            {
                return MashOne(json, json, context);
            }
            else
            {
                return Target
                    .Mash(json, context)
                    .SelectMany(target => MashOne(target, json, context));
            }
        }

        private IEnumerable<Json> MashOne(Json target, Json index, IMashContext context)
        {
            if (target.Type != JsonValueType.Array && target.Type != JsonValueType.String)
            {
                if (!IsOptional)
                {
                    throw context.Error($"Cannot slice a {target.Type}.", target);
                }
                else
                {
                    return Enumerable.Empty<Json>();
                }
            }
            return GetFromsAndTos(target, index, context).Select(fromTo => {
                context.Tick();
                return target.GetSliceAt(fromTo.Item1, fromTo.Item2);
            });
        }

        private IEnumerable<Tuple<int, int>> GetFromsAndTos(
            Json target, Json index, IMashContext context)
        {
            foreach (var from in Froms(target, index, context))
            {
                if (from.Type != JsonValueType.Number)
                {
                    context.Error($"Index must be a number, not {from.Type}.", from);
                }
                foreach (var to in Tos(target, index, context))
                {
                    if (from.Type != JsonValueType.Number)
                    {
                        context.Error($"Index must be a number, not {from.Type}.", from);
                    }
                    context.Tick();
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

        private IEnumerable<Json> Froms(Json target, Json index, IMashContext context)
            => From == null
                ? Json.Number(0).AsEnumerable()
                : From.Mash(index, context);

        private IEnumerable<Json> Tos(Json target, Json index, IMashContext context)
            => To == null
                ? ExtractLength(target)
                : To.Mash(index, context);

        private static IEnumerable<Json> ExtractLength(Json json)
        {
            if (json.Type == JsonValueType.Array || json.Type == JsonValueType.String)
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
