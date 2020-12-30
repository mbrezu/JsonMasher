using System;

namespace JsonMasher.Compiler
{
    public class LexerException : Exception
    {
        public int Line { get; private set; }
        public int Column { get; private set; }
        public string Highlights { get; private set; }

        public LexerException(
            string message,
            int line,
            int column,
            string highlights,
            Exception innerException = null)
            : base(message, innerException)
        {
            Line = line;
            Column = column;
            Highlights = highlights;
        }
    }
}
