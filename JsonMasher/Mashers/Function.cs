using System.Collections.Generic;

namespace JsonMasher.Mashers
{
    public record Callable();
    public record Thunk(IJsonMasherOperator Op): Callable;
    public record Function(IJsonMasherOperator Op, List<string> Arguments): Callable;
}
