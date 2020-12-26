using System.Collections.Generic;

namespace JsonMasher
{
    public class JsonMasher : IJsonMasher
    {
        MashContext _context = new();

        public IMashContext Context => _context;

        public IEnumerable<Json> Mash(IEnumerable<Json> seq, IJsonMasherOperator op)
        {
            return op.Mash(seq, _context);
        }
    }
}
