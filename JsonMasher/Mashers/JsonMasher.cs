using System.Collections.Generic;
using System.Linq;
using JsonMasher.Compiler;

namespace JsonMasher.Mashers
{
    public class JsonMasher
    {
        public (IEnumerable<Json> sequence, IMashContext context) Mash(
            IEnumerable<Json> seq,
            IJsonMasherOperator op,
            IMashStack stack,
            SourceInformation sourceInformation = null,
            int tickLimit = 0)
        {
            MashContext _context = new()
            {
                SourceInformation = sourceInformation,
                TickLimit = tickLimit
            };
            return (
                sequence: seq.SelectMany(json => op.Mash(json, _context, stack)),
                context: _context);
        }
    }
}
