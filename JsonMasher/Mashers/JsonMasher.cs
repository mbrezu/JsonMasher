using System.Collections.Generic;
using System.Linq;
using JsonMasher.Mashers.Builtins;

namespace JsonMasher.Mashers
{
    public class JsonMasher : IJsonMasher
    {
        public (IEnumerable<Json> sequence, IMashContext context) Mash(IEnumerable<Json> seq, IJsonMasherOperator op)
        {
            MashContext _context = new();
            _context.PushEnvironmentFrame();
            _context.SetCallable("not", Not.Builtin);
            _context.SetCallable("empty", Empty.Builtin);
            _context.SetCallable("range", Range.Builtin);
            return (sequence: seq.SelectMany(json => op.Mash(json, _context)), context: _context);
        }
    }
}
