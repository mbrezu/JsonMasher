using System.Collections.Generic;

namespace JsonMasher.Mashers
{
    public interface IJsonMasher
    {
        IEnumerable<Json> Mash(IEnumerable<Json> seq, IJsonMasherOperator op);
        IMashContext Context { get; }
    }
}
