using System.Collections.Generic;

namespace JsonMasher
{
    public interface IJsonMasherOperator
    {
        IEnumerable<Json> Mash(Json json, IMashContext context);
    }
}
