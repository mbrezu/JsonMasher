using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Mashers.Combinators
{
    public class IfThenElse : IJsonMasherOperator
    {
        public IJsonMasherOperator Cond { get; init; }
        public IJsonMasherOperator Then { get; init; }
        public IJsonMasherOperator Else { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            var newStack = stack.Push(this);
            context.Tick(stack);
            var condSequence = Cond.Mash(json, context, newStack);
            foreach (var condValue in condSequence)
            {
                if (condValue.Type != JsonValueType.True && condValue.Type != JsonValueType.False)
                {
                    throw context.Error(
                        $"If condition value must be a boolean, but found {condValue.Type}.",
                        newStack.Push(Cond), condValue);
                }
                var resultSequence = condValue.GetBool()
                    ? Then.Mash(json, context, newStack)
                    : Else.Mash(json, context, newStack);
                foreach (var result in resultSequence)
                {
                    yield return result;
                }
            }
        }
    }
}
