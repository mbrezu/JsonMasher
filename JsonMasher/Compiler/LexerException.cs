using System;

namespace JsonMasher.Compiler
{
    public class LexerException : Exception
    {
        public int Position { get; private set; }
        public string Highlights { get; private set; }

        public LexerException(string message, int position, string highlights)
            : base(message)
        {
            Position = position;
            Highlights = highlights;
        }
    }
}
