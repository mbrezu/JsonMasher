using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Mashers.Primitives
{
    public class Identity : IJsonMasherOperator, IJsonZipper
    {
        public IEnumerable<Json> Mash(Json json, IMashContext context)
            => json.AsEnumerable();

        public ZipStage ZipDown(Json json, IMashContext context)
            => new ZipStage(parts => parts.First(), Mash(json, context));

        private Identity()
        {
        }

        private static Identity _instance = new Identity();
        public static Identity Instance => _instance;
    }
}
