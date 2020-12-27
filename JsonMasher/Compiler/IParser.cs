using JsonMasher.Mashers;

namespace JsonMasher.Compiler
{
    public interface IParser
    {
        IJsonMasherOperator Parse(string program);
    }
}
