using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Primitives
{
    public class Debug : IJsonMasherOperator
    {
        public IEnumerable<Json> Mash(IEnumerable<Json> seq, IMashContext context) 
            => seq.SelectMany(x => Mash(x, context));

        public IEnumerable<Json> Mash(Json json, IMashContext context)
        {
            context.LogValue(json);
            yield return json;
        }

        private static Debug _instance = new Debug();
        public static Debug Instance => _instance;
    }
}
