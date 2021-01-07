using System.Collections.Generic;
using System.Linq;
using JsonMasher.Compiler;
using JsonMasher.JsonRepresentation;

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
            MashContext _context = MashContext.CreateContext(tickLimit, sourceInformation, stack);
            return (
                sequence: seq.SelectMany(json => op.Mash(json, _context)),
                context: _context);
        }
    }
}
