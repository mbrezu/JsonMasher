using System.Collections.Generic;

namespace JsonMasher.Mashers
{
    public interface Callable {}
    public record Function(IJsonMasherOperator Op, List<string> Arguments): Callable;
}
