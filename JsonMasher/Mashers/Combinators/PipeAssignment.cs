using System.Collections.Generic;

namespace JsonMasher.Mashers.Combinators
{
    public class PipeAssignment : IJsonMasherOperator
    {
        public IJsonMasherOperator PathExpression { get; init; }
        public IJsonMasherOperator Masher { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            var newStack = stack.Push(this);
            context.Tick(stack);
            yield return json;
        }
    }
}
