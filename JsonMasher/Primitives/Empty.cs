using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Primitives
{
    public class Empty : IJsonMasher
    {
        public IEnumerable<Json> Mash(Json json)
            => Enumerable.Empty<Json>();

        public IEnumerable<Json> Mash(IEnumerable<Json> seq)
            => Enumerable.Empty<Json>();

        private Empty()
        {
        }

        private static Empty _instance = new Empty();
        public static Empty Instance => _instance;
    }
}
