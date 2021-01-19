namespace JsonMasher.Compiler
{
    public class JsonBreakException : JsonMasherException
    {
        public string Label { get; init; }
        public JsonBreakException(string label): base($"break on ${label}")
        {
            Label = label;
        }
    }
}
