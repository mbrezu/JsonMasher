using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Primitives
{
    public class Length : IJsonMasher
    {
        public IEnumerable<Json> Mash(IEnumerable<Json> seq)
        {
            foreach (var json in seq)
            {
                yield return MashOne(json);
            }
        }

        public IEnumerable<Json> Mash(Json json)
        {
            yield return MashOne(json);
        }

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
