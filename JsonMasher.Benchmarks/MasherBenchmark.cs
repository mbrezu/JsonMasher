using System.Linq;
using JsonMasher.Compiler;
using JsonMasher.Mashers;

namespace JsonMasher.Benchmarks
{
    public class MasherBenchmark
    {
        private string _program = @"range(1000000) | . + 2 | empty";
        private IJsonMasherOperator _filter;
        private Mashers.JsonMasher _masher;
        private SourceInformation _sourceInformation;

        public MasherBenchmark()
        {
            (_filter, _sourceInformation) = new Parser().Parse(_program, new SequenceGenerator());
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
