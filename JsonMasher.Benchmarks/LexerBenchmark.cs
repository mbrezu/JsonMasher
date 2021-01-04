using System.Linq;
using JsonMasher.Compiler;

namespace JsonMasher.Benchmarks
{
    public class LexerBenchmark
    {
        private string _program;
        private Lexer _lexer;
        public LexerBenchmark()
        {
            _program += ". == test def null true false\n";
            _program += "def map(x): [.[] | x;]";
            _program += "def select(x): if x then . else empty end;";
            _lexer = new();
        }

        public void TokenizeProgram()
        {
            _lexer.Tokenize(_program).ToList();
        }
    }
}
