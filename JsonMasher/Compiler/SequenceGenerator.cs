namespace JsonMasher.Compiler
{
    public class SequenceGenerator : ISequenceGenerator
    {
        private int _current;

        public string GetValue() => $"_{_current}";

        public void Next() => _current++;
    }
}