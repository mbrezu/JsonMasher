using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace JsonMasher.Benchmarks
{
    public class Benchmarks 
    {
        LexerBenchmark _lexerBenchmark;
        ParserBenchmark _parserBenchmark;
        MasherBenchmark _masherBenchmarkRange;
        MasherBenchmark _masherBenchmarkRecurse;
        MasherBenchmark _masherBenchmarkReduce;

        public Benchmarks()
        {
            _lexerBenchmark = new();
            _parserBenchmark = new();
            _masherBenchmarkRange = new(@"range(1000000) | . + 2 | empty");
            _masherBenchmarkRecurse = new(@"[range(2000)] | map([range(.)]) | recurse | empty");
            _masherBenchmarkReduce = new(@"[range(1000)] | map([range(.)]) | flatten | length");
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
        public void RangeMasherWithoutDebug()
        {
            _masherBenchmarkRange.MashWithoutDebug();
        }

        [Benchmark]
        public void RangeMasherWithDebug()
        {
            _masherBenchmarkRange.MashWithDebug();
        }

        [Benchmark]

        public void RecurseMasherWithoutDebug()
        {
            _masherBenchmarkRecurse.MashWithoutDebug();
        }

        [Benchmark]
        public void RecurseMasherWithDebug()
        {
            _masherBenchmarkRecurse.MashWithDebug();
        }

        [Benchmark]
        public void ReduceMasherWithoutDebug()
        {
            _masherBenchmarkReduce.MashWithoutDebug();
        }

        [Benchmark]
        public void ReduceMasherWithDebug()
        {
            _masherBenchmarkReduce.MashWithDebug();
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
