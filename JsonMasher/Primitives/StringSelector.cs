using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Primitives
{
    public class StringSelector : IJsonMasherOperator
    {
        public string Key { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
            => MashOne(json).AsEnumerable();

        private Json MashOne(Json json)
            => json.Type switch {
                JsonValueType.Object => json.GetElementAt(Key),
                _ => throw new InvalidOperationException()
            };
    }
}
