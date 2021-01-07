using System.Linq;
using JsonMasher.Compiler;
using JsonMasher.Mashers;

namespace JsonMasher.Benchmarks
{
    public class RecurseMasherBenchmark
    {
        private string _program = @"[range(2000)] | map([range(.)]) | recurse";
        private IJsonMasherOperator _filter;
        private Mashers.JsonMasher _masher;
        private SourceInformation _sourceInformation;

        public RecurseMasherBenchmark()
        {
            (_filter, _sourceInformation) = new Parser().Parse(_program);
            _masher = new();
        }

        public void MashWithoutDebug()
        {
            var (sequence, _) = _masher.Mash(
                "null".AsJson().AsEnumerable(),
                _filter,
                DefaultMashStack.Instance,
                _sourceInformation);
            sequence.ToList();
        }
        public void MashWithDebug()
        {
            var (sequence, _) = _masher.Mash(
                "null".AsJson().AsEnumerable(),
                _filter,
                new DebugMashStack(),
                _sourceInformation);
            sequence.ToList();
        }
    }
}
