using System;

namespace JsonMasher.Compiler
{
    public class JsonMasherException : Exception
    {
        public int Line { get; private set; }
        public int Column { get; private set; }
        public string Highlights { get; private set; }

        public JsonMasherException(
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
