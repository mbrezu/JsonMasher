using System.Linq;
using JsonMasher.Compiler;
using JsonMasher.Mashers;

namespace JsonMasher.Benchmarks
{
    public class MasherBenchmark
    {
        private string _program;
        private IJsonMasherOperator _filter;
        private Mashers.JsonMasher _masher;
        private SourceInformation _sourceInformation;

        public MasherBenchmark(string program)
        {
            _program = program;
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
