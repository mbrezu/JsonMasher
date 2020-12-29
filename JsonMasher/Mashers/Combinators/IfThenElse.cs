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

        public IEnumerable<Json> Mash(Json json, IMashContext context)
        {
            var condSequence = Cond.Mash(json, context);
            if (!condSequence.Any())
            {
                throw new InvalidOperationException();
            }
            foreach (var condValue in condSequence)
            {
                if (condValue.Type != JsonValueType.True && condValue.Type != JsonValueType.False)
                {
                    throw new InvalidOperationException();
                }
                var resultSequence = condValue.GetBool()
                    ? Then.Mash(json, context)
                    : Else.Mash(json, context);
                foreach (var result in resultSequence)
                {
                    yield return result;
                }
            }
        }
    }
}
