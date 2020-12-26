using System.Collections.Generic;

namespace JsonMasher
{
    public class MashContext : IMashContext
    {
        List<Json> _log = new();

        public void LogValue(Json value)
        {
            _log.Add(value);
        }

        public IEnumerable<Json> Log => _log;
    }
}
