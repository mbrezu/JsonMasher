using System.Collections.Generic;

namespace JsonMasher.Mashers.Combinators
{
    public class Let : IJsonMasherOperator
    {
        public string Name { get; init; }
        public IJsonMasherOperator Value { get; init; }
        public IJsonMasherOperator Body { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            context.PushEnvironmentFrame();
            var newStack = stack.Push(this);
            foreach (var jsonValue in Value.Mash(json, context, newStack))
            {
                context.SetVariable(Name, jsonValue);
                foreach (var result in Body.Mash(json, context, newStack))
                {
                    yield return result;
                }
            }
            context.PopEnvironmentFrame();
        }
    }
}
