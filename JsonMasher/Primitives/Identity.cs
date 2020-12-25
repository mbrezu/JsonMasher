using System.Collections.Generic;

namespace JsonMasher.Primitives
{
    public class Identity : IJsonMasherOperator
    {
        public IEnumerable<Json> Mash(IEnumerable<Json> seq, IMashContext context) 
            => seq;

        public IEnumerable<Json> Mash(Json seq, IMashContext context)
            => seq.AsEnumerable();

        private Identity()
        {
        }

        private static Identity _instance = new Identity();
        public static Identity Instance => _instance;
    }
}
