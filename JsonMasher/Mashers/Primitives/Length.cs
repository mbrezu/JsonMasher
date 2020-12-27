using System;
using System.Collections.Generic;

namespace JsonMasher.Mashers.Primitives
{
    public class Length : IJsonMasherOperator
    {
        public IEnumerable<Json> Mash(Json json, IMashContext context)
            => MashOne(json).AsEnumerable();

        private Json MashOne(Json json)
            => json.Type switch {
                JsonValueType.Array or JsonValueType.Object => Json.Number(json.GetLength()),
                _ => throw new InvalidOperationException()
            };
    
        private Length()
        {
        }

        private static Length _instance = new Length();
        public static Length Instance => _instance;
    }
}
