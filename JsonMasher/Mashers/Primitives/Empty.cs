using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Mashers.Primitives
{
    public class Empty : IJsonMasherOperator
    {
        public IEnumerable<Json> Mash(Json json, IMashContext context)
            => Enumerable.Empty<Json>();

        private Empty()
        {
        }

        private static Empty _instance = new Empty();
        public static Empty Instance => _instance;
    }
}
