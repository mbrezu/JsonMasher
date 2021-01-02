using System.Collections.Generic;

namespace JsonMasher.Mashers.Combinators
{
    public class SliceSelector : IJsonMasherOperator
    {
        public IJsonMasherOperator From { get; init; }
        public IJsonMasherOperator To { get; init; }
        public bool IsOptional { get; init; }
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
                foreach (var from in Froms(json, context, newStack))
                {
                    if (from.Type != JsonValueType.Number)
                    {
                        context.Error($"Index must be a number, not {from.Type}.", newStack, from);
                    }
                    foreach (var to in Tos(json, context, newStack))
                    {
                        if (from.Type != JsonValueType.Number)
                        {
                            context.Error($"Index must be a number, not {from.Type}.", newStack, from);
                        }
                        context.Tick(newStack);
                        var slice = new List<Json>();
                        int start = (int)from.GetNumber();
                        int end = (int)to.GetNumber();
                        if (start < 0 && end > 0)
                        {
                            end -= json.GetLength();
                        }
                        if (end < 0 && start >= 0)
                        {
                            start -= json.GetLength();
                        }
                        for (int i = (int)start; i < end; i++)
                        {
                            slice.Add(json.GetElementAt(i));
                        }
                        yield return Json.Array(slice);
                    }
                }
            }
        }

        private IEnumerable<Json> Froms(Json json, IMashContext context, IMashStack stack)
            => From == null
                ? Json.Number(0).AsEnumerable()
                : From.Mash(json, context, stack);

        private IEnumerable<Json> Tos(Json json, IMashContext context, IMashStack stack)
            => To == null
                ? Json.Number(json.GetLength()).AsEnumerable()
                : To.Mash(json, context, stack);
    }
}
