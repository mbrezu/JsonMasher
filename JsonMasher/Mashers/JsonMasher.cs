using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Mashers
{
    public class JsonMasher : IJsonMasher
    {
        MashContext _context = new();

        public IMashContext Context => _context;

        public IEnumerable<Json> Mash(IEnumerable<Json> seq, IJsonMasherOperator op)
            => seq.SelectMany(json => op.Mash(json, _context));
    }
}
