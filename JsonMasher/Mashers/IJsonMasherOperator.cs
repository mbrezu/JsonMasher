using System.Collections.Generic;

namespace JsonMasher.Mashers
{
    public interface IJsonMasherOperator : Callable
    {
        IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack);
    }
}
