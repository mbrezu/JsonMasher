using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Primitives
{
    public class IndexSelector : IJsonMasherOperator
    {
        public int Index { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
            => MashOne(json).AsEnumerable();

        public IEnumerable<Json> Mash(IEnumerable<Json> seq, IMashContext context)
            => seq.Select(MashOne);

        private Json MashOne(Json json)
            => json.Type switch {
                JsonValueType.Array => json.GetElementAt(Index),
                _ => throw new InvalidOperationException()
            };
    }
}
