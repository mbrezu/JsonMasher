using System.Collections.Generic;
using System.Linq;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Combinators
{
    public class Reduce : IJsonMasherOperator
    {
        public string Name { get; set; }
        public IJsonMasherOperator Inputs { get; set; }
        public IJsonMasherOperator Initial { get; set; }
        public IJsonMasherOperator Update { get; set; }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            context.PushEnvironmentFrame();
            var newStack = stack.Push(this);
            context.Tick(newStack);
            var result = Initial.Mash(json, context, stack).FirstOrDefault() ?? Json.Null;
            foreach (var value in Inputs.Mash(json, context, stack))
            {
                context.SetVariable(Name, value);
                result = Update.Mash(result, context, stack).LastOrDefault() ?? Json.Null;
            }
            context.PopEnvironmentFrame();
            yield return result;
        }
    }
}
