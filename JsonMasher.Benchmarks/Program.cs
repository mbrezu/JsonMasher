using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace JsonMasher.Benchmarks
{
    public class Benchmarks 
    {
        LexerBenchmark _lexerBenchmark;
        ParserBenchmark _parserBenchmark;
        MasherBenchmark _masherBenchmark;

        public Benchmarks()
        {
            _lexerBenchmark = new();
            _parserBenchmark = new();
            _masherBenchmark = new();
        }

        [Benchmark]
        public void Lexer()
        {
            _lexerBenchmark.TokenizeProgram();
        }

        [Benchmark]
        public void Parser()
        {
            _parserBenchmark.ParseProgram();
        }

        [Benchmark]
        public void MasherWithoutDebug()
        {
            _masherBenchmark.MashWithoutDebug();
        }

        [Benchmark]
        public void MasherWithDebug()
        {
            _masherBenchmark.MashWithDebug();
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<Benchmarks>();
        }
    }
}
