using System.Collections.Generic;

namespace JsonMasher
{
    public interface IMashContext
    {
        void LogValue(Json value);
        IEnumerable<Json> Log { get; }
    }
}
