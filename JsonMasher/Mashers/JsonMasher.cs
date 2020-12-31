using System.Collections.Generic;
using System.Linq;
using JsonMasher.Compiler;
using JsonMasher.Mashers.Builtins;
using JsonMasher.Mashers.Combinators;

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
            MashContext _context = new() {
                SourceInformation = sourceInformation,
                TickLimit = tickLimit
            };
            _context.PushEnvironmentFrame();
            _context.SetCallable(new FunctionName("not", 0), Not.Builtin);
            _context.SetCallable(new FunctionName("empty", 0), Empty.Builtin);
            _context.SetCallable(new FunctionName("range", 1), Range.Builtin_1);
            _context.SetCallable(new FunctionName("range", 2), Range.Builtin_2);
            _context.SetCallable(new FunctionName("range", 3), Range.Builtin_3);
            _context.SetCallable(new FunctionName("length", 0), Length.Builtin);
            _context.SetCallable(new FunctionName("limit", 2), Limit.Builtin);
            return (sequence: seq.SelectMany(json => op.Mash(json, _context, stack)), context: _context);
        }
    }
}
