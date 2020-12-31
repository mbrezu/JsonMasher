using System;
using System.Collections.Generic;

namespace JsonMasher.Compiler
{
    public class JsonMasherException : Exception
    {
        public int Line { get; private set; }
        public int Column { get; private set; }
        public string Highlights { get; private set; }
        public IEnumerable<Json> Values { get; private set; }

        public JsonMasherException(
            string message,
            Exception innerException = null)
            : base(message, innerException)
        {
        }

        public JsonMasherException(
            string message,
            int line,
            int column,
            string highlights,
            Exception innerException = null,
            params Json[] values)
            : this(message, innerException)
        {
            Line = line;
            Column = column;
            Highlights = highlights;
            Values = values;
        }
    }
}
