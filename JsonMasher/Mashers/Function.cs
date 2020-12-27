using System.Collections.Generic;

namespace JsonMasher.Mashers
{
    public interface Callable {}
    public record Thunk(IJsonMasherOperator Op): Callable;
    public record Function(IJsonMasherOperator Op, List<string> Arguments): Callable;
}
