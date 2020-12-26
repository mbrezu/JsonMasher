using System.Collections.Generic;

namespace JsonMasher.Primitives
{
    public class Identity : IJsonMasherOperator
    {
        public IEnumerable<Json> Mash(Json json, IMashContext context)
            => json.AsEnumerable();

        private Identity()
        {
        }

        private static Identity _instance = new Identity();
        public static Identity Instance => _instance;
    }
}
