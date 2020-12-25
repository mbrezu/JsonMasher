using System.Collections.Generic;

namespace JsonMasher
{
    public interface IJsonMasherOperator
    {
        IEnumerable<Json> Mash(Json json, IMashContext context);
        IEnumerable<Json> Mash(IEnumerable<Json> seq, IMashContext context);
    }
}
