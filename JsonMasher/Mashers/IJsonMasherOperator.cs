using System.Collections.Generic;

namespace JsonMasher.Mashers
{
    public interface IJsonMasherOperator
    {
        IEnumerable<Json> Mash(Json json, IMashContext context);
    }
}
