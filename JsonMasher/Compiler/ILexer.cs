using System;
using System.Collections.Generic;

namespace JsonMasher.Compiler
{
    public interface ILexer
    {
        public IEnumerable<Token> Tokenize(string program);
    }
}
