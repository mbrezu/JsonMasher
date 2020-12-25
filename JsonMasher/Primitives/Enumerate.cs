using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Primitives
{
    public class Enumerate : IJsonMasher
    {
        public IEnumerable<Json> Mash(Json json)
        {
            if (json.Type == JsonValueType.Array)
            {
                return json.EnumerateArray();
            }
            else if (json.Type == JsonValueType.Object)
            {
                return json.EnumerateObject().Select(kv => kv.Value);
            }
            else
            {
                throw new InvalidOperationException("Can't enumerate value.");
            }
        }

        public IEnumerable<Json> Mash(IEnumerable<Json> seq)
        {
            foreach (var json in seq)
            {
                foreach (var result in Mash(json))
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
