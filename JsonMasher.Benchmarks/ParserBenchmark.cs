using JsonMasher.Compiler;

namespace JsonMasher.Benchmarks
{
    public class ParserBenchmark
    {
        private string _program;
        private Parser _parser;
        public ParserBenchmark()
        {
            _program = "def map(x): [.[] | x];";
            _program += "def select(x): if x then . else empty end;";
            _parser = new Parser();
        }

        public void ParseProgram()
        {
            _parser.Parse(_program);
        }
    }
}
