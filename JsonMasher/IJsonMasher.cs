using System;
using System.Collections.Generic;

namespace JsonMasher
{
    public interface IJsonMasher
    {
        IEnumerable<Json> Mash(Json json);
        IEnumerable<Json> Mash(IEnumerable<Json> seq);
    }
}
