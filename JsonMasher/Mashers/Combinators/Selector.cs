using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Mashers.Combinators
{
    public class Selector : IJsonMasherOperator
    {
        public IJsonMasherOperator Index { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
            => MashOne(json, context).AsEnumerable();

        private IEnumerable<Json> MashOne(Json json, IMashContext context)
            => Index.Mash(json, context).Select(index => 
                (index.Type, json.Type) switch {
                    (JsonValueType.Number, JsonValueType.Array) => json.GetElementAt((int)(index.GetNumber())),
                    (JsonValueType.String, JsonValueType.Object) => json.GetElementAt(index.GetString()),
                    _ => throw new InvalidOperationException()
                });
    }
}
