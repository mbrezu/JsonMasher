using System.Collections.Generic;

namespace JsonMasher.Mashers
{
    public interface IJsonMasher
    {
        (IEnumerable<Json> sequence, IMashContext context) Mash(IEnumerable<Json> seq, IJsonMasherOperator op);
    }
}
