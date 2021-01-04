using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Mashers.Primitives
{
    public class StringSelector : IJsonMasherOperator
    {
        public string Key { get; init; }
        public bool IsOptional { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
            => json.Type switch {
                JsonValueType.Object => json.GetElementAt(Key).AsEnumerable(),
                _ => !IsOptional
                    ? throw context.Error($"Can't index {json.Type} with a string.", stack.Push(this), json)
                    : Enumerable.Empty<Json>()
            };
    }
}
