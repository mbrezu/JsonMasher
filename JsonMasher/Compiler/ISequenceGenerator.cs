namespace JsonMasher.Compiler
{
    public interface ISequenceGenerator
    {
        string GetValue();
        void Next();
    }
}