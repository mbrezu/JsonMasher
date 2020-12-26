using System.Collections.Generic;

namespace JsonMasher
{
    public interface IJsonMasher
    {
        IEnumerable<Json> Mash(IEnumerable<Json> seq, IJsonMasherOperator op);
        IMashContext Context { get; }
    }
}
