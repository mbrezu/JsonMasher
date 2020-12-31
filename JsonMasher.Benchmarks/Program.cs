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
        public void Masher()
        {
            _masherBenchmark.Mash();
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
