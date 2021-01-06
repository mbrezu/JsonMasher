using System.Collections.Generic;
using System.Linq;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Combinators
{
    public class ReduceForeach : IJsonMasherOperator
    {
        public string Name { get; set; }
        public IJsonMasherOperator Inputs { get; init; }
        public IJsonMasherOperator Initial { get; init; }
        public IJsonMasherOperator Update { get; init; }

        public bool IsForeach { get; init; }
        public IJsonMasherOperator Extract { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            context.PushEnvironmentFrame();
            var newStack = stack.Push(this);
            context.Tick(newStack);
            var result = Initial.Mash(json, context, newStack).FirstOrDefault() ?? Json.Null;
            foreach (var value in Inputs.Mash(json, context, newStack))
            {
                context.SetVariable(Name, value);
                result = Update.Mash(result, context, newStack).LastOrDefault() ?? Json.Null;
                if (IsForeach)
                {
                    if (Extract != null)
                    {
                        foreach (var extractedValue in Extract.Mash(result, context, newStack))
                        {
                            yield return extractedValue;
                        }
                    }
                    else
                    {
                        yield return result;
                    }
                }
            }
            context.PopEnvironmentFrame();
            if (!IsForeach) {
                yield return result;
            }
        }
    }
}
