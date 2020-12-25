using System.Collections.Generic;

namespace JsonMasher.Primitives
{
    public class Identity : IJsonMasher
    {
        public IEnumerable<Json> Mash(IEnumerable<Json> seq) 
            => seq;

        public IEnumerable<Json> Mash(Json seq)
            => seq.AsEnumerable();

        private Identity()
        {
        }

        private static Identity _instance = new Identity();
        public static Identity Instance => _instance;
    }
}
