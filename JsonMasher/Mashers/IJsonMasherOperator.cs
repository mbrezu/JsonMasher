using System.Collections.Generic;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers
{
    public interface IJsonMasherOperator : Callable
    {
        IEnumerable<Json> Mash(Json json, IMashContext context);
    }
}
