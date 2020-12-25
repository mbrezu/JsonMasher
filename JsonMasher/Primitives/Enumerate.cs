using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Primitives
{
    public class Enumerate : IJsonMasherOperator
    {
        public IEnumerable<Json> Mash(Json json, IMashContext context)
            => json.Type switch 
            {
                JsonValueType.Array => json.EnumerateArray(),
                JsonValueType.Object => json.EnumerateObject().Select(kv => kv.Value),
                _ => throw new InvalidOperationException("Can't enumerate value.")
            };

        public IEnumerable<Json> Mash(IEnumerable<Json> seq, IMashContext context)
        {
            foreach (var json in seq)
            {
                foreach (var result in Mash(json, context))
                {
                    yield return result;
                }
            }
        }

        private Enumerate()
        {
        }

        private static Enumerate _instance = new Enumerate();
        public static Enumerate Instance => _instance;
    }
}
