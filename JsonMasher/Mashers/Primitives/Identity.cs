using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Mashers.Primitives
{
    public class Identity : IJsonMasherOperator, IJsonZipper
    {
        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
            => json.AsEnumerable();

        public ZipStage ZipDown(Json json, IMashContext context, IMashStack stack)
            => new ZipStage(parts => parts.First(), Mash(json, context, stack));

        private Identity()
        {
        }

        private static Identity _instance = new Identity();
        public static Identity Instance => _instance;
    }
}
