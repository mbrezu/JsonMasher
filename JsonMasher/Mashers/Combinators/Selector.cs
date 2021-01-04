using System.Collections.Generic;

namespace JsonMasher.Mashers.Combinators
{
    public class Selector : IJsonMasherOperator
    {
        public IJsonMasherOperator Index { get; init; }
        public bool IsOptional { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            context.Tick(stack);
            return MashOne(json, context, stack.Push(this));
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
