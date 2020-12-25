using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Primitives
{
    public class Length : IJsonMasherOperator
    {
        public IEnumerable<Json> Mash(IEnumerable<Json> seq, IMashContext context)
            => seq.Select(MashOne);

        public IEnumerable<Json> Mash(Json json, IMashContext context)
            => MashOne(json).AsEnumerable();

        private Json MashOne(Json json)
            => json.Type switch {
                JsonValueType.Array => Json.Number(json.EnumerateArray().Count()),
                JsonValueType.Object => Json.Number(json.EnumerateObject().Count()),
                _ => throw new InvalidOperationException()
            };
    
        private Length()
        {
        }

        private static Length _instance = new Length();
        public static Length Instance => _instance;
    }
}
