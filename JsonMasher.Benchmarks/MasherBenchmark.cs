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

        public MasherBenchmark()
        {
            var (filter, _) = new Parser().Parse(_program);
            _filter = filter;
            _masher = new();
        }

        public void Mash()
        {
            var (sequence, _) = _masher.Mash(
                "null".AsJson().AsEnumerable(), _filter, DefaultMashStack.Instance);
            sequence.ToList();
        }
    }
}
