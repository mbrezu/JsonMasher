using System.Collections.Generic;

namespace JsonMasher
{
    public class JsonMasher : IJsonMasher
    {
        public IEnumerable<Json> Mash(IEnumerable<Json> seq, IJsonMasherOperator op)
        {
            var context = new MashContext();
            return op.Mash(seq, context);
        }
    }
}
