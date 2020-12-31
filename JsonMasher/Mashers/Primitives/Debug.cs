using System.Collections.Generic;

namespace JsonMasher.Mashers.Primitives
{
    public class Debug : IJsonMasherOperator
    {
        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            context.LogValue(json);
            yield return json;
        }

        private Debug()
        {
        }

        private static Debug _instance = new Debug();
        public static Debug Instance => _instance;
    }
}
